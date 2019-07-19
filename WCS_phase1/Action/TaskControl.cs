﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Models;
using System.Data;
using WCS_phase1.Functions;
using WCS_phase1.Devices;
using WCS_phase1.Socket;
using System.Configuration;
using System.Threading;

namespace WCS_phase1.Action
{
    class TaskControl
    {
        MySQL mySQL = new MySQL();
        SimpleTools tools = new SimpleTools();
        TaskTools task = new TaskTools();
        DataControl dataControl = new DataControl();


        #region 初步入库任务
        /// <summary>
        /// 执行WCS入库清单（初步执行）
        /// </summary>
        public void Run_InInitial()
        {
            try
            {
                // 获取可执行的入库清单
                DataTable dtcommand = mySQL.SelectAll("select * from wcs_command_v where TASK_TYPE = '1' and STEP = '2' order by CREATION_TIME");
                if (tools.IsNoData(dtcommand))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList = dtcommand.ToDataList<WCS_COMMAND_V>();
                // 遍历执行入库任务
                foreach (WCS_COMMAND_V com in comList)
                {
                    Task_InInitial(com);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行入库任务（初步执行）
        /// </summary>
        /// <param name="command"></param>
        public void Task_InInitial(WCS_COMMAND_V command)
        {
            try
            {
                // 选择目的货位最靠外的货物优先处理
                String loc1 = task.GetRGVLoc(2, command.LOC_TO_1); //运输车辊台②任务1
                String loc2 = task.GetRGVLoc(1, command.LOC_TO_2); //运输车辊台①任务2
                String loc = task.GetLocByRgvToLoc(loc1, loc2); //处理货位
                if (loc == "NG")
                {
                    //不能没有货物目的位置
                    return;
                }

                // 摆渡车到固定辊台对接点
                String ARFloc = task.GetARFLoc(command.FRT);    //获取对应摆渡车位置
                task.CreateItem(command.WCS_NO, ItemId.摆渡车定位固定辊台, ARFloc);  //生成摆渡车任务

                // 运输车到摆渡车对接点
                task.CreateItem(command.WCS_NO, ItemId.运输车复位1, ConfigurationManager.AppSettings["StandbyR1"]);  //生成运输车任务

                // 行车到运输车对接取货点
                String ABCloc = task.GetABCTrackLoc(loc);     //获取对应行车位置
                task.CreateItem(command.WCS_NO, ItemId.行车轨道定位, ABCloc);     //生成行车任务

                //更新WCS COMMAND状态——执行中
                task.UpdateCommand(command.WCS_NO, CommandStep.执行中);
                //更新WCS TASK状态——任务中
                task.UpdateTaskByWCSNo(command.WCS_NO, TaskSite.任务中);
            }
            catch (Exception ex)
            {
                //初始化
                task.UpdateCommand(command.WCS_NO, CommandStep.请求执行);
                task.UpdateTaskByWCSNo(command.WCS_NO, TaskSite.未执行);
                task.DeleteItem(command.WCS_NO, "");
                throw ex;
            }
        }
        #endregion


        #region 可持续任务
        /// <summary>
        /// 执行判断是否需要后续作业
        /// </summary>
        public void Run_TaskContinued()
        {
            try
            {
                // 以wcs_no为单位提取最后一笔对接任务
                DataTable dtlast = mySQL.SelectAll(@"select * from WCS_TASK_ITEM where LEFT(ITEM_ID,2) = '11' and (WCS_NO,CREATION_TIME) in 
                                                    (select WCS_NO, MAX(CREATION_TIME) from WCS_TASK_ITEM group by WCS_NO) order by CREATION_TIME");
                if (tools.IsNoData(dtlast))
                {
                    return;
                }
                List<WCS_TASK_ITEM> lastList = dtlast.ToDataList<WCS_TASK_ITEM>();
                // 遍历后续判断作业
                foreach (WCS_TASK_ITEM last in lastList)
                {
                    Task_Continued(last);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 后续任务作业
        /// </summary>
        /// <param name="item"></param>
        public void Task_Continued(WCS_TASK_ITEM item)
        {
            try
            {
                // 任务目的比对检测
                CheckTask(item.WCS_NO, item.LOC_TO);

                // 清单是[结束]状态不作业
                if (task.GetCommandStep(item.WCS_NO) == CommandStep.结束)
                {
                    return;
                }

                // Item非[完成]状态不作业
                if (item.STATUS != ItemStatus.完成任务)
                {
                    return;
                }

                // 依出入库类型处理
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   //入库
                        ProcessInTask(item);
                        break;
                    case "O":   //出库
                        ProcessOutTask(item);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 检查比对任务目的
        /// </summary>
        /// <param name="id"></param>
        /// <param name="wcs_no"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public void CheckTask(String wcs_no, String loc)
        {
            try
            {
                // 获取对应清单
                String sql = String.Format(@"select TASK_TYPE,FRT,TASK_UID_1,LOC_TO_1,TASK_UID_2,LOC_TO_2 from wcs_command_v where STEP <>'4' and WCS_NO = '{0}'", wcs_no);
                DataTable dt = mySQL.SelectAll(sql);
                if (tools.IsNoData(dt))
                {
                    return;
                }
                String tasktype = dt.Rows[0]["TASK_TYPE"].ToString();
                String frt = dt.Rows[0]["FRT"].ToString();
                String taskid1 = dt.Rows[0]["TASK_UID_1"].ToString();
                String loc1 = dt.Rows[0]["LOC_TO_1"].ToString();
                String taskid2 = dt.Rows[0]["TASK_UID_2"].ToString();
                String loc2 = dt.Rows[0]["LOC_TO_2"].ToString();
                // 判断目的位置是否一致
                // 更新Task状态(Command于数据库TRIGGER 'update_command_T' 触发更新)
                switch (tasktype)
                {
                    case TaskType.入库:
                        if (loc == task.GetABCStockLoc(loc1))
                        {
                            task.UpdateTask(taskid1, TaskSite.完成);
                        }
                        if (loc == task.GetABCStockLoc(loc2))
                        {
                            task.UpdateTask(taskid2, TaskSite.完成);
                        }
                        break;
                    case TaskType.出库:
                        if (loc == frt)
                        {
                            task.UpdateTask(taskid1, TaskSite.完成);
                            task.UpdateTask(taskid2, TaskSite.完成);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 入库任务处理
        /// </summary>
        /// <param name="item"></param>
        public void ProcessInTask(WCS_TASK_ITEM item)
        {
            try
            {
                // 摆渡车于运输车对接点
                String AR = ConfigurationManager.AppSettings["StandbyAR"];
                // 运输车[内]待命复位点
                String R = ConfigurationManager.AppSettings["StandbyR2"];
                // 运输车于运输车对接点
                String RR = ConfigurationManager.AppSettings["StandbyRR"];

                // 获取对应清单
                String sql = String.Format(@"select TASK_UID_1,LOC_TO_1,SITE_1, TASK_UID_2,LOC_TO_2,SITE_2 from wcs_command_v where WCS_NO = '{0}'", item.WCS_NO);
                DataTable dt = mySQL.SelectAll(sql);
                if (tools.IsNoData(dt))
                {
                    return;
                }
                String taskid1 = dt.Rows[0]["TASK_UID_1"].ToString(); //任务1
                String taskid2 = dt.Rows[0]["TASK_UID_2"].ToString(); //任务2
                String site1 = dt.Rows[0]["SITE_1"].ToString(); //状态1
                String site2 = dt.Rows[0]["SITE_2"].ToString(); //状态2
                String loc1 = dt.Rows[0]["LOC_TO_1"].ToString(); //目标点1
                String loc2 = dt.Rows[0]["LOC_TO_2"].ToString(); //目标点2

                // 默认入库时 taskid1对接运输车设备辊台②、taskid2对接运输车设备辊台①
                String loc_1 = task.GetRGVLoc(2, loc1); //辊台②任务1
                String loc_2 = task.GetRGVLoc(1, loc2); //辊台①任务2
                String loc; //执行目标

                switch (item.ITEM_ID)   //根据最后的设备指令，可得货物已在流程中该设备对接的下一设备处
                {
                    case ItemId.固定辊台入库: //目的设备为对接的摆渡车，可直接加以分配
                        #region 将摆渡车移至运输车对接位置
                        // 可断定货物需移至运输车
                        // 生成摆渡车任务
                        task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车定位运输车对接, item.LOC_TO, "", AR, ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.摆渡车入库:  //目的设备为对接的运输车，可直接加以分配
                        #region 将运输车移至行车对接位置 || 运输车间对接
                        // 根据货物目的地判断是否需要运输车对接运输车
                        loc = task.GetLocByRgvToLoc(loc_1, loc_2);
                        if (loc == "NG")
                        {
                            //不能没有货物目的位置
                            break;
                        }
                        // 判断是否需要对接到运输车[内]范围内作业
                        if (Convert.ToInt32(loc) >= Convert.ToInt32(R))  // 需对接运输车[内]
                        {
                            // 生成运输车[内]复位任务
                            task.CreateItem(item.WCS_NO, ItemId.运输车复位2, R);    // 待分配设备
                            // 生成运输车[外]对接位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车对接定位, item.LOC_TO, "", RR, ItemStatus.请求执行);
                        }
                        else
                        {
                            // 生成运输车[外]定位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, item.LOC_TO, "", loc, ItemStatus.请求执行);
                        }
                        #endregion

                        break;
                    case ItemId.运输车入库:  //目的设备为对接的运输车，可直接加以分配
                        #region 将运输车移至行车对接位置
                        // 判断是否作业过运输车定位对接行车任务
                        String sqlrr = String.Format(@"select * from wcs_task_item where ITEM_ID = '033' and STATUS not in ('E','X') and WCS_NO = '{0}'", item.WCS_NO);
                        DataTable dtrr = mySQL.SelectAll(sqlrr);
                        if (tools.IsNoData(dt))
                        {
                            loc = task.GetLocByRgvToLoc(loc_1, loc_2);
                            if (loc == "NG")
                            {
                                //不能没有货物目的位置
                                break;
                            }
                            // 生成运输车定位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, item.LOC_TO, "", loc, ItemStatus.请求执行);
                        }
                        #endregion

                        break;
                    case ItemId.行车取货:
                        #region 将运输车移至行车对接位置 && 行车定位
                        // 默认入库时 taskid1对接运输车设备辊台②、taskid2对接运输车设备辊台①
                        // 获取当前运输车加以分配
                        String rgv = task.GetItemDeviceLast(item.WCS_NO, ItemId.运输车定位);
                        // 获取当前运输车资讯
                        RGV RGV = new RGV(rgv);
                        // 获取有货&无货辊台各对应的WMS任务目标
                        String loc_Y = "";  //有货辊台对应目标点
                        String loc_N = "";  //无货辊台对应目标点

                        // 根据当前运输车坐标及任务目标，生成对应运输车定位/对接运输车任务
                        if (RGV.GoodsStatus() == RGV.GoodsYes1) // 辊台①有货
                        {
                            loc_Y = loc_2;
                            loc_N = loc_1;
                        }
                        else if (RGV.GoodsStatus() == RGV.GoodsYes2) // 辊台②有货
                        {
                            loc_Y = loc_1;
                            loc_N = loc_2;
                        }
                        else
                        {
                            break;
                        }
                        // 判断是否需要对接到运输车[内]范围内作业
                        if (Convert.ToInt32(loc_Y) >= Convert.ToInt32(R))  // 需对接运输车[内]
                        {
                            // 生成运输车[内]复位任务
                            task.CreateItem(item.WCS_NO, ItemId.运输车复位2, R);    // 待分配设备
                                                                               // 生成运输车[外]对接位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车对接定位, rgv, "", RR, ItemStatus.请求执行);
                        }
                        else
                        {
                            // 生成运输车[外]定位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, rgv, "", loc_Y, ItemStatus.请求执行);
                        }
                        // 生成行车库存定位任务
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车库存定位, item.DEVICE, "", task.GetABCStockLoc(loc_N), ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.行车放货:
                        #region 行车定位
                        // 未完成的任务目标点
                        loc = site1 == "Y" ? loc2 : loc1;
                        // 行车到运输车对接取货点
                        String ABCloc = task.GetABCTrackLoc(loc); //获取对应行车位置
                        // 生成行车轨道定位任务
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车轨道定位, item.DEVICE, "", ABCloc, ItemStatus.请求执行);
                        #endregion

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 出库任务处理
        /// </summary>
        /// <param name="item"></param>
        public void ProcessOutTask(WCS_TASK_ITEM item)
        {
            try
            {
                // 运输车
                String rgv;
                RGV RGV;
                // 摆渡车于运输车对接点
                String AR = ConfigurationManager.AppSettings["StandbyAR"];
                // 运输车[外]待命复位点
                String R1 = ConfigurationManager.AppSettings["StandbyR1"];
                // 运输车[内]待命复位点
                String R2 = ConfigurationManager.AppSettings["StandbyR2"];
                // 运输车于运输车对接点
                String RR = ConfigurationManager.AppSettings["StandbyRR"];

                // 获取对应清单
                String sql = String.Format(@"select FRT, TASK_UID_1,LOC_FROM_1,SITE_1, TASK_UID_2,LOC_FROM_2,SITE_2 from wcs_command_v where WCS_NO = '{0}'", item.WCS_NO);
                DataTable dt = mySQL.SelectAll(sql);
                if (tools.IsNoData(dt))
                {
                    return;
                }
                String frt = dt.Rows[0]["FRT"].ToString(); // 锁定固定辊台
                String taskid1 = dt.Rows[0]["TASK_UID_1"].ToString(); //任务1
                String taskid2 = dt.Rows[0]["TASK_UID_2"].ToString(); //任务2
                String site1 = dt.Rows[0]["SITE_1"].ToString(); //状态1
                String site2 = dt.Rows[0]["SITE_2"].ToString(); //状态2
                String loc1 = dt.Rows[0]["LOC_FROM_1"].ToString(); //来源点1
                String loc2 = dt.Rows[0]["LOC_FROM_2"].ToString(); //来源点2

                // 默认出库时 taskid1对接运输车设备辊台①、taskid2对接运输车设备辊台②
                String loc_1 = task.GetRGVLoc(1, loc1); //辊台①任务1
                String loc_2 = task.GetRGVLoc(2, loc2); //辊台②任务2

                switch (item.ITEM_ID)   //根据最后的设备指令，可得货物已在流程中该设备对接的下一设备处
                {
                    case ItemId.行车取货:
                        #region 行车轨道定位与运输车对接点
                        // 生成行车库存定位任务
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车轨道定位, item.DEVICE, "", task.GetABCTrackLoc(item.LOC_TO), ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.行车放货:
                        #region 将运输车复位待命点 / 将运输车移至行车对接位置 && 行车库存定位
                        // 获取当前运输车加以分配
                        rgv = task.GetItemDeviceLast(item.WCS_NO, ItemId.运输车定位);
                        // 获取当前运输车资讯
                        RGV = new RGV(rgv);

                        // 流程上设定是先分配运输车辊台①放置货物，即现确认运输车辊台②是否有任务/有货物
                        // 车辊台②不存在任务直接复位 ｜｜　所有辊台都有货
                        if (String.IsNullOrEmpty(taskid2) || RGV.GoodsStatus() == RGV.GoodsYesAll)
                        {
                            // 将运输车复位待命点
                            // => 当前设备位置 >= 运输车[内]待命复位点 ?  运输车[内]复位 ：运输车[外]复位
                            if (RGV.GetCurrentSite() >= Convert.ToInt32(R2))
                            {
                                // 生成运输车[内]复位任务
                                task.CreateCustomItem(item.WCS_NO, ItemId.运输车复位2, rgv, "", R2, ItemStatus.请求执行);
                                // 生成运输车[外]对接任务
                                task.CreateItem(item.WCS_NO, ItemId.运输车对接定位, RR);    // 待分配设备
                            }
                            else
                            {
                                // 生成运输车[外]复位任务
                                task.CreateCustomItem(item.WCS_NO, ItemId.运输车复位1, rgv, "", R1, ItemStatus.请求执行);
                            }
                        }
                        else if (RGV.GoodsStatus() == RGV.GoodsYes1) //执行辊台②任务2
                        {
                            // 将运输车移至行车对接位置 && 行车库存定位
                            // 生成运输车定位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, rgv, "", loc_2, ItemStatus.请求执行);
                            // 生成行车库存定位任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.行车库存定位, item.DEVICE, "", task.GetABCTrackLoc(loc2), ItemStatus.请求执行);
                        }

                        #endregion

                        break;
                    case ItemId.运输车出库:
                        #region 将摆渡车移至固定辊台对接点 / 将运输车移至摆渡车对接点(复位待命点1)
                        // 获取当前对接任务的目的设备类型
                        String type = task.GetDeviceType(item.LOC_TO);
                        if (type == DeviceType.运输车)
                        {
                            // 即执行完运输车间对接作业，按流程需将货物移至摆渡车
                            // 生成将运输车[外]复位待命点1任务
                            task.CreateCustomItem(item.WCS_NO, ItemId.运输车复位1, item.LOC_TO, "", R1, ItemStatus.请求执行);
                        }
                        else if (type == DeviceType.摆渡车)
                        {
                            // 即执行完运输车&摆渡车对接作业，按流程需将货物移至固定辊台
                            // 生成将摆渡车移至固定辊台对接点任务
                            String ARFloc = task.GetARFLoc(frt);    //获取对应摆渡车位置
                            task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车定位固定辊台, item.LOC_TO, "", ARFloc, ItemStatus.请求执行);
                        }
                        #endregion

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region 接轨出库任务
        /// <summary>
        /// 执行入库完成后操作出库作业
        /// </summary>
        public void Run_OutFollow()
        {
            try
            {
                // 获取所有作业区域
                DataTable dtarea = mySQL.SelectAll("select distinct AREA from wcs_config_device");
                if (tools.IsNoData(dtarea))
                {
                    return;
                }
                // 遍历判断作业区域情况
                foreach (DataRow dr in dtarea.Rows)
                {
                    String area = dr[0].ToString();

                    // 判断当前区域是否存在出库请求
                    int taskcount = mySQL.GetCount("wcs_task_info", String.Format(@"TASK_TYPE = '2' and SITE = 'N' and W_D_LOC = '{0}'", area));
                    if (taskcount == 0) //无则退出
                    {
                        continue;
                    }

                    // 判断当前区域是否存在执行中的任务
                    int commandcount = mySQL.GetCount("wcs_command_master", String.Format(@"STEP = '3' and FRT in
                                                   (select distinct DEVICE from wcs_config_device where TYPE = 'FRT' and AREA = '{0}')", area));
                    if (commandcount > 0) //有则退出
                    {
                        continue;
                    }

                    // 判断当前区域是否存在满足入库条件的任务
                    int incount = mySQL.GetCount("wcs_command_v", String.Format(@"TASK_TYPE = '1' and STEP = '2' and FRT in
                                                  (select distinct DEVICE from wcs_config_device where TYPE = 'FRT' and AREA = '{0}')", area)); 
                    if (incount > 0) //有则退出
                    {
                        continue;
                    }

                    // 处理该区域出库任务
                    Task_OutFollow(area);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行出库任务（初步执行）
        /// </summary>
        /// <param name="abc"></param>
        public void Task_OutFollow(String area)
        {
            try
            {
                // 获取空闲的行车资讯
                String sql = String.Format(@"select * from wcs_config_device where TYPE = 'ABC' and AREA = '{0}'", area);
                DataTable dtabc = mySQL.SelectAll(sql);
                if (tools.IsNoData(dtabc))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> abcList = dtabc.ToDataList<WCS_CONFIG_DEVICE>();

                // 行车中间值
                int AA = Convert.ToInt32(ConfigurationManager.AppSettings["CenterX"]);

                // 遍历确认出库任务
                foreach (WCS_CONFIG_DEVICE abc in abcList)
                {
                    // 获取当前行车资讯
                    ABC ABC = new ABC(abc.DEVICE);
                    // 获取当前坐标X轴值
                    int X = tools.bytesToInt(ABC.CurrentXsite(), 0);
                    // 任务
                    String[] task;
                    // 对比当前坐标X轴值与行车中间值
                    if (X <= AA)
                    {
                        // 仅处理货物来源位置于行车[外]运输范围内合适的出库任务
                        task = GetSuitableTask(1, X);
                    }
                    else
                    {
                        // 仅处理货物来源位置于行车[内]运输范围内的出库任务
                        task = GetSuitableTask(2, X);
                    }

                    // 生成 WCS 出库清单及 ITEM 初始任务
                    if (task == null)
                    {
                        continue;
                    }
                    CreateOutJob(task[0].ToString(), task[1].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取合适的出库任务UID [range：靠外范围=1；靠内范围=2]
        /// </summary>
        /// <param name="range"></param>
        /// <param name="X"></param>
        /// <returns></returns>
        public String[] GetSuitableTask(int range, int X)
        {
            try
            {
                // 行车中间值
                int AA = Convert.ToInt32(ConfigurationManager.AppSettings["CenterX"]);
                // 大小运算符号
                String sign;
                if (range ==1)
                {
                    sign = "<=";
                }
                else if (range == 2)
                {
                    sign = ">";
                }
                else
                {
                    return null;
                }

                // 获取X轴值与当前行车X轴值差值最小的任务UID
                String sql = String.Format(@"select (case when t.X > {0} then (t.X - {0})
						 when t.X < {0} then ({0} - t.X)
			       else t.X end) as loc , t.TASK_UID
  from (select a.TASK_UID, (b.STOCK_X + 0) X
          from (select distinct TASK_UID, W_S_LOC from wcs_task_info where TASK_TYPE = '2' and SITE = 'N') a, 
	             (select distinct WMS_LOC, SUBSTRING_INDEX(ABC_LOC_STOCK,'-',1) STOCK_X from wcs_config_loc) b
         where a.W_S_LOC = b.WMS_LOC and (b.STOCK_X + 0) {1} {2}
       ) t order by loc", X, sign, AA);
                DataTable dt = mySQL.SelectAll(sql);
                if (tools.IsNoData(dt))
                {
                    return null;
                }
                // 确认取范围内任务 1~2 个
                String t1 = "";
                String t2 = "";
                if (dt.Rows.Count < 2)
                {
                    t1 = dt.Rows[0]["TASK_UID"].ToString();
                }
                else
                {
                    t1 = dt.Rows[0]["TASK_UID"].ToString();
                    t2 = dt.Rows[1]["TASK_UID"].ToString();
                }
                return new String[] { t1, t2 };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 生成 WCS 出库相关作业
        /// </summary>
        /// <param name="taskuid_1"></param>
        /// <param name="taskuid_2"></param>
        public void CreateOutJob(String taskuid_1, String taskuid_2)
        {
            //生成 WCS 清单号
            Thread.Sleep(1000);
            String wcs_no = "O" + System.DateTime.Now.ToString("yyMMddHHmmss");
            try
            {
                // =>生成 COMMAND

                // 生成 ITEM
                // 生成行车库存定位任务
                String ABCloc = ""; //获取对应库存位置
                task.CreateItem(wcs_no, ItemId.行车库存定位, ABCloc);  //生成行车任务

                // 生成运输车对接行车任务
                String RGVloc = ""; //获取运输车对接行车位置
                task.CreateItem(wcs_no, ItemId.运输车定位, RGVloc);  //生成运输车任务

                // 生成摆渡车对接运输车任务
                task.CreateItem(wcs_no, ItemId.摆渡车定位运输车对接, ConfigurationManager.AppSettings["StandbyAR"]);  //生成摆渡车任务

                //更新WCS COMMAND状态——执行中
                task.UpdateCommand(wcs_no, CommandStep.执行中);
                //更新WCS TASK状态——任务中
                task.UpdateTaskByWCSNo(wcs_no, TaskSite.任务中);

            }
            catch (Exception ex)
            {
                //初始化
                task.DeleteCommand(wcs_no);
                task.UpdateTaskByWCSNo(wcs_no, TaskSite.未执行);
                task.DeleteItem(wcs_no, "");
                throw ex;
            }
        }

        #endregion




        #region 分配设备
        /// <summary>
        /// 执行分配设备至各个任务
        /// </summary>
        public void Run_ItemDevice()
        {
            try
            {
                // 获取待分配设备任务
                DataTable dtitem = mySQL.SelectAll("select * from WCS_TASK_ITEM where STATUS = 'N' and DEVICE is null order by CREATION_TIME");
                if (tools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历分配设备
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    ReadDevice(item);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 分配任务设备
        /// </summary>
        /// <param name="item"></param>
        public void ReadDevice(WCS_TASK_ITEM item)
        {
            try
            {
                // 获取任务所在固定辊台
                String frt = task.GetFRTByWCSNo(item.WCS_NO);
                if (String.IsNullOrEmpty(frt))
                {
                    return;
                }

                // 获取任务所在作业区域
                String area = task.GetArea(frt);
                if (String.IsNullOrEmpty(frt))
                {
                    return;
                }

                // 分配设备
                String device = String.Empty;
                // 当前位置
                String loc = String.Empty;
                switch (item.ITEM_ID.Substring(0, 2))
                {
                    case "01":
                        #region 摆渡车

                        // 获取作业区域内的摆渡车
                        List<WCS_CONFIG_DEVICE> dList_ARF = task.GetDeviceList(area, DeviceType.摆渡车);
                        // 确认其中最适合的摆渡车
                        String arf = GetSuitableARF(item.LOC_TO, dList_ARF);
                        if (String.IsNullOrEmpty(arf))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        ARF ARF = new ARF(arf);
                        device = Convert.ToString(ARF.CurrentSite());
                        loc = arf;

                        #endregion

                        break;
                    case "02":
                        #region 运输车

                        // 获取作业区域内的运输车
                        List<WCS_CONFIG_DEVICE> dList_RGV = task.GetDeviceList(area, DeviceType.运输车);
                        // 确认其中最适合的摆渡车
                        String rgv = GetSuitableRGV(item.LOC_TO, dList_RGV);
                        if (String.IsNullOrEmpty(rgv))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        RGV RGV = new RGV(rgv);
                        device = Convert.ToString(RGV.GetCurrentSite());
                        loc = rgv;

                        #endregion

                        break;
                    case "03":
                        #region 行车

                        // 获取作业区域内的行车
                        List<WCS_CONFIG_DEVICE> dList_ABC = task.GetDeviceList(area, DeviceType.行车);
                        // 确认其中最适合的行车
                        String abc = GetSuitableABC(item.LOC_TO, dList_ABC);
                        if (String.IsNullOrEmpty(abc))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        ABC ABC = new ABC(abc);
                        device = ABC.GetCurrentSite();
                        loc = abc;

                        #endregion

                        break;
                    default:
                        break;
                }

                if (String.IsNullOrEmpty(device) || String.IsNullOrEmpty(loc))
                {
                    return;
                }
                // 确认设备是否锁定
                if (task.IsDeviceLock(device))
                {
                    return;
                }
                // 确认任务设备
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.设备编号, device);
                // 确认设备来源
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.来源位置, loc);
                // 更新状态
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.请求执行);
                // 锁定设备
                task.DeviceLock(device);
            }
            catch (Exception ex)
            {
                //初始化
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.不可执行);
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前目标对应的合适摆渡车
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public String GetSuitableARF(String loc, List<WCS_CONFIG_DEVICE> dList)
        {
            try
            {
                // 摆渡车
                ARF ARF;
                // 终选摆渡车
                String arf = String.Empty;
                // 距离比较值
                byte X = 0;
                // 目标位置
                int LOC = Convert.ToInt32(loc);

                // 遍历比对
                foreach (WCS_CONFIG_DEVICE d in dList)
                {
                    // 获取摆渡车设备资讯
                    ARF = new ARF(d.DEVICE);

                    // 当前坐标值比较目标位置
                    if (ARF.CurrentSite() == LOC)    // 当前坐标值 = 目标位置
                    {
                        // 直接选取该设备
                        arf = d.DEVICE;
                        break;
                    }
                    else if (ARF.CurrentSite() < LOC) // 当前坐标值 < 目标位置
                    {
                        // 距离比较值 >=（目标位置 - 当前坐标值）
                        if (X >= (LOC - ARF.CurrentSite()))
                        {
                            // 暂选取该设备
                            arf = d.DEVICE;
                            // 更新距离比较值
                            X = (byte)(LOC - ARF.CurrentSite());
                        }
                    }
                    else // 当前坐标值 > 目标位置
                    {
                        // 距离比较值 >=（当前坐标值 - 目标位置）
                        if (X >= (ARF.CurrentSite() - LOC))
                        {
                            // 暂选取该设备
                            arf = d.DEVICE;
                            // 更新距离比较值
                            X = (byte)(ARF.CurrentSite() - LOC);
                        }
                    }
                    ARF = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(arf))
                {
                    ARF = new ARF(arf);
                    // 命令状态不为完成, 货物状态不为无货 ==> 不可用
                    if (ARF.CommandStatus() != ARF.CommandFinish && ARF.GoodsStatus() != ARF.GoodsNoAll && ARF.CurrentStatus() != ARF.RollerStop)
                    {
                        arf = String.Empty;
                    }
                }

                return arf;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前目标对应的合适运输车
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public String GetSuitableRGV(String loc, List<WCS_CONFIG_DEVICE> dList)
        {
            try
            {
                // 运输车
                RGV RGV;
                // 终选运输车
                String rgv = String.Empty;
                // 目标位置
                int LOC = Convert.ToInt32(loc);
                // 运输车间对接点
                int RR = Convert.ToInt32(ConfigurationManager.AppSettings["StandbyRR"]);

                // 遍历比对
                foreach (WCS_CONFIG_DEVICE d in dList)
                {
                    // 获取摆渡车设备资讯
                    RGV = new RGV(d.DEVICE);

                    // 比较目标位置与对接点
                    if (LOC <= RR)
                    {
                        // 仅获取位置于运输车[外]运输范围内的 RGV
                        if (RGV.GetCurrentSite() <= RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }
                    else
                    {
                        // 仅获取位置于运输车[内]运输范围内的 RGV
                        if (RGV.GetCurrentSite() > RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }

                    RGV = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(rgv))
                {
                    RGV = new RGV(rgv);
                    // 命令状态不为完成, 货物状态不为无货, 辊台状态不为停止 ==> 不可用
                    if (RGV.CommandStatus() != RGV.CommandFinish && RGV.GoodsStatus() != RGV.GoodsNoAll && RGV.CurrentStatus() != RGV.RollerStop)
                    {
                        rgv = String.Empty;
                    }
                }

                return rgv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前目标对应的合适行车
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public String GetSuitableABC(String loc, List<WCS_CONFIG_DEVICE> dList)
        {
            try
            {
                // 行车
                ABC ABC;
                // 终选行车
                String abc = String.Empty;
                // 行车中间值
                int AA = Convert.ToInt32(ConfigurationManager.AppSettings["CenterX"]);
                // 目标位置 X轴值
                String[] LOC = loc.Split('-');
                int X = Convert.ToInt32(LOC[0]);

                // 遍历比对
                foreach (WCS_CONFIG_DEVICE d in dList)
                {
                    // 获取行车设备资讯
                    ABC = new ABC(d.DEVICE);

                    // 比较目标X轴值与对接点
                    if (X <= AA)
                    {
                        // 仅获取位置于行车[外]运输范围内的 ABC
                        if (tools.bytesToInt(ABC.CurrentXsite(), 0) <= AA)
                        {
                            // 锁定设备
                            abc = d.DEVICE;
                            break;
                        }
                    }
                    else
                    {
                        // 仅获取位置于行车[内]运输范围内的 ABC
                        if (tools.bytesToInt(ABC.CurrentXsite(), 0) > AA)
                        {
                            // 锁定设备
                            abc = d.DEVICE;
                            break;
                        }
                    }

                    ABC = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(abc))
                {
                    ABC = new ABC(abc);
                    // 命令状态不为完成, 货物状态不为无货 ==> 不可用
                    if (ABC.CommandStatus() != ABC.CommandFinish && ABC.GoodsStatus() != ABC.GoodsNo)
                    {
                        abc = String.Empty;
                    }
                }

                return abc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region 下发指令(除对接任务)
        /// <summary>
        /// 执行设备通讯下发指令(除对接任务)
        /// </summary>
        public void Run_SendOrderNotLink()
        {
            try
            {
                // 获取请求执行的任务
                DataTable dtitem = mySQL.SelectAll("select * from WCS_TASK_ITEM where STATUS = 'Q' and LEFT(ITEM_ID,2) <> '11' order by CREATION_TIME");
                if (tools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历下发指令
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    WriteDeviceNotLink(item);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 下发设备指令(除对接任务)
        /// </summary>
        /// <param name="item"></param>
        public void WriteDeviceNotLink(WCS_TASK_ITEM item)
        {
            try
            {
                // =>组合资讯，下发指令

                //更新状态
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.任务中);
            }
            catch (Exception ex)
            {
                //初始化
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.请求执行);
                throw ex;
            }
        }

        #endregion


        #region 设备对接
        /// <summary>
        /// 执行设备到位进行对接作业 [对接任务排程触发下发指令]
        /// </summary>
        public void Run_LinkDevice()
        {
            try
            {
                #region 固定辊台 <==> 摆渡车
                // 获取已完成对接阶段的摆渡车任务
                List<WCS_TASK_ITEM> itemList_ARF = task.GetItemList_R(ItemId.摆渡车定位固定辊台);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_ARF in itemList_ARF)
                {
                    CreateTask_ARF_FRT(item_ARF);
                }
                #endregion

                #region 摆渡车 <==> 运输车
                // 获取已完成对接阶段的摆渡车任务
                List<WCS_TASK_ITEM> itemList_A = task.GetItemList_R(ItemId.摆渡车定位运输车对接);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_A in itemList_A)
                {
                    CreateTask_ARF_RGV(item_A);
                }
                #endregion

                #region 运输车 <==> 运输车
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_R = task.GetItemList_R(ItemId.运输车对接定位);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_R in itemList_R)
                {
                    CreateTask_RGV_RGV(item_R);
                }
                #endregion

                #region 运输车 <==> 行车
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_ABC = task.GetItemList_R(ItemId.行车轨道定位);
                // 遍历生成夹具取放任务
                foreach (WCS_TASK_ITEM item_ABC in itemList_ABC)
                {
                    CreateTask_RGV_ABC(item_ABC);
                }
                #endregion

                #region 行车 <==> 库存货位
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_LOC = task.GetItemList_R(ItemId.行车库存定位);
                // 遍历生成夹具取放任务
                foreach (WCS_TASK_ITEM item_LOC in itemList_LOC)
                {
                    CreateTask_ABC(item_LOC);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建摆渡车&固定辊台对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_ARF_FRT(WCS_TASK_ITEM item)
        {
            try
            {
                // 判断是出入库类型
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   // 入库  固定辊台 (货物)==> 摆渡车
                        // 先动摆渡车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车入库, item.DEVICE, "", "", ItemStatus.请求执行);
                        // 后动固定辊台滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.固定辊台入库, task.GetFRTDevice(item.LOC_TO), "", item.DEVICE, ItemStatus.请求执行); //入库目的为摆渡车
                        break;
                    case "O":   // 出库  摆渡车 (货物)==> 固定辊台
                        // 先动固定辊台滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.固定辊台出库, task.GetFRTDevice(item.LOC_TO), "", "", ItemStatus.请求执行);
                        // 后动摆渡车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车出库, item.DEVICE, "", task.GetFRTDevice(item.LOC_TO), ItemStatus.请求执行); //出库目的为固定辊台
                        break;
                    default:
                        break;
                }
                //摆渡车初始任务更新状态——完成
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                task.DeleteItem(item.WCS_NO, ItemId.固定辊台入库);
                task.DeleteItem(item.WCS_NO, ItemId.固定辊台出库);
                task.DeleteItem(item.WCS_NO, ItemId.摆渡车入库);
                task.DeleteItem(item.WCS_NO, ItemId.摆渡车出库);
                throw ex;
            }
        }

        /// <summary>
        /// 创建摆渡车&运输车对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_ARF_RGV(WCS_TASK_ITEM item)
        {
            int id_R = 0;
            String wcsno_R = "";
            String itemid_R = "";
            String device_R = "";
            try
            {
                // 查看运输车是否到位
                String sql_R = String.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = 'R' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车复位1);
                DataTable dtitem_R = mySQL.SelectAll(sql_R);
                if (tools.IsNoData(dtitem_R))
                {
                    return;
                }
                id_R = Convert.ToInt32(dtitem_R.Rows[0]["ID"].ToString());
                wcsno_R = dtitem_R.Rows[0]["WCS_NO"].ToString();
                itemid_R = dtitem_R.Rows[0]["ITEM_ID"].ToString();
                device_R = dtitem_R.Rows[0]["DEVICE"].ToString();
                // 判断是出入库类型
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   // 入库  摆渡车 (货物)==> 运输车
                        // 先动运输车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车入库, device_R, "", "", ItemStatus.请求执行);
                        // 后动摆渡车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车入库, item.DEVICE, "", device_R, ItemStatus.请求执行); //入库目的为运输车
                        break;
                    case "O":   // 出库  运输车 (货物)==> 摆渡车
                        // 先动摆渡车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.摆渡车出库, item.DEVICE, "", "", ItemStatus.请求执行);
                        // 后动运输车滚棒
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车出库, device_R, "", item.DEVICE, ItemStatus.请求执行); //出库目的为摆渡车
                        break;
                    default:
                        break;
                }
                //摆渡车&运输车初始任务更新状态——完成
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                task.DeleteItem(item.WCS_NO, ItemId.运输车入库);
                task.DeleteItem(item.WCS_NO, ItemId.摆渡车入库);
                task.DeleteItem(item.WCS_NO, ItemId.摆渡车出库);
                task.DeleteItem(item.WCS_NO, ItemId.运输车出库);
                throw ex;
            }
        }

        /// <summary>
        /// 创建运输车&运输车对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_RGV_RGV(WCS_TASK_ITEM item)
        {
            int id_R = 0;
            String wcsno_R = "";
            String itemid_R = "";
            String device_R = "";
            try
            {
                // 查看运输车是否在运输车对接待命点
                String sql_R = String.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = 'R' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车复位2);
                DataTable dtitem_R = mySQL.SelectAll(sql_R);
                if (tools.IsNoData(dtitem_R))
                {
                    return;
                }
                id_R = Convert.ToInt32(dtitem_R.Rows[0]["ID"].ToString());
                wcsno_R = dtitem_R.Rows[0]["WCS_NO"].ToString();
                itemid_R = dtitem_R.Rows[0]["ITEM_ID"].ToString();
                device_R = dtitem_R.Rows[0]["DEVICE"].ToString();
                // 判断是出入库类型
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   // 入库  运输车[外] (货物)==> 运输车[内]
                        // 先动运输车滚棒[内]
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车入库, device_R, "", "", ItemStatus.请求执行);
                        // 后动运输车滚棒[外]
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车入库, item.DEVICE, "", device_R, ItemStatus.请求执行); //入库目的为运输车[内]
                        break;
                    case "O":   // 出库  运输车[内] (货物)==> 运输车[外]
                        // 先动运输车滚棒[外]
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车出库, item.DEVICE, "", "", ItemStatus.请求执行);
                        // 后动运输车滚棒[内]
                        task.CreateCustomItem(item.WCS_NO, ItemId.运输车出库, device_R, "", item.DEVICE, ItemStatus.请求执行);    //出库目的为运输车[外]
                        break;
                    default:
                        break;
                }
                //内外运输车初始任务更新状态——完成
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                task.DeleteItem(item.WCS_NO, ItemId.运输车入库);
                task.DeleteItem(item.WCS_NO, ItemId.运输车出库);
                throw ex;
            }
        }

        /// <summary>
        /// 创建运输车&行车对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_RGV_ABC(WCS_TASK_ITEM item)
        {
            int id_R = 0;
            String wcsno_R = "";
            String itemid_R = "";
            String device_R = "";
            try
            {
                // 查看运输车是否在运输车对接待命点
                String sql_R = String.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = 'R' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车定位);
                DataTable dtitem_R = mySQL.SelectAll(sql_R);
                if (tools.IsNoData(dtitem_R))
                {
                    return;
                }
                id_R = Convert.ToInt32(dtitem_R.Rows[0]["ID"].ToString());
                wcsno_R = dtitem_R.Rows[0]["WCS_NO"].ToString();
                itemid_R = dtitem_R.Rows[0]["ITEM_ID"].ToString();
                device_R = dtitem_R.Rows[0]["DEVICE"].ToString();
                // 判断是出入库类型
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   // 入库  运输车 (货物)==> 行车 
                        // 行车取货
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车取货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    case "O":   // 出库  行车 (货物)==> 运输车 
                        // 行车放货
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车放货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    default:
                        break;
                }
                //行车&运输车初始任务更新状态——完成
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                task.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                task.DeleteItem(item.WCS_NO, ItemId.行车取货);
                task.DeleteItem(item.WCS_NO, ItemId.行车放货);
                throw ex;
            }
        }

        /// <summary>
        /// 创建行车出入库取放货 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_ABC(WCS_TASK_ITEM item)
        {
            try
            {
                // 判断是出入库类型
                switch (item.WCS_NO.Substring(0, 1))
                {
                    case "I":   // 入库  行车 (货物)==> 货位 
                        // 行车放货
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车放货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    case "O":   // 出库  货位 (货物)==> 行车
                        // 行车取货
                        task.CreateCustomItem(item.WCS_NO, ItemId.行车取货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    default:
                        break;
                }
                //行车&运输车初始任务更新状态——完成
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                task.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                task.DeleteItem(item.WCS_NO, ItemId.行车取货);
                task.DeleteItem(item.WCS_NO, ItemId.行车放货);
                throw ex;
            }
        }
        #endregion


        #region 下发指令(对接任务)
        /// <summary>
        /// 执行设备通讯下发指令(对接任务)
        /// </summary>
        public void Run_SendOrderOnlyLink()
        {
            try
            {
                // 以wcs_no为单位提取任务
                // 依时间先后顺序下发指令
                // 循环获取设备状态
                // 前任务完成再启动后任务
                // 更新任务状态
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
