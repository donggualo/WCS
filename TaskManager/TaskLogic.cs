using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using TaskManager.Devices;
using ModuleManager.WCS;
using PubResourceManager;

namespace TaskManager
{
    /// <summary>
    /// <summary>
    /// 任务中心
    /// </summary>
    public class TaskLogic
    {
        #region 入库呼叫WMS分配库位

        /// <summary>
        /// 执行单托入库库位/双托入库库位分配
        /// </summary>
        public void Run_InCallWMS()
        {
            try
            {
                // 获取单托任务清单
                String sql1 = String.Format(@"select * from wcs_command_v where TASK_UID_1 is not null and TASK_UID_2 is null and TASK_TYPE = '{0}' and STEP = '{1}' 
                    and TRUNCATE((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(CREATION_TIME))/60,0) >= {2}", TaskType.入库, CommandStep.生成单号, DataControl._mStools.GetValueByKey("InTimeMax")); // 等待时间
                DataTable dtcommand1 = DataControl._mMySql.SelectAll(sql1);
                if (DataControl._mStools.IsNoData(dtcommand1))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList1 = dtcommand1.ToDataList<WCS_COMMAND_V>();
                // 遍历执行单托任务请求WMS分配库位
                foreach (WCS_COMMAND_V com1 in comList1)
                {
                }

                // 获取双托任务清单
                String sql2 = String.Format(@"select * from wcs_command_v where TASK_UID_1 is not null and TASK_UID_2 is not null and  TASK_TYPE = '{0}' and STEP = '{1}' order by CREATION_TIME",
                    TaskType.入库, CommandStep.生成单号);
                DataTable dtcommand2 = DataControl._mMySql.SelectAll(sql2);
                if (DataControl._mStools.IsNoData(dtcommand2))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList2 = dtcommand2.ToDataList<WCS_COMMAND_V>();
                // 遍历执行双托任务请求WMS分配库位
                foreach (WCS_COMMAND_V com2 in comList2)
                {
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 初步入库任务

        /// <summary>
        /// 执行WCS入库清单（初步执行）
        /// </summary>
        public void Run_InInitial()
        {
            try
            {
                // 获取可执行的入库清单
                String sql = String.Format(@"select * from wcs_command_v where TASK_TYPE = '{0}' and STEP = '{1}' order by CREATION_TIME", TaskType.入库, CommandStep.请求执行);
                DataTable dtcommand = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtcommand))
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
                String loc1 = DataControl._mTaskTools.GetRGVLoc(2, command.LOC_TO_1); //运输车辊台②任务1
                String loc2 = DataControl._mTaskTools.GetRGVLoc(1, command.LOC_TO_2); //运输车辊台①任务2
                String loc = DataControl._mTaskTools.GetLocByRgvToLoc(loc1, loc2); //处理货位
                if (loc == "NG")
                {
                    //不能没有货物目的位置
                    return;
                }

                // 摆渡车到固定辊台对接点
                String ARFloc = DataControl._mTaskTools.GetARFLoc(command.FRT);    //获取对应摆渡车位置
                DataControl._mTaskTools.CreateItem(command.WCS_NO, ItemId.摆渡车定位固定辊台, ARFloc);  //生成摆渡车任务

                // 运输车到摆渡车对接点
                DataControl._mTaskTools.CreateItem(command.WCS_NO, ItemId.运输车复位1, DataControl._mStools.GetValueByKey("StandbyR1"));  //生成运输车任务

                // 行车到运输车对接取货点
                String ABCloc = DataControl._mTaskTools.GetABCTrackLoc(loc == loc1 ? command.LOC_TO_1 : command.LOC_TO_2);     //获取对应行车位置
                DataControl._mTaskTools.CreateItem(command.WCS_NO, ItemId.行车轨道定位, ABCloc);     //生成行车任务

                //更新WCS COMMAND状态——执行中
                DataControl._mTaskTools.UpdateCommand(command.WCS_NO, CommandStep.执行中);
                //更新WCS TASK状态——任务中
                DataControl._mTaskTools.UpdateTaskByWCSNo(command.WCS_NO, TaskSite.任务中);
            }
            catch (Exception ex)
            {
                // 初始化
                DataControl._mTaskTools.UpdateCommand(command.WCS_NO, CommandStep.请求执行);
                DataControl._mTaskTools.UpdateTaskByWCSNo(command.WCS_NO, TaskSite.未执行);
                DataControl._mTaskTools.DeleteItem(command.WCS_NO, "");
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("Task_InInitial()", "生成初始入库任务", command.WCS_NO, null, ex.ToString());
            }
        }
        #endregion

        #region 设备对接任务

        /// <summary>
        /// 执行设备到位进行对接作业
        /// </summary>
        public void Run_LinkDevice()
        {
            try
            {
                #region 固定辊台 <==> 摆渡车
                // 获取已完成对接阶段的摆渡车任务
                List<WCS_TASK_ITEM> itemList_ARF = DataControl._mTaskTools.GetItemList_R(ItemId.摆渡车定位固定辊台);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_ARF in itemList_ARF)
                {
                    CreateTask_ARF_FRT(item_ARF);
                }
                #endregion

                #region 摆渡车 <==> 运输车
                // 获取已完成对接阶段的摆渡车任务
                List<WCS_TASK_ITEM> itemList_A = DataControl._mTaskTools.GetItemList_R(ItemId.摆渡车定位运输车对接);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_A in itemList_A)
                {
                    CreateTask_ARF_RGV(item_A);
                }
                #endregion

                #region 运输车 <==> 运输车
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_R = DataControl._mTaskTools.GetItemList_R(ItemId.运输车对接定位);
                // 遍历生成滚棒任务
                foreach (WCS_TASK_ITEM item_R in itemList_R)
                {
                    CreateTask_RGV_RGV(item_R);
                }
                #endregion

                #region 运输车 <==> 行车
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_ABC = DataControl._mTaskTools.GetItemList_R(ItemId.行车轨道定位);
                // 遍历生成夹具取放任务
                foreach (WCS_TASK_ITEM item_ABC in itemList_ABC)
                {
                    CreateTask_RGV_ABC(item_ABC);
                }
                #endregion

                #region 行车 <==> 库存货位
                // 获取已完成对接阶段的运输车任务
                List<WCS_TASK_ITEM> itemList_LOC = DataControl._mTaskTools.GetItemList_R(ItemId.行车库存定位);
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
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车正向, item.DEVICE, DataControl._mTaskTools.GetFRTDevice(item.LOC_TO), "", ItemStatus.请求执行); //入库来源为固定辊台
                        // 后动固定辊台滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.固定辊台正向, DataControl._mTaskTools.GetFRTDevice(item.LOC_TO), "", item.DEVICE, ItemStatus.请求执行); //入库目的为摆渡车
                        break;
                    case "O":   // 出库  摆渡车 (货物)==> 固定辊台
                        // 先动固定辊台滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.固定辊台反向, DataControl._mTaskTools.GetFRTDevice(item.LOC_TO), item.DEVICE, "", ItemStatus.请求执行); //出库来源为摆渡车
                        // 后动摆渡车滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车反向, item.DEVICE, "", DataControl._mTaskTools.GetFRTDevice(item.LOC_TO), ItemStatus.请求执行); //出库目的为固定辊台
                        break;
                    default:
                        break;
                }
                //摆渡车初始任务更新状态——完成
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.固定辊台正向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.固定辊台反向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.摆渡车正向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.摆渡车反向);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateTask_ARF_FRT()", "生成摆渡车&固定辊台对接任务", item.WCS_NO, null, ex.ToString());
            }
        }

        /// <summary>
        /// 创建摆渡车&运输车对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_ARF_RGV(WCS_TASK_ITEM item)
        {
            int id_R = 0;
            string wcsno_R = "";
            string itemid_R = "";
            string device_R = "";
            try
            {
                // 查看运输车是否到位
                string sql_R = string.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = '{2}' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车复位1, ItemStatus.交接中);
                DataTable dtitem_R = DataControl._mMySql.SelectAll(sql_R);
                if (DataControl._mStools.IsNoData(dtitem_R))
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
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车正向, device_R, item.DEVICE, "", ItemStatus.请求执行); //入库来源为摆渡车
                        // 后动摆渡车滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车正向, item.DEVICE, "", device_R, ItemStatus.请求执行); //入库目的为运输车
                        break;
                    case "O":   // 出库  运输车 (货物)==> 摆渡车
                        // 先动摆渡车滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车反向, item.DEVICE, device_R, "", ItemStatus.请求执行); //出库来源为运输车
                        // 后动运输车滚棒
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车反向, device_R, "", item.DEVICE, ItemStatus.请求执行); //出库目的为摆渡车
                        break;
                    default:
                        break;
                }
                //摆渡车&运输车初始任务更新状态——完成
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.运输车正向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.摆渡车正向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.摆渡车反向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.运输车反向);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateTask_ARF_RGV()", "生成摆渡车&运输车对接任务", item.WCS_NO, null, ex.ToString());
            }
        }

        /// <summary>
        /// 创建运输车&运输车对接出入库 ITEM 任务
        /// </summary>
        /// <param name="item"></param>
        public void CreateTask_RGV_RGV(WCS_TASK_ITEM item)
        {
            int id_R = 0;
            string wcsno_R = "";
            string itemid_R = "";
            string device_R = "";
            try
            {
                // 查看运输车是否在运输车对接待命点
                string sql_R = string.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = '{2}' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车复位2, ItemStatus.交接中);
                DataTable dtitem_R = DataControl._mMySql.SelectAll(sql_R);
                if (DataControl._mStools.IsNoData(dtitem_R))
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
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车正向, device_R, item.DEVICE, "", ItemStatus.请求执行); //入库来源为运输车[外]
                        // 后动运输车滚棒[外]
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车正向, item.DEVICE, "", device_R, ItemStatus.请求执行); //入库目的为运输车[内]
                        break;
                    case "O":   // 出库  运输车[内] (货物)==> 运输车[外]
                        // 先动运输车滚棒[外]
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车反向, item.DEVICE, device_R, "", ItemStatus.请求执行); //出库来源为运输车[内]
                        // 后动运输车滚棒[内]
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车反向, device_R, "", item.DEVICE, ItemStatus.请求执行); //出库目的为运输车[外]
                        break;
                    default:
                        break;
                }
                //内外运输车初始任务更新状态——完成
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.运输车正向);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.运输车反向);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateTask_RGV_RGV()", "生成运输车&运输车对接任务", item.WCS_NO, null, ex.ToString());
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
                String sql_R = String.Format(@"select ID, WCS_NO, ITEM_ID, DEVICE from WCS_TASK_ITEM where STATUS = '{2}' and WCS_NO = '{0}' and ITEM_ID = '{1}'", item.WCS_NO, ItemId.运输车定位, ItemStatus.交接中);
                DataTable dtitem_R = DataControl._mMySql.SelectAll(sql_R);
                if (DataControl._mStools.IsNoData(dtitem_R))
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
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车取货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    case "O":   // 出库  行车 (货物)==> 运输车 
                        // 行车放货
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车放货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    default:
                        break;
                }
                //行车&运输车初始任务更新状态——完成
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.UpdateItem(id_R, wcsno_R, itemid_R, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.行车取货);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.行车放货);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateTask_RGV_ABC()", "生成运输车&行车对接任务", item.WCS_NO, null, ex.ToString());
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
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车放货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    case "O":   // 出库  货位 (货物)==> 行车
                        // 行车取货
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车取货, item.DEVICE, "", item.LOC_TO, ItemStatus.请求执行);
                        break;
                    default:
                        break;
                }
                //行车&运输车初始任务更新状态——完成
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
            }
            catch (Exception ex)
            {
                //恢复
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.行车取货);
                DataControl._mTaskTools.DeleteItem(item.WCS_NO, ItemId.行车放货);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateTask_ABC()", "生成行车取放货任务", item.WCS_NO, null, ex.ToString());
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
                DataTable dtlast = DataControl._mMySql.SelectAll(@"select * from WCS_TASK_ITEM where LEFT(ITEM_ID,2) = '11' and (WCS_NO,ID) in 
                                                    (select WCS_NO, MAX(ID) from WCS_TASK_ITEM group by WCS_NO) order by CREATION_TIME");
                if (DataControl._mStools.IsNoData(dtlast))
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
                // Item非[完成]状态 => 不作业
                if (item.STATUS != ItemStatus.完成任务)
                {
                    return;
                }

                // 当ITEM任务为出入库最后流程时
                if (item.ITEM_ID == ItemId.行车放货 || item.ITEM_ID == ItemId.摆渡车反向)
                {
                    // 目的位置比对检测是否抵达——>完成任务
                    CheckTask(item.WCS_NO, item.LOC_TO);
                }

                // 清单是[结束]状态 => 不作业
                if (DataControl._mTaskTools.GetCommandStep(item.WCS_NO) == CommandStep.结束)
                {
                    // 备份任务数据
                    DataControl._mTaskTools.BackupTask(item.WCS_NO);
                    // 解锁对应清单所有设备数据状态
                    DataControl._mTaskTools.DeviceUnLock(item.WCS_NO);
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
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("Task_Continued()", "后续任务判断实施", item.WCS_NO, null, ex.ToString());
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
                String sql = String.Format(@"select * from wcs_command_v where STEP <>'{1}' and WCS_NO = '{0}'", wcs_no, CommandStep.结束);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                WCS_COMMAND_V command = dt.ToDataEntity<WCS_COMMAND_V>();

                // 判断目的位置是否一致
                // 更新Task状态(Command于数据库TRIGGER 'update_command_T' 触发更新)
                switch (command.TASK_TYPE)
                {
                    case TaskType.入库:
                        if (loc == DataControl._mTaskTools.GetABCStockLoc(command.LOC_TO_1))
                        {
                            // 更新清单完成
                            DataControl._mTaskTools.UpdateTask(command.TASK_UID_1, TaskSite.完成);
                            // 通知WMS完成
                            DataControl._mHttp.DoStockInFinishTask(command.LOC_TO_1, command.TASK_UID_1);
                        }

                        if (loc == DataControl._mTaskTools.GetABCStockLoc(command.LOC_TO_2))
                        {
                            // 更新清单完成
                            DataControl._mTaskTools.UpdateTask(command.TASK_UID_2, TaskSite.完成);
                            // 通知WMS完成
                            DataControl._mHttp.DoStockInFinishTask(command.LOC_TO_2, command.TASK_UID_2);
                        }

                        break;
                    case TaskType.出库:
                        if (loc == command.FRT)
                        {
                            // 更新清单完成
                            DataControl._mTaskTools.UpdateTask(command.TASK_UID_1, TaskSite.完成);
                            DataControl._mTaskTools.UpdateTask(command.TASK_UID_2, TaskSite.完成);
                            // 通知WMS完成
                            DataControl._mHttp.DoStockOutFinishTask(command.LOC_TO_1, command.TASK_UID_1);
                            DataControl._mHttp.DoStockOutFinishTask(command.LOC_TO_2, command.TASK_UID_2);
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
                String AR = DataControl._mStools.GetValueByKey("StandbyAR");
                // 运输车[内]待命复位点
                String R = DataControl._mStools.GetValueByKey("StandbyR2");
                // 运输车于运输车对接点
                String RR = DataControl._mStools.GetValueByKey("StandbyRR");

                // 获取对应清单
                String sql = String.Format(@"select * from wcs_command_v where WCS_NO = '{0}'", item.WCS_NO);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                WCS_COMMAND_V command = dt.ToDataEntity<WCS_COMMAND_V>();

                // 默认入库时 taskid1对接运输车设备辊台②、taskid2对接运输车设备辊台①
                String loc_1 = DataControl._mTaskTools.GetRGVLoc(2, command.LOC_TO_1); //辊台②任务1
                String loc_2 = DataControl._mTaskTools.GetRGVLoc(1, command.LOC_TO_2); //辊台①任务2
                String loc; //执行目标

                switch (item.ITEM_ID)   //根据最后的设备指令，可得货物已在流程中该设备对接的下一设备处
                {
                    case ItemId.固定辊台正向: //目的设备为对接的摆渡车，可直接加以分配
                        #region 将摆渡车移至运输车对接位置
                        // 可断定货物需移至运输车
                        // 生成摆渡车任务
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车定位运输车对接, item.LOC_TO, "", AR, ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.摆渡车正向:  //目的设备为对接的运输车，可直接加以分配
                        #region 将运输车移至行车对接位置 || 运输车间对接
                        // 根据货物目的地判断是否需要运输车对接运输车
                        loc = DataControl._mTaskTools.GetLocByRgvToLoc(loc_1, loc_2);
                        if (loc == "NG")
                        {
                            //不能没有货物目的位置
                            break;
                        }
                        // 判断是否需要对接到运输车[内]范围内作业
                        if (Convert.ToInt32(loc) >= Convert.ToInt32(R))  // 需对接运输车[内]
                        {
                            // 生成运输车[内]复位任务
                            DataControl._mTaskTools.CreateItem(item.WCS_NO, ItemId.运输车复位2, R);    // 待分配设备
                            // 生成运输车[外]对接位任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车对接定位, item.LOC_TO, "", RR, ItemStatus.请求执行);
                        }
                        else
                        {
                            // 生成运输车[外]定位任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, item.LOC_TO, "", loc, ItemStatus.请求执行);
                        }
                        #endregion

                        break;
                    case ItemId.运输车正向:  //目的设备为对接的运输车，可直接加以分配
                        #region 将运输车移至行车对接位置
                        // 判断是否作业过运输车定位对接行车任务
                        String sqlrr = String.Format(@"select * from wcs_task_item where ITEM_ID = '{3}' and STATUS not in ('{1}','{2}') and WCS_NO = '{0}'", item.WCS_NO, ItemStatus.出现异常, ItemStatus.失效, ItemId.行车取货);
                        DataTable dtrr = DataControl._mMySql.SelectAll(sqlrr);
                        if (DataControl._mStools.IsNoData(dt))
                        {
                            loc = DataControl._mTaskTools.GetLocByRgvToLoc(loc_1, loc_2);
                            if (loc == "NG")
                            {
                                //不能没有货物目的位置
                                break;
                            }
                            // 生成运输车定位任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, item.LOC_TO, "", loc, ItemStatus.请求执行);
                        }
                        #endregion

                        break;
                    case ItemId.行车取货:
                        #region 将运输车移至行车对接位置 && 行车定位
                        // 默认入库时 taskid1对接运输车设备辊台②、taskid2对接运输车设备辊台①
                        // 获取当前运输车加以分配
                        string rgv = DataControl._mTaskTools.GetItemDeviceLast(item.WCS_NO, ItemId.运输车定位);
                        // 获取当前运输车资讯
                        RGV RGV = new RGV(rgv);
                        // 获取有货&无货辊台各对应的WMS任务目标
                        String loc_Y = "";  //有货辊台对应目标点
                        String loc_N = "";  //无货辊台对应目标点

                        bool isNoGoodsRGV = false; // 运输车是否无货

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
                            // 辊台已无货，即解锁设备数据状态
                            DataControl._mTaskTools.DeviceUnLock(rgv);
                            isNoGoodsRGV = true;
                        }

                        if (!isNoGoodsRGV)
                        {
                            // 判断是否需要对接到运输车[内]范围内作业
                            if (Convert.ToInt32(loc_Y) >= Convert.ToInt32(R))  // 需对接运输车[内]
                            {
                                // 生成运输车[内]复位任务
                                DataControl._mTaskTools.CreateItem(item.WCS_NO, ItemId.运输车复位2, R);    // 待分配设备
                                                                                                      // 生成运输车[外]对接位任务
                                DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车对接定位, rgv, "", RR, ItemStatus.请求执行);
                            }
                            else
                            {
                                // 生成运输车[外]定位任务
                                DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, rgv, "", loc_Y, ItemStatus.请求执行);
                            }
                        }

                        // 生成行车库存定位任务
                        if (String.IsNullOrEmpty(loc_N) && String.IsNullOrEmpty(loc_Y))
                        {
                            loc = command.SITE_1 == TaskSite.完成 ? command.LOC_TO_2 : command.LOC_TO_1;
                        }
                        else
                        {
                            loc = loc_N == loc_1 ? command.LOC_TO_1 : command.LOC_TO_2;
                        }
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车库存定位, item.DEVICE, "", DataControl._mTaskTools.GetABCStockLoc(loc), ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.行车放货:
                        #region 行车定位
                        // 未完成的任务目标点
                        loc = command.SITE_1 == TaskSite.完成 ? command.LOC_TO_2 : command.LOC_TO_1;
                        // 行车到运输车对接取货点
                        String ABCloc = DataControl._mTaskTools.GetABCTrackLoc(loc); //获取对应行车位置
                        // 生成行车轨道定位任务
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车轨道定位, item.DEVICE, "", ABCloc, ItemStatus.请求执行);
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
                // 运输车[外]待命复位点
                String R1 = DataControl._mStools.GetValueByKey("StandbyR1");
                // 运输车[内]待命复位点
                String R2 = DataControl._mStools.GetValueByKey("StandbyR2");
                // 运输车于运输车对接点
                String RR = DataControl._mStools.GetValueByKey("StandbyRR");

                // 获取对应清单
                String sql = String.Format(@"select * from wcs_command_v where WCS_NO = '{0}'", item.WCS_NO);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                WCS_COMMAND_V command = dt.ToDataEntity<WCS_COMMAND_V>();

                // 默认出库时 taskid1对接运输车设备辊台①、taskid2对接运输车设备辊台②
                String loc_1 = DataControl._mTaskTools.GetRGVLoc(1, command.LOC_FROM_1); //辊台①任务1
                String loc_2 = DataControl._mTaskTools.GetRGVLoc(2, command.LOC_FROM_2); //辊台②任务2

                switch (item.ITEM_ID)   //根据最后的设备指令，可得货物已在流程中该设备对接的下一设备处
                {
                    case ItemId.行车取货:
                        #region 行车轨道定位与运输车对接点
                        // 生成行车库存定位任务
                        String sqlloc = String.Format(@"select distinct ABC_LOC_TRACK from WCS_CONFIG_LOC where ABC_LOC_STOCK = '{0}'", item.LOC_TO);
                        DataTable dtloc = DataControl._mMySql.SelectAll(sqlloc);
                        if (DataControl._mStools.IsNoData(dtloc))
                        {
                            return;
                        }
                        DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车轨道定位, item.DEVICE, "", dtloc.Rows[0]["ABC_LOC_TRACK"].ToString(), ItemStatus.请求执行);
                        #endregion

                        break;
                    case ItemId.行车放货:
                        #region 将运输车复位待命点 / 将运输车移至行车对接位置 && 行车库存定位
                        // 获取当前运输车加以分配
                        rgv = DataControl._mTaskTools.GetItemDeviceLast(item.WCS_NO, ItemId.运输车定位);
                        // 获取当前运输车资讯
                        RGV = new RGV(rgv);

                        // 流程上设定是先分配运输车辊台①放置货物，即现确认运输车辊台②是否有任务/有货物
                        // 车辊台②不存在任务直接复位 ｜｜　所有辊台都有货
                        if (String.IsNullOrEmpty(command.TASK_UID_2.Trim()) || RGV.GoodsStatus() == RGV.GoodsYesAll)
                        {
                            // 将运输车复位待命点
                            // 当前设备位置 >= 运输车[内]待命复位点 ?  运输车[内]复位 ：运输车[外]复位
                            if (RGV.GetCurrentSite() >= Convert.ToInt32(R2))
                            {
                                // 生成运输车[内]复位任务
                                DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车复位2, rgv, "", R2, ItemStatus.请求执行);
                                // 生成运输车[外]对接任务
                                DataControl._mTaskTools.CreateItem(item.WCS_NO, ItemId.运输车对接定位, RR);    // 待分配设备
                            }
                            else
                            {
                                // 生成运输车[外]复位任务
                                DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车复位1, rgv, "", R1, ItemStatus.请求执行);
                            }
                        }
                        else if (RGV.GoodsStatus() == RGV.GoodsYes1) //执行辊台②任务2
                        {
                            // 将运输车移至行车对接位置 && 行车库存定位
                            // 生成运输车定位任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车定位, rgv, "", loc_2, ItemStatus.请求执行);
                            // 生成行车库存定位任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.行车库存定位, item.DEVICE, "", DataControl._mTaskTools.GetABCStockLoc(command.LOC_FROM_2), ItemStatus.请求执行);
                        }

                        #endregion

                        break;
                    case ItemId.运输车反向:
                        #region 将摆渡车移至固定辊台对接点 / 将运输车移至摆渡车对接点(复位待命点1)
                        // 获取当前对接任务的目的设备类型
                        String type = DataControl._mTaskTools.GetDeviceType(item.LOC_TO);
                        if (type == DeviceType.运输车)
                        {
                            // 即执行完运输车间对接作业，按流程需将货物移至摆渡车
                            // 生成将运输车[外]复位待命点1任务
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.运输车复位1, item.LOC_TO, "", R1, ItemStatus.请求执行);
                        }
                        else if (type == DeviceType.摆渡车)
                        {
                            // 即执行完运输车&摆渡车对接作业，按流程需将货物移至固定辊台
                            // 生成将摆渡车移至固定辊台对接点任务
                            String ARFloc = DataControl._mTaskTools.GetARFLoc(command.FRT);    //获取对应摆渡车位置
                            DataControl._mTaskTools.CreateCustomItem(item.WCS_NO, ItemId.摆渡车定位固定辊台, item.LOC_TO, "", ARFloc, ItemStatus.请求执行);
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
                DataTable dtarea = DataControl._mMySql.SelectAll("select distinct AREA from wcs_config_device");
                if (DataControl._mStools.IsNoData(dtarea))
                {
                    return;
                }
                // 遍历判断作业区域情况
                foreach (DataRow dr in dtarea.Rows)
                {
                    String area = dr[0].ToString();

                    // 判断当前区域是否存在出库请求
                    int taskcount = DataControl._mMySql.GetCount("wcs_task_info", String.Format(@"TASK_TYPE = '{2}' and SITE = '{1}' and W_D_LOC = '{0}'", area, TaskSite.未执行, TaskType.出库));
                    if (taskcount == 0) //无则退出
                    {
                        continue;
                    }

                    // 判断当前区域是否存在执行中的任务
                    int commandcount = DataControl._mMySql.GetCount("wcs_command_master", String.Format(@"STEP = '{1}' and FRT in
                                                   (select distinct DEVICE from wcs_config_device where TYPE = '{2}' and AREA = '{0}')", area, CommandStep.执行中, DeviceType.固定辊台));
                    if (commandcount > 0) //有则退出
                    {
                        continue;
                    }

                    // 判断当前区域是否存在满足入库条件的任务
                    int incount = DataControl._mMySql.GetCount("wcs_command_v", String.Format(@"TASK_TYPE = '{2}' and STEP = '{1}' and FRT in
                                                  (select distinct DEVICE from wcs_config_device where TYPE = '{3}' and AREA = '{0}')", area, CommandStep.请求执行, TaskType.入库, DeviceType.固定辊台));
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
                String sql = String.Format(@"select * from wcs_config_device where TYPE = '{1}' and AREA = '{0}'", area, DeviceType.行车);
                DataTable dtabc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtabc))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> abcList = dtabc.ToDataList<WCS_CONFIG_DEVICE>();

                // 行车中间值
                int AA = Convert.ToInt32(DataControl._mStools.GetValueByKey("CenterX"));

                // 遍历确认出库任务
                foreach (WCS_CONFIG_DEVICE abc in abcList)
                {
                    // 获取当前行车资讯
                    ABC ABC = new ABC(abc.DEVICE);
                    // 获取当前坐标X轴值
                    int X = DataControl._mStools.BytesToInt(ABC.CurrentXsite(), 0);
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
                    CreateOutJob(area, task[0].ToString(), task[1].ToString());
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
                int AA = Convert.ToInt32(DataControl._mStools.GetValueByKey("CenterX"));
                // 大小运算符号
                String sign;
                if (range == 1)
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
			       else t.X end) as locX, t.TASK_UID, t.Z
  from (select a.TASK_UID, (b.STOCK_X + 0) X, (b.STOCK_Z + 0) Z
          from (select distinct TASK_UID, W_S_LOC from wcs_task_info where SITE = '{3}' and TASK_TYPE = '{4}') a, 
	             (select distinct WMS_LOC, SUBSTRING_INDEX(ABC_LOC_STOCK,'-',1) STOCK_X, SUBSTRING_INDEX(ABC_LOC_STOCK,'-',-1) STOCK_Z from wcs_config_loc) b
         where a.W_S_LOC = b.WMS_LOC and (b.STOCK_X + 0) {1} {2}
       ) t order by locX asc, Z desc", X, sign, AA, TaskSite.未执行, TaskType.出库);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
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
        public void CreateOutJob(String area, String taskuid_1, String taskuid_2)
        {
            //生成 WCS 清单号
            String wcs_no = "O" + System.DateTime.Now.ToString("yyMMddHHmmss");
            try
            {
                String frt;
                // 自动生成 / 手动
                string sqlwcs = string.Format(@"select * from wcs_command_v where TASK_UID_1 = '{0}' ", taskuid_1);
                if (!String.IsNullOrEmpty(taskuid_2.Trim()))
                {
                    sqlwcs += string.Format(@" and TASK_UID_2 = '{0}'", taskuid_2);
                }
                DataTable dtwcs = DataControl._mMySql.SelectAll(sqlwcs);
                if (DataControl._mStools.IsNoData(dtwcs))
                {
                    // 获取该区域可用的固定辊台
                    String sqlfrt = String.Format(@"select MAX(device) FRT from wcs_config_device where FLAG = '{1}' and TYPE = '{2}' and AREA = '{0}'", area, DeviceFlag.空闲, DeviceType.固定辊台);
                    DataTable dtfrt = DataControl._mMySql.SelectAll(sqlfrt);
                    if (DataControl._mStools.IsNoData(dtfrt))
                    {
                        return;
                    }
                    frt = dtfrt.Rows[0]["FRT"].ToString();

                    // 自动生成 COMMAND
                    String sql = String.Format(@"insert into wcs_command_master(WCS_NO, FRT, TASK_UID_1, TASK_UID_2) values('{0}','{1}','{2}','{3}')",
                            wcs_no, frt, taskuid_1, String.IsNullOrEmpty(taskuid_2.Trim()) ? null : taskuid_2);
                    DataControl._mMySql.ExcuteSql(sql);
                }
                else
                {
                    // 手动
                    WCS_COMMAND_V cmd = dtwcs.ToDataEntity<WCS_COMMAND_V>();
                    wcs_no = cmd.WCS_NO;
                    frt = cmd.FRT;
                }

                // 默认先处理任务1：获取对应的任务1资讯
                DataTable dttask = DataControl._mMySql.SelectAll(String.Format(@"select * From wcs_task_info where task_uid = '{0}'", taskuid_1));
                if (DataControl._mStools.IsNoData(dttask))
                {
                    return;
                }
                WCS_TASK_INFO info = dttask.ToDataEntity<WCS_TASK_INFO>();

                //生成 ITEM
                // 生成行车库存定位任务
                String ABCloc = DataControl._mTaskTools.GetABCStockLoc(info.W_S_LOC); //获取对应库存位置
                DataControl._mTaskTools.CreateItem(wcs_no, ItemId.行车库存定位, ABCloc);  //生成行车任务

                //生成运输车对接行车任务(默认先对接运输车辊台①)
                String RGVloc = DataControl._mTaskTools.GetRGVLoc(1, info.W_S_LOC); //获取运输车对接行车位置
                DataControl._mTaskTools.CreateItem(wcs_no, ItemId.运输车定位, RGVloc);  //生成运输车任务

                //生成摆渡车对接运输车任务
                DataControl._mTaskTools.CreateItem(wcs_no, ItemId.摆渡车定位运输车对接, DataControl._mStools.GetValueByKey("StandbyAR"));  //生成摆渡车任务

                //更新WCS COMMAND状态——执行中
                DataControl._mTaskTools.UpdateCommand(wcs_no, CommandStep.执行中);
                //更新WCS TASK状态——任务中
                DataControl._mTaskTools.UpdateTaskByWCSNo(wcs_no, TaskSite.任务中);
                // 锁定设备
                DataControl._mTaskTools.DeviceLock(wcs_no, frt);

            }
            catch (Exception ex)
            {
                //初始化
                DataControl._mTaskTools.DeleteCommand(wcs_no);
                DataControl._mTaskTools.UpdateTaskByWCSNo(wcs_no, TaskSite.未执行);
                DataControl._mTaskTools.DeleteItem(wcs_no, "");
                // 解锁设备数据状态
                DataControl._mTaskTools.DeviceUnLock(wcs_no);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateOutJob()", "生成WCS出库清单", null, null, ex.ToString());
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
                String sql = String.Format(@"select * from WCS_TASK_ITEM where STATUS = '{0}' and DEVICE is null order by ID", ItemStatus.不可执行);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
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
                String frt = DataControl._mTaskTools.GetFRTByWCSNo(item.WCS_NO);
                if (String.IsNullOrEmpty(frt.Trim()))
                {
                    return;
                }

                // 获取任务所在作业区域
                String area = DataControl._mTaskTools.GetArea(frt);
                if (String.IsNullOrEmpty(frt.Trim()))
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
                        List<WCS_CONFIG_DEVICE> dList_ARF = DataControl._mTaskTools.GetDeviceList(area, DeviceType.摆渡车);
                        // 确认其中最适合的摆渡车
                        String arf = GetSuitableARF(item.LOC_TO, dList_ARF);
                        if (String.IsNullOrEmpty(arf.Trim()))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        ARF ARF = new ARF(arf);
                        device = arf;
                        loc = ARF.CurrentSite().ToString();

                        #endregion

                        break;
                    case "02":
                        #region 运输车

                        // 获取作业区域内的运输车
                        List<WCS_CONFIG_DEVICE> dList_RGV = DataControl._mTaskTools.GetDeviceList(area, DeviceType.运输车);
                        // 确认其中最适合的摆渡车
                        String rgv = GetSuitableRGV(item.LOC_TO, dList_RGV);
                        if (String.IsNullOrEmpty(rgv.Trim()))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        RGV RGV = new RGV(rgv);
                        device = rgv;
                        loc = RGV.GetCurrentSite().ToString();

                        #endregion

                        break;
                    case "03":
                        #region 行车

                        // 获取作业区域内的行车
                        List<WCS_CONFIG_DEVICE> dList_ABC = DataControl._mTaskTools.GetDeviceList(area, DeviceType.行车);
                        // 确认其中最适合的行车
                        String abc = GetSuitableABC(item.LOC_TO, dList_ABC);
                        if (String.IsNullOrEmpty(abc.Trim()))
                        {
                            return;
                        }
                        // 确认任务设备&位置
                        ABC ABC = new ABC(abc);
                        device = abc;
                        loc = ABC.GetCurrentSite();

                        #endregion

                        break;
                    default:
                        break;
                }

                if (String.IsNullOrEmpty(device.Trim()) || String.IsNullOrEmpty(loc.Trim()) || loc.Equals("0"))
                {
                    return;
                }
                // 确认设备是否锁定
                if (DataControl._mTaskTools.IsDeviceLock(device))
                {
                    return;
                }
                // 确认任务设备
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.设备编号, device);
                // 确认设备来源
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.来源位置, loc);
                // 更新状态
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.请求执行);
                // 锁定设备
                DataControl._mTaskTools.DeviceLock(item.WCS_NO, device);
            }
            catch (Exception ex)
            {
                //初始化
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.不可执行);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("ReadDevice()", "分配任务指定设备", item.WCS_NO, item.ITEM_ID, ex.ToString());
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
                ARF dev;
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
                    dev = new ARF(d.DEVICE);

                    // 当前坐标值比较目标位置
                    if (dev.CurrentSite() == LOC)    // 当前坐标值 = 目标位置
                    {
                        // 直接选取该设备
                        arf = d.DEVICE;
                        break;
                    }
                    else if (dev.CurrentSite() < LOC) // 当前坐标值 < 目标位置
                    {
                        // 距离比较值 >=（目标位置 - 当前坐标值）
                        if (X >= (LOC - dev.CurrentSite()))
                        {
                            // 暂选取该设备
                            arf = d.DEVICE;
                            // 更新距离比较值
                            X = (byte)(LOC - dev.CurrentSite());
                        }
                    }
                    else // 当前坐标值 > 目标位置
                    {
                        // 距离比较值 >=（当前坐标值 - 目标位置）
                        if (X >= (dev.CurrentSite() - LOC))
                        {
                            // 暂选取该设备
                            arf = d.DEVICE;
                            // 更新距离比较值
                            X = (byte)(dev.CurrentSite() - LOC);
                        }
                    }
                    dev = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(arf.Trim()))
                {
                    dev = new ARF(arf);
                    // 设备故障, 正在运行, 货物状态不为无货, 辊台状态不为停止 ==> 不可用
                    if (dev.DeviceStatus() == ARF.DeviceError || dev.ActionStatus() == ARF.Run || dev.GoodsStatus() != ARF.GoodsNoAll || dev.CurrentStatus() != ARF.RollerStop)
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
                RGV dev;
                // 终选运输车
                String rgv = String.Empty;
                // 目标位置
                int LOC = Convert.ToInt32(loc);
                // 运输车间对接点
                int RR = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyRR"));

                // 遍历比对
                foreach (WCS_CONFIG_DEVICE d in dList)
                {
                    // 获取摆渡车设备资讯
                    dev = new RGV(d.DEVICE);

                    // 比较目标位置与对接点
                    if (LOC <= RR)
                    {
                        // 仅获取位置于运输车[外]运输范围内的 RGV
                        if (dev.GetCurrentSite() <= RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }
                    else
                    {
                        // 仅获取位置于运输车[内]运输范围内的 RGV
                        if (dev.GetCurrentSite() > RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }

                    dev = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(rgv.Trim()))
                {
                    dev = new RGV(rgv);
                    // 设备故障, 正在运行, 货物状态不为无货, 辊台状态不为停止 ==> 不可用
                    if (dev.DeviceStatus() == RGV.DeviceError || dev.ActionStatus() == RGV.Run || dev.GoodsStatus() != RGV.GoodsNoAll || dev.CurrentStatus() != RGV.RollerStop)
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
                ABC dev;
                // 终选行车
                String abc = String.Empty;
                // 行车中间值
                int AA = Convert.ToInt32(DataControl._mStools.GetValueByKey("CenterX"));
                // 目标位置 X轴值
                String[] LOC = loc.Split('-');
                int X = Convert.ToInt32(LOC[0]);

                // 遍历比对
                foreach (WCS_CONFIG_DEVICE d in dList)
                {
                    // 获取行车设备资讯
                    dev = new ABC(d.DEVICE);

                    // 比较目标X轴值与对接点
                    if (X <= AA)
                    {
                        // 仅获取位置于行车[外]运输范围内的 ABC
                        if (DataControl._mStools.BytesToInt(dev.CurrentXsite(), 0) <= AA)
                        {
                            // 锁定设备
                            abc = d.DEVICE;
                            break;
                        }
                    }
                    else
                    {
                        // 仅获取位置于行车[内]运输范围内的 ABC
                        if (DataControl._mStools.BytesToInt(dev.CurrentXsite(), 0) > AA)
                        {
                            // 锁定设备
                            abc = d.DEVICE;
                            break;
                        }
                    }

                    dev = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(abc.Trim()))
                {
                    dev = new ABC(abc);
                    // 设备故障, 正在运行, 货物状态不为无货 ==> 不可用
                    if (dev.DeviceStatus() == ABC.DeviceError || dev.ActionStatus() == ABC.Run || dev.GoodsStatus() != ABC.GoodsNo)
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

        #region 生成指令&加入作业链表

        /// <summary>
        /// 执行出入库任务对应指令
        /// </summary>
        public void Run_Order()
        {
            try
            {
                // 获取 请求执行 的任务对应的 ITEM 资讯
                String sql = String.Format(@"select * from WCS_TASK_ITEM where STATUS = '{0}' order by ID", ItemStatus.请求执行);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历处理任务
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    CreateAndAddTaskList(item);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 生成设备指令&加入作业链表
        /// </summary>
        /// <param name="item"></param>
        public void CreateAndAddTaskList(WCS_TASK_ITEM item)
        {
            try
            {
                // 指令
                byte[] order = null;
                // 任务货物数量
                int qty = DataControl._mTaskTools.GetTaskGoodsQty(item.WCS_NO);
                //启动方式
                byte site1;
                //启动方向
                byte site2;
                //接送类型
                byte site3;
                //货物数量
                byte site4;

                switch (item.ITEM_ID)
                {
                    case ItemId.固定辊台正向:
                    case ItemId.固定辊台反向:
                        #region FRT 辊台指令
                        FRT frt = new FRT(item.DEVICE);
                        // 根据任务类型确认
                        site2 = item.ITEM_ID == ItemId.固定辊台正向 ? FRT.RunFront : FRT.RunObverse;
                        // 根据货物对接任务的目的设备确认
                        site3 = String.IsNullOrEmpty(item.LOC_TO.Trim()) ? FRT.GoodsReceive : FRT.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = FRT.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == FRT.RunFront && site3 == FRT.GoodsReceive) // 正向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes2) { site1 = FRT.RollerRun1; }
                        }
                        else if (site2 == FRT.RunFront && site3 == FRT.GoodsDeliver) // 正向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes2) { site1 = FRT.RollerRun2; }
                        }
                        else if (site2 == FRT.RunObverse && site3 == FRT.GoodsReceive) // 反向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes1) { site1 = FRT.RollerRun2; }
                        }
                        else if (site2 == FRT.RunObverse && site3 == FRT.GoodsDeliver) // 反向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes1) { site1 = FRT.RollerRun1; }
                        }

                        // 确认辊台货物数量
                        site4 = qty == 1 ? FRT.GoodsQty1 : FRT.GoodsQty2;   // 初始默认根据任务货物数量确认
                        if (site3 == FRT.GoodsDeliver)  // 送货
                        {
                            // 送货指令中以设备当前货物数量确定
                            site4 = frt.GoodsStatus() == FRT.GoodsYesAll ? FRT.GoodsQty2 : FRT.GoodsQty1;
                        }

                        // 获取指令
                        order = FRT._RollerControl(frt.FRTNum(), site1, site2, site3, site4);
                        // 加入任务作业链表
                        DataControl._mTaskControler.StartTask(new FRTTack(item, DeviceType.固定辊台, order));
                        #endregion

                        break;
                    case ItemId.摆渡车正向:
                    case ItemId.摆渡车反向:
                        #region ARF 辊台指令
                        ARF arf = new ARF(item.DEVICE);
                        // 根据任务类型确认
                        site2 = item.ITEM_ID == ItemId.摆渡车正向 ? ARF.RunFront : ARF.RunObverse;
                        // 根据货物对接任务的目的设备确认
                        site3 = String.IsNullOrEmpty(item.LOC_TO.Trim()) ? ARF.GoodsReceive : ARF.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = ARF.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == ARF.RunFront && site3 == ARF.GoodsReceive) // 正向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes2) { site1 = ARF.RollerRun1; }
                        }
                        else if (site2 == ARF.RunFront && site3 == ARF.GoodsDeliver) // 正向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes2) { site1 = ARF.RollerRun2; }
                        }
                        else if (site2 == ARF.RunObverse && site3 == ARF.GoodsReceive) // 反向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes1) { site1 = ARF.RollerRun2; }
                        }
                        else if (site2 == ARF.RunObverse && site3 == ARF.GoodsDeliver) // 反向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes1) { site1 = ARF.RollerRun1; }
                        }

                        // 确认辊台货物数量
                        site4 = qty == 1 ? ARF.GoodsQty1 : ARF.GoodsQty2;   // 初始默认根据任务货物数量确认
                        if (site3 == ARF.GoodsDeliver)  // 送货
                        {
                            // 送货指令中以设备当前货物数量确定
                            site4 = arf.GoodsStatus() == ARF.GoodsYesAll ? ARF.GoodsQty2 : ARF.GoodsQty1;
                        }

                        // 获取指令
                        order = ARF._RollerControl(arf.ARFNum(), site1, site2, site3, site4);
                        // 加入任务作业链表
                        DataControl._mTaskControler.StartTask(new ARFTack(item, DeviceType.摆渡车, order));
                        #endregion

                        break;
                    case ItemId.运输车正向:
                    case ItemId.运输车反向:
                        #region RGV 辊台指令
                        RGV rgv = new RGV(item.DEVICE);
                        // 根据任务类型确认
                        site2 = item.ITEM_ID == ItemId.运输车正向 ? RGV.RunFront : RGV.RunObverse;
                        // 根据货物对接任务的目的设备确认
                        site3 = String.IsNullOrEmpty(item.LOC_TO.Trim()) ? RGV.GoodsReceive : RGV.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = RGV.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == RGV.RunFront && site3 == RGV.GoodsReceive) // 正向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes2) { site1 = RGV.RollerRun1; }
                        }
                        else if (site2 == RGV.RunFront && site3 == RGV.GoodsDeliver) // 正向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes2) { site1 = RGV.RollerRun2; }
                        }
                        else if (site2 == RGV.RunObverse && site3 == RGV.GoodsReceive) // 反向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes1) { site1 = RGV.RollerRun2; }
                        }
                        else if (site2 == RGV.RunObverse && site3 == RGV.GoodsDeliver) // 反向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes1) { site1 = RGV.RollerRun1; }
                        }

                        // 确认辊台货物数量
                        site4 = qty == 1 ? RGV.GoodsQty1 : RGV.GoodsQty2;   // 初始默认根据任务货物数量确认
                        if (site3 == RGV.GoodsDeliver)  // 送货
                        {
                            // 送货指令中以设备当前货物数量确定
                            site4 = rgv.GoodsStatus() == RGV.GoodsYesAll ? RGV.GoodsQty2 : RGV.GoodsQty1;
                        }

                        // 获取指令
                        order = RGV._RollerControl(rgv.RGVNum(), site1, site2, site3, site4);
                        // 加入任务作业链表
                        DataControl._mTaskControler.StartTask(new RGVTack(item, DeviceType.运输车, order));
                        #endregion

                        break;
                    case ItemId.摆渡车复位:
                    case ItemId.摆渡车定位固定辊台:
                    case ItemId.摆渡车定位运输车对接:
                        #region ARF 定位指令
                        ARF arfMove = new ARF(item.DEVICE);
                        // 提取目的位置
                        byte arfloc = (byte)(Convert.ToInt32(item.LOC_TO));
                        // 获取指令
                        order = ARF._Position(arfMove.ARFNum(), arfloc);
                        // 加入任务作业链表
                        DataControl._mTaskControler.StartTask(new ARFTack(item, DeviceType.摆渡车, order));
                        #endregion

                        break;
                    case ItemId.运输车定位:
                    case ItemId.运输车复位1:
                    case ItemId.运输车复位2:
                    case ItemId.运输车对接定位:
                        #region RGV 定位指令
                        RGV rgvMove = new RGV(item.DEVICE);
                        // 提取目的位置
                        byte[] rgvloc = DataControl._mStools.IntToBytes(Convert.ToInt32(item.LOC_TO));
                        // 获取指令
                        order = RGV._Position(rgvMove.RGVNum(), rgvloc);
                        // 加入任务作业链表
                        DataControl._mTaskControler.StartTask(new RGVTack(item, DeviceType.运输车, order));
                        #endregion

                        break;
                    case ItemId.行车取货:
                    case ItemId.行车放货:
                    case ItemId.行车轨道定位:
                    case ItemId.行车库存定位:
                        #region ABC 指令
                        ABC abc = new ABC(item.DEVICE);
                        // 提取目的位置
                        String[] LOC = item.LOC_TO.Split('-');
                        byte[] locX = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[0].Trim()) ? "0" : LOC[0]));
                        byte[] locY = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[1].Trim()) ? "0" : LOC[1]));
                        byte[] locZ = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[2].Trim()) ? "0" : LOC[2]));
                        // 指令类型
                        byte type;
                        if (item.ITEM_ID == ItemId.行车取货)
                        {
                            type = ABC.TaskTake;
                        }
                        else if (item.ITEM_ID == ItemId.行车放货)
                        {
                            type = ABC.TaskRelease;
                        }
                        else // 定位任务
                        {
                            type = ABC.TaskLocate;
                        }
                        // 获取指令
                        order = ABC._TaskControl(type, abc.ABCNum(), locX, locY, locZ);
                        //加入任务作业链表
                        DataControl._mTaskControler.StartTask(new ABCTack(item, DeviceType.行车, order));
                        #endregion

                        break;
                    default:
                        return;
                }
                // 更新状态
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, item.ITEM_ID, ItemColumnName.作业状态, ItemStatus.任务中);
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateAndAddTaskList()", "生成并加入设备指令任务链表", item.WCS_NO, item.ITEM_ID, ex.ToString());
            }
        }

        #endregion

    }

    /// <summary>
    /// 任务流程
    /// </summary>
    public class RunTask
    {
        // 线程
        Thread _thread;
        // 线程开关
        public bool PowerSwitch = true;
        // 执行对象
        TaskLogic _task = new TaskLogic();

        /// <summary>
        /// 构造函数
        /// </summary>
        public RunTask()
        {
            _thread = new Thread(ThreadFunc)
            {
                Name = "任务逻辑处理线程",
                IsBackground = true
            };

            _thread.Start();
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        public void Close()
        {
            PowerSwitch = false;
        }

        /// <summary>
        /// 事务线程
        /// </summary>
        private void ThreadFunc()
        {
            while (PowerSwitch)
            {
                Thread.Sleep(5000);
                if (!PublicParam.IsRunTaskLogic)
                {
                    continue;
                }
                try
                {
                    _task.Run_TaskContinued();
                    _task.Run_InInitial();
                    _task.Run_ItemDevice();
                    _task.Run_LinkDevice();
                    _task.Run_OutFollow();
                    _task.Run_Order();
                }
                catch (Exception)
                {
                }
            }
        }
    }

}
