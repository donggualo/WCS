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
        #region 入库流程

        #region 初步入库任务

        /// <summary>
        /// 执行入库清单确认
        /// </summary>
        public void CheckInTask()
        {
            try
            {
                // 获取单托任务清单
                String sql1 = String.Format(@"select * from wcs_command_v where TASK_UID_1 is not null and TASK_UID_1 <> '' and (TASK_UID_2 is null or TASK_UID_2 = '') and TASK_TYPE = '{0}' and STEP = '{1}' 
                    and TRUNCATE((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(CREATION_TIME))/60,0) >= {2}",
                    TaskType.入库, CommandStep.生成单号, DataControl._mStools.GetValueByKey("InTimeMax")); // 等待时间
                DataTable dtcommand1 = DataControl._mMySql.SelectAll(sql1);
                if (DataControl._mStools.IsNoData(dtcommand1))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList1 = dtcommand1.ToDataList<WCS_COMMAND_V>();
                // 遍历执行单托任务请求WMS分配库位
                foreach (WCS_COMMAND_V com1 in comList1)
                {
                    //更新WCS COMMAND状态——请求执行
                    DataControl._mTaskTools.UpdateCommand(com1.WCS_NO, CommandStep.请求执行);
                    // 锁定设备
                    DataControl._mTaskTools.DeviceLock(com1.WCS_NO, com1.FRT);
                }

                // 获取双托任务清单
                String sql2 = String.Format(@"select * from wcs_command_v where TASK_UID_1 is not null and TASK_UID_1 <> '' and TASK_UID_2 is not null and TASK_UID_2 <> '' 
                                                 and  TASK_TYPE = '{0}' and STEP = '{1}' order by CREATION_TIME",
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
                    //更新WCS COMMAND状态——请求执行
                    DataControl._mTaskTools.UpdateCommand(com2.WCS_NO, CommandStep.请求执行);
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CheckInTask()", "入库清单检测", null, null, ex.Message);
            }
        }

        /// <summary>
        /// 执行WCS入库清单（初步执行）
        /// </summary>
        public void Run_InInitial()
        {
            try
            {
                CheckInTask();
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
                String loc1 = DataControl._mTaskTools.GetRGVLoc(1, command.LOC_TO_1); //运输车辊台①任务1
                String loc2 = DataControl._mTaskTools.GetRGVLoc(2, command.LOC_TO_2); //运输车辊台②任务2
                String loc = DataControl._mTaskTools.GetLocByRgvToLoc(loc1, loc2); //处理货位
                if (loc == "NG")
                {
                    //不能没有货物目的位置
                    return;
                }
                // 获取清单任务分别对应的分区行车
                string ABC_1 = DataControl._mTaskTools.GetABCByInTaskUID(command.TASK_UID_1);
                string ABC_2 = DataControl._mTaskTools.GetABCByInTaskUID(command.TASK_UID_2);
                string ABCloc_1;
                string ABCloc_2;
                string ABCtask_1;
                string ABCtask_2;
                // 行车到运输车对接取货点
                if (ABC_1 == ABC_2)
                {
                    // 当所有入库坐标为同一行车分区，确认执行先后
                    if (loc == loc1)
                    {
                        ABCtask_1 = command.TASK_UID_1;
                        ABCloc_1 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_1);

                        ABCtask_2 = command.TASK_UID_2;
                        ABCloc_2 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_2);
                    }
                    else
                    {
                        ABCtask_1 = command.TASK_UID_2;
                        ABCloc_1 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_2);

                        ABCtask_2 = command.TASK_UID_1;
                        ABCloc_2 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_1);
                    }
                    DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, ABCtask_1, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc_1, ItemStatus.不可执行, ABC_1);
                    DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, ABCtask_2, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc_2, ItemStatus.不可执行, ABC_2);
                }
                else if (string.IsNullOrEmpty(ABC_1) && string.IsNullOrEmpty(ABC_2))
                {
                    return;
                }
                else if (string.IsNullOrEmpty(ABC_1) || string.IsNullOrEmpty(ABC_2))
                {
                    ABCtask_1 = string.IsNullOrEmpty(ABC_1) ? command.TASK_UID_2 : command.TASK_UID_1;
                    ABCloc_1 = DataControl._mTaskTools.GetABCTrackLoc(string.IsNullOrEmpty(ABC_1) ? command.LOC_TO_2 : command.LOC_TO_1);
                    DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, ABCtask_1, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc_1, ItemStatus.不可执行, string.IsNullOrEmpty(ABC_1) ? ABC_2 : ABC_1);
                }
                else
                {
                    ABCloc_1 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_1);
                    ABCloc_2 = DataControl._mTaskTools.GetABCTrackLoc(command.LOC_TO_2);
                    DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, command.TASK_UID_1, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc_1, ItemStatus.不可执行, ABC_1);
                    DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, command.TASK_UID_2, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc_2, ItemStatus.不可执行, ABC_2);
                }

                // 优先执行任务
                String task = loc == loc1 ? command.TASK_UID_1 : command.TASK_UID_2;

                // 摆渡车到固定辊台对接点
                String ARFloc = DataControl._mTaskTools.GetARFLoc(command.FRT); //获取对应摆渡车位置
                DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, task, ItemId.摆渡车定位固定辊台, DeviceType.摆渡车, null, ARFloc, ItemStatus.不可执行);

                // 运输车到摆渡车对接点
                String RGVloc = DataControl._mStools.GetValueByKey("StandbyR1"); //获取运输车复位1位置
                DataControl._mTaskTools.CreateTaskItem(command.WCS_NO, task, ItemId.运输车复位1, DeviceType.运输车, null, RGVloc, ItemStatus.不可执行);

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
                DataControl._mTaskTools.DeleteItem(command.WCS_NO);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("Task_InInitial()", "生成初始入库任务", command.WCS_NO, null, ex.Message);
            }
        }

        #endregion

        #region 各设备入库步骤

        public void Run_WCStask_In()
        {
            try
            {
                // 获取执行中的入库清单
                String sql = String.Format(@"select * from wcs_command_v where TASK_TYPE = '{0}' and STEP in('{1}','{2}') order by CREATION_TIME",
                    TaskType.入库, CommandStep.执行中, CommandStep.结束);
                DataTable dtcommand = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtcommand))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList = dtcommand.ToDataList<WCS_COMMAND_V>();
                // 遍历执行入库任务
                foreach (WCS_COMMAND_V com in comList)
                {
                    // 清单是[结束]状态 => 不作业
                    if (com.STEP == CommandStep.结束)
                    {
                        // 备份任务数据
                        //DataControl._mTaskTools.BackupTask(com.WCS_NO);
                        // 解锁对应清单所有设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(com.WCS_NO);
                        continue;
                    }

                    TaskIn_ABC(com);
                    TaskIn_RGV(com);
                    TaskIn_ARF(com);

                    AllotItemDev(com);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void TaskIn_ABC(WCS_COMMAND_V com)
        {
            try
            {
                // 获取当前任务最新的行车任务
                List<WCS_TASK_ITEM> itemList_ABC = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.行车);
                if (itemList_ABC.Count != 0)
                {
                    // 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_ABC)
                    {
                        switch (item.ITEM_ID)
                        {
                            case ItemId.行车轨道定位:
                                #region 生成取货
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应任务的运输车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.运输车定位, out int rgvID, out string rgv))
                                {
                                    // 生成取货任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车取货, DeviceType.行车, null, item.LOC_TO, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }

                                #endregion
                                break;
                            case ItemId.行车取货:
                                #region 生成库存定位
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 生成库存定位任务
                                String ABCloc = DataControl._mTaskTools.GetABCStockLoc(item.TASK_NOW == com.TASK_UID_1 ? com.LOC_TO_1 : com.LOC_TO_2);
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车库存定位, DeviceType.行车, null, ABCloc, ItemStatus.请求执行, item.DEVICE);

                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.行车库存定位:
                                #region 生成放货
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 生成放货任务
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车放货, DeviceType.行车, null, item.LOC_TO, ItemStatus.请求执行, item.DEVICE);
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.行车放货:
                                #region 完成任务 || 启动另一笔任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 是否存在另一笔任务待完成
                                if (DataControl._mTaskTools.IsOtherTask(item.WCS_NO, item.TASK_NOW, item.DEVICE, ItemStatus.不可执行, out int ID))
                                {
                                    // 更新状态=> 请求执行
                                    DataControl._mTaskTools.UpdateItem(ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.请求执行);
                                }
                                else
                                {
                                    // 解锁当前设备
                                    DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);
                                }

                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                // 更新Task完成
                                DataControl._mTaskTools.UpdateTask(item.TASK_NOW, TaskSite.完成);
                                // 通知WMS完成
                                //DataControl._mHttp.DoStockInFinishTask(item.TASK_NOW == com.TASK_UID_1 ? com.LOC_TO_1 : com.LOC_TO_2, item.TASK_NOW);

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskIn_ABC()", "行车入库任务", com.WCS_NO, null, ex.Message);
            }
        }

        public void TaskIn_RGV(WCS_COMMAND_V com)
        {
            try
            {
                // 获取当前任务最新的运输车任务
                List<WCS_TASK_ITEM> itemList_RGV = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.运输车);
                if (itemList_RGV.Count != 0)
                {
                    // 运输车[内]待命复位点
                    String R2 = DataControl._mStools.GetValueByKey("StandbyR2");
                    // 运输车于运输车对接点
                    String RR = DataControl._mStools.GetValueByKey("StandbyRR");
                    // 行车&运输车 取货安全高度
                    String AR = DataControl._mStools.GetValueByKey("ARTake");

                    // 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_RGV)
                    {
                        // 获取同区内另一个RGV
                        string RGVother = DataControl._mTaskTools.GetOtherDev(item.DEVICE, DeviceType.运输车);
                        switch (item.ITEM_ID)
                        {
                            case ItemId.运输车复位1:
                                #region 生成 摆渡车-运输车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应任务的摆渡车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车定位运输车, out int arfID, out string arf))
                                {
                                    // 摆渡车辊台, 入库目的为运输车
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车正向, DeviceType.摆渡车, null, item.DEVICE, ItemStatus.请求执行, arf);
                                    // 运输车辊台, 入库来源为摆渡车
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车正向, DeviceType.运输车, arf, null, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    DataControl._mTaskTools.UpdateItem(arfID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }

                                #endregion
                                break;
                            case ItemId.运输车复位2:
                                #region 生成 运输车-运输车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应任务的运输车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.运输车对接定位, out int rgvID, out string rgv))
                                {
                                    // 运输车[外]辊台, 入库目的为运输车[内]
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车正向, DeviceType.运输车, null, item.DEVICE, ItemStatus.请求执行, rgv);
                                    // 运输车[内]辊台, 入库来源为运输车[外]
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车正向, DeviceType.运输车, rgv, null, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    DataControl._mTaskTools.UpdateItem(rgvID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }

                                #endregion
                                break;
                            case ItemId.运输车正向:
                                #region 生成定位任务 || 对接任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                string RGVloc;
                                // 获取当前任务对应辊台
                                if (item.TASK_NOW == com.TASK_UID_1)
                                {
                                    RGVloc = DataControl._mTaskTools.GetRGVLoc(1, com.LOC_TO_1);
                                }
                                else
                                {
                                    RGVloc = DataControl._mTaskTools.GetRGVLoc(2, com.LOC_TO_2);
                                }
                                // 判断是否需要对接到运输车[内]范围内作业
                                if (item.LOC_FROM == DeviceType.摆渡车 && Convert.ToInt32(RGVloc) >= Convert.ToInt32(R2))  // 需对接运输车[内]
                                {
                                    // 生成运输车[外]对接运输车[内]任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车对接定位, DeviceType.运输车, null, RR, ItemStatus.请求执行, item.DEVICE);
                                    // 生成运输车[内]复位任务,待分配设备
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车复位2, DeviceType.运输车, null, R2, ItemStatus.不可执行, RGVother);
                                }
                                else
                                {
                                    // 生成运输车定位任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车定位, DeviceType.运输车, null, RGVloc, ItemStatus.请求执行, item.DEVICE);
                                }

                                // 获取对应任务
                                string itemID = DataControl._mTaskTools.GetDeviceType(item.LOC_FROM) == DeviceType.运输车 ? ItemId.运输车正向 : ItemId.摆渡车正向;
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, itemID, out int devID, out string dev))
                                {
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(devID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.运输车定位:
                                #region 生成定位任务 || 对接任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应行车任务
                                if (!DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.行车取货, out int abcID, out string abc))
                                {
                                    // 获取行车设备资讯
                                    ABC _abc = new ABC(abc);
                                    int abcZ = DataControl._mStools.BytesToInt(_abc.CurrentZsite());
                                    /* 设备坐标偏差 */
                                    if (!DataControl._mTaskTools.GetLocByLessGap(abc, abcZ.ToString(), out string result, "Z"))
                                    {
                                        // 记录LOG
                                        DataControl._mTaskTools.RecordTaskErrLog("TaskIn_RGV()", "运输车入库流程获取行车位置[设备号,坐标]", abc, abcZ.ToString(), result);
                                        break;
                                    }

                                    if (DataControl._mTaskTools.GetItemStatus(abcID) == ItemStatus.完成任务 || Convert.ToInt32(result) >= Convert.ToInt32(AR))
                                    {
                                        string taskNext;
                                        string locNext;
                                        if (DataControl._mTaskTools.IsRGVtaskOK(item.WCS_NO))
                                        {
                                            // 更新状态=> 完成
                                            DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                            // 解锁当前设备
                                            DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);
                                            break;
                                        }
                                        else if (item.TASK_NOW == com.TASK_UID_1)
                                        {
                                            taskNext = com.TASK_UID_2;
                                            locNext = DataControl._mTaskTools.GetRGVLoc(2, com.LOC_TO_2);
                                        }
                                        else
                                        {
                                            taskNext = com.TASK_UID_1;
                                            locNext = DataControl._mTaskTools.GetRGVLoc(1, com.LOC_TO_1);
                                        }
                                        // 判断是否需要对接到运输车[内]范围内作业
                                        if (Convert.ToInt32(item.LOC_TO) <= Convert.ToInt32(RR) && Convert.ToInt32(locNext) >= Convert.ToInt32(R2))  // 需对接运输车[内]
                                        {
                                            // 生成运输车[外]对接运输车[内]任务
                                            DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, taskNext, ItemId.运输车对接定位, DeviceType.运输车, null, RR, ItemStatus.请求执行, item.DEVICE);
                                            // 生成运输车[内]复位任务,待分配设备
                                            DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, taskNext, ItemId.运输车复位2, DeviceType.运输车, null, R2, ItemStatus.不可执行, RGVother);
                                        }
                                        else
                                        {
                                            // 生成运输车定位任务
                                            DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, taskNext, ItemId.运输车定位, DeviceType.运输车, null, locNext, ItemStatus.请求执行, item.DEVICE);
                                        }
                                        // 更新状态=> 完成
                                        DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    }
                                }

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskIn_RGV()", "运输车入库任务", com.WCS_NO, null, ex.Message);
            }
        }

        public void TaskIn_ARF(WCS_COMMAND_V com)
        {
            try
            {
                // 固定辊台&摆渡车&运输车对接点
                String AR = DataControl._mStools.GetValueByKey("StandbyAR");

                // 获取当前任务最新的行车任务
                List<WCS_TASK_ITEM> itemList_ARF = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.摆渡车);
                if (itemList_ARF.Count != 0)
                {// 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_ARF)
                    {
                        switch (item.ITEM_ID)
                        {
                            case ItemId.摆渡车定位固定辊台:
                                #region 生成 固定辊台-摆渡车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }

                                // 固定辊台, 入库目的为摆渡车
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.固定辊台正向, DeviceType.固定辊台, null, item.DEVICE, ItemStatus.请求执行, com.FRT);
                                // 摆渡车辊台, 入库来源为固定辊台
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车正向, DeviceType.摆渡车, com.FRT, null, ItemStatus.请求执行, item.DEVICE);
                                // 更新状态=> 完成任务
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.摆渡车正向:
                                #region 生成定位任务
                                if (item.STATUS != ItemStatus.交接中 || DataControl._mTaskTools.GetDeviceType(item.LOC_TO) == DeviceType.运输车)
                                {
                                    break;
                                }
                                // 当属于中间三方对接点固定辊台时，直接等待对接
                                if (com.FRT == DataControl._mTaskTools.GetFRTByARFLoc(AR))
                                {
                                    // 生成摆渡车任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车定位运输车, DeviceType.摆渡车, null, AR, ItemStatus.交接中, item.DEVICE);
                                }
                                else
                                {
                                    // 生成摆渡车任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车定位运输车, DeviceType.摆渡车, null, AR, ItemStatus.请求执行, item.DEVICE);
                                }

                                // 获取对应任务
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.固定辊台正向, out int devID, out string dev))
                                {
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(devID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.摆渡车复位:
                                #region 解锁任务设备
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                // 解锁当前设备
                                DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskIn_ARF()", "摆渡车入库任务", com.WCS_NO, null, ex.Message);
            }
        }

        #endregion

        #endregion


        #region 出库流程

        #region 初步出库任务

        /// <summary>
        /// 执行出库清单确认
        /// </summary>
        public void Run_OutInitial()
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
                    int taskcount = DataControl._mMySql.GetCount("wcs_task_info", String.Format(@"TASK_TYPE = '{2}' and SITE = '{1}' and W_D_LOC = '{0}'",
                        area, TaskSite.未执行, TaskType.出库));
                    if (taskcount == 0) //无则退出
                    {
                        continue;
                    }

                    // 判断当前区域是否存在执行中的出库清单（最多2笔-4托）
                    int commandcount = DataControl._mMySql.GetCount("wcs_command_v", String.Format(@"TASK_TYPE = '{1}' and FRT in
                        (select distinct DEVICE from wcs_config_device where TYPE = '{2}' and AREA = '{0}')", area, TaskType.出库, DeviceType.固定辊台));
                    if (commandcount > 0) //有则退出
                    {
                        continue;
                    }

                    // 处理该区域出库任务
                    Task_OutInitial(area);
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
        /// <param name="area"></param>
        public void Task_OutInitial(string area)
        {
            try
            {
                // 获取当前区域可用行车
                String sql = String.Format(@"select DEVICE from wcs_config_device where TYPE = '{0}' and AREA = '{1}' and FLAG <> '{2}'",
                    DeviceType.行车, area, DeviceFlag.失效);
                DataTable dtABC = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtABC))
                {
                    return;
                }
                // 遍历处理行车对应任务
                foreach (DataRow dr in dtABC.Rows)
                {
                    string ABC = dr[0].ToString();
                    string sqltask = string.Format(@"select TASK_UID,RGV from wcs_task_out_range_v where ABC = '{0}' and SITE = '{1}'",
                        ABC, TaskSite.未执行);
                    DataTable dt = DataControl._mMySql.SelectAll(sqltask);
                    if (DataControl._mStools.IsNoData(dt))
                    {
                        continue;
                    }
                    // 确认取范围内任务 1~2 个
                    string RGV = dt.Rows[0]["RGV"].ToString();
                    string task1 = "";
                    string task2 = "";
                    if (dt.Rows.Count < 2)
                    {
                        task1 = dt.Rows[0]["TASK_UID"].ToString();
                    }
                    else
                    {
                        task1 = dt.Rows[0]["TASK_UID"].ToString();

                        task2 = dt.Rows[1]["TASK_UID"].ToString();
                    }
                    // 生成 WCS 出库清单
                    string wcsNo = DataControl._mTaskTools.CreateCommandOut(area, task1, task2);
                    // 生成 ITEM 初始任务
                    if (!string.IsNullOrEmpty(wcsNo))
                    {
                        CreateOutItem(wcsNo, ABC, RGV);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("Task_OutInitial()", "生成WCS出库数据", null, null, ex.Message);
            }
        }

        /// <summary>
        /// 生成WCS出库清单Item（初始任务）
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="ABC"></param>
        /// <param name="RGV"></param>
        public void CreateOutItem(string wcs_no, string ABC, string RGV)
        {
            try
            {
                // 获取WCS清单资讯
                String sql = String.Format(@"select * from wcs_command_v where WCS_NO = '{0}'", wcs_no);
                DataTable dtcommand = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtcommand))
                {
                    return;
                }
                WCS_COMMAND_V com = dtcommand.ToDataEntity<WCS_COMMAND_V>();

                // TASK 1 :生成行车库存定位任务
                string ABCloc = DataControl._mTaskTools.GetABCStockLoc(com.LOC_FROM_1);
                DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, com.TASK_UID_1, ItemId.行车库存定位, DeviceType.行车, null, ABCloc, ItemStatus.不可执行, ABC);

                // TASK 1 :生成运输车定位任务(默认先对接运输车辊台①)
                string RGVloc = DataControl._mTaskTools.GetRGVLoc(1, com.LOC_FROM_1);
                DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, com.TASK_UID_1, ItemId.运输车定位, DeviceType.运输车, null, RGVloc, ItemStatus.不可执行, RGV);

                if (!string.IsNullOrEmpty(com.TASK_UID_2))
                {
                    // TASK 1 :生成行车库存定位任务
                    string ABCloc2 = DataControl._mTaskTools.GetABCStockLoc(com.LOC_FROM_2);
                    DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, com.TASK_UID_2, ItemId.行车库存定位, DeviceType.行车, null, ABCloc2, ItemStatus.不可执行, ABC);

                    // TASK 1 :生成运输车定位任务(对接运输车辊台②)
                    string RGVloc2 = DataControl._mTaskTools.GetRGVLoc(2, com.LOC_FROM_2);
                    DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, com.TASK_UID_2, ItemId.运输车定位, DeviceType.运输车, null, RGVloc2, ItemStatus.不可执行, RGV);
                }

                // 生成摆渡车对接运输车任务
                string ARFloc = DataControl._mStools.GetValueByKey("StandbyAR");
                DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, com.WCS_NO, ItemId.摆渡车定位运输车, DeviceType.摆渡车, null, ARFloc, ItemStatus.不可执行);

                //更新WCS COMMAND状态——执行中
                DataControl._mTaskTools.UpdateCommand(wcs_no, CommandStep.执行中);
                //更新WCS TASK状态——任务中
                DataControl._mTaskTools.UpdateTaskByWCSNo(wcs_no, TaskSite.任务中);
            }
            catch (Exception ex)
            {
                //初始化
                DataControl._mTaskTools.DeleteCommand(wcs_no);
                DataControl._mTaskTools.UpdateTaskByWCSNo(wcs_no, TaskSite.未执行);
                DataControl._mTaskTools.DeleteItem(wcs_no);
                // 解锁设备数据状态
                DataControl._mTaskTools.DeviceUnLock(wcs_no);
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateOutItem()", "生成WCS出库清单Item", null, null, ex.Message);
            }
        }

        #endregion

        #region 各设备出库步骤

        public void Run_WCStask_Out()
        {
            try
            {
                // 获取执行中的出库清单
                String sql = String.Format(@"select * from wcs_command_v where TASK_TYPE = '{0}' and STEP in('{1}','{2}') order by CREATION_TIME",
                    TaskType.出库, CommandStep.执行中, CommandStep.结束);
                DataTable dtcommand = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtcommand))
                {
                    return;
                }
                List<WCS_COMMAND_V> comList = dtcommand.ToDataList<WCS_COMMAND_V>();
                // 遍历执行入库任务
                foreach (WCS_COMMAND_V com in comList)
                {
                    // 清单是[结束]状态 => 不作业
                    if (com.STEP == CommandStep.结束)
                    {
                        // 备份任务数据
                        DataControl._mTaskTools.BackupTask(com.WCS_NO);
                        // 解锁对应清单所有设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(com.WCS_NO);
                        continue;
                    }

                    TaskOut_ABC(com);
                    TaskOut_RGV(com);
                    TaskOut_ARF(com);

                    AllotItemDev(com);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void TaskOut_ABC(WCS_COMMAND_V com)
        {
            try
            {
                // 获取当前任务最新的行车任务
                List<WCS_TASK_ITEM> itemList_ABC = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.行车);
                if (itemList_ABC.Count != 0)
                {
                    // 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_ABC)
                    {
                        switch (item.ITEM_ID)
                        {
                            case ItemId.行车库存定位:
                                #region 生成取货
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 生成取货任务
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车取货, DeviceType.行车, null, item.LOC_TO, ItemStatus.请求执行, item.DEVICE);
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.行车轨道定位:
                                #region 生成放货
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应任务的运输车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.运输车定位, out int rgvID, out string rgv))
                                {
                                    // 生成放货任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车放货, DeviceType.行车, null, item.LOC_TO, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }

                                #endregion
                                break;
                            case ItemId.行车取货:
                                #region 生成轨道定位
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 生成轨道定位任务
                                String ABCloc = DataControl._mTaskTools.GetABCTrackLoc(item.TASK_NOW == com.TASK_UID_1 ? com.LOC_FROM_1 : com.LOC_FROM_2);
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.行车轨道定位, DeviceType.行车, null, ABCloc, ItemStatus.请求执行, item.DEVICE);

                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.行车放货:
                                #region 完成任务 || 启动另一笔任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 是否存在另一笔任务待完成
                                if (DataControl._mTaskTools.IsOtherTask(item.WCS_NO, item.TASK_NOW, item.DEVICE, ItemStatus.不可执行, out int ID))
                                {
                                    // 更新状态=> 请求执行
                                    DataControl._mTaskTools.UpdateItem(ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.请求执行);
                                }
                                else
                                {
                                    // 解锁当前设备
                                    DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);
                                }

                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskOut_ABC()", "行车出库任务", com.WCS_NO, null, ex.Message);
            }
        }

        public void TaskOut_RGV(WCS_COMMAND_V com)
        {
            try
            {
                // 获取当前任务最新的运输车任务
                List<WCS_TASK_ITEM> itemList_RGV = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.运输车);
                if (itemList_RGV.Count != 0)
                {
                    // 运输车间对接距离
                    String RD = DataControl._mStools.GetValueByKey("RGVDistance");
                    // 运输车[外]待命复位点
                    String R1 = DataControl._mStools.GetValueByKey("StandbyR1");
                    // 运输车[内]待命复位点
                    String R2 = DataControl._mStools.GetValueByKey("StandbyR2");
                    // 运输车于运输车对接点
                    String RR = DataControl._mStools.GetValueByKey("StandbyRR");
                    // 行车&运输车 放货安全高度
                    String AR = DataControl._mStools.GetValueByKey("ARRelease");

                    // 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_RGV)
                    {
                        // 获取同区内另一个RGV
                        string RGVother = DataControl._mTaskTools.GetOtherDev(item.DEVICE, DeviceType.运输车);
                        switch (item.ITEM_ID)
                        {
                            case ItemId.运输车定位:
                                #region 生成定位任务 || 对接任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应行车任务
                                if (!DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.行车放货, out int abcID, out string abc))
                                {
                                    // 获取行车设备资讯
                                    ABC _abc = new ABC(abc);
                                    int abcZ = DataControl._mStools.BytesToInt(_abc.CurrentZsite());
                                    /* 设备坐标偏差 */
                                    if (!DataControl._mTaskTools.GetLocByLessGap(abc, abcZ.ToString(), out string result, "Z"))
                                    {
                                        // 记录LOG
                                        DataControl._mTaskTools.RecordTaskErrLog("TaskOut_RGV()", "运输车出库流程获取行车位置[设备号,坐标]", abc, abcZ.ToString(), result);
                                        break;
                                    }

                                    if (DataControl._mTaskTools.GetItemStatus(abcID) == ItemStatus.完成任务 || Convert.ToInt32(result) >= Convert.ToInt32(AR))
                                    {
                                        // 是否存在另一笔任务待完成
                                        if (DataControl._mTaskTools.IsOtherTask(item.WCS_NO, item.TASK_NOW, item.DEVICE, ItemStatus.不可执行, out int ID))
                                        {
                                            // 更新状态=> 请求执行
                                            DataControl._mTaskTools.UpdateItem(ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.请求执行);
                                        }
                                        else
                                        {
                                            // 获取运输车对接资讯
                                            int P = DataControl._mTaskTools.GetCloseRGVLoc(RD, item.TASK_NOW, out string loc1, out string loc2);
                                            if (P == 1)
                                            {
                                                // 生成复位1
                                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.WCS_NO, ItemId.运输车复位1, DeviceType.运输车, null, R1, ItemStatus.请求执行, item.DEVICE);
                                            }
                                            else if (P == 2)
                                            {
                                                // 判断复位对接坐标
                                                if (Convert.ToInt32(loc1) >= Convert.ToInt32(R2))
                                                {
                                                    // 生成运输车[内]复位任务
                                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.WCS_NO, ItemId.运输车复位2, DeviceType.运输车, null, R2, ItemStatus.请求执行, item.DEVICE);
                                                    // 生成运输车[外]对接运输车[内]任务
                                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.WCS_NO, ItemId.运输车对接定位, DeviceType.运输车, null, RR, ItemStatus.不可执行, RGVother);
                                                }
                                                else
                                                {
                                                    // 生成运输车[内]复位任务
                                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.WCS_NO, ItemId.运输车复位2, DeviceType.运输车, null, loc1, ItemStatus.请求执行, item.DEVICE);
                                                    // 生成运输车[外]对接运输车[内]任务
                                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.WCS_NO, ItemId.运输车对接定位, DeviceType.运输车, null, loc2, ItemStatus.不可执行, RGVother);
                                                }
                                            }
                                        }
                                        // 更新状态=> 完成
                                        DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    }
                                }

                                #endregion
                                break;
                            case ItemId.运输车对接定位:
                                #region 生成 运输车-运输车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 获取对应任务的运输车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.运输车复位2, out int rgvID, out string rgv))
                                {
                                    // 运输车[内]辊台, 入库目的为运输车[外]
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车反向, DeviceType.运输车, null, item.DEVICE, ItemStatus.请求执行, rgv);
                                    // 运输车[外]辊台, 入库来源为运输车[内]
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车反向, DeviceType.运输车, rgv, null, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    DataControl._mTaskTools.UpdateItem(rgvID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                    // 运输车[内]复位任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, "ResetLoc", ItemId.运输车复位2, DeviceType.运输车, null, R2, ItemStatus.不可执行, rgv);
                                }

                                #endregion
                                break;
                            case ItemId.运输车反向:
                                #region 生成复位1任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                if (DataControl._mTaskTools.GetDeviceType(item.LOC_FROM) == DeviceType.运输车)
                                {
                                    // 生成运输车复位任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车复位1, DeviceType.运输车, null, R1, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID - 1, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                
                                #endregion
                                break;
                            case ItemId.运输车复位2:
                                #region 解锁任务设备
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                // 解锁当前设备
                                DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskOut_RGV()", "运输车出库任务", com.WCS_NO, null, ex.Message);
            }
        }

        public void TaskOut_ARF(WCS_COMMAND_V com)
        {
            try
            {
                // 获取当前任务最新的行车任务
                List<WCS_TASK_ITEM> itemList_ARF = DataControl._mTaskTools.GetItemList_Now(com.WCS_NO, DeviceType.摆渡车);
                if (itemList_ARF.Count != 0)
                {
                    // 固定辊台&摆渡车&运输车对接点
                    string AR = DataControl._mStools.GetValueByKey("StandbyAR");

                    // 遍历生成后续任务
                    foreach (WCS_TASK_ITEM item in itemList_ARF)
                    {
                        switch (item.ITEM_ID)
                        {
                            case ItemId.摆渡车定位运输车:
                                #region 生成 运输车-摆渡车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }

                                // 获取对应任务的运输车是否到位
                                if (DataControl._mTaskTools.IsStandBy(item.WCS_NO, item.TASK_NOW, ItemId.运输车复位1, out int rgvID, out string rgv))
                                {
                                    // 运输车辊台, 出库目的为摆渡车
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.运输车反向, DeviceType.运输车, null, item.DEVICE, ItemStatus.请求执行, rgv);
                                    // 摆渡车辊台, 出库来源为运输车
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车反向, DeviceType.摆渡车, rgv, null, ItemStatus.请求执行, item.DEVICE);
                                    // 更新状态=> 完成
                                    DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                    DataControl._mTaskTools.UpdateItem(rgvID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                }

                                #endregion
                                break;
                            case ItemId.摆渡车定位固定辊台:
                                #region 生成 运输车-摆渡车 辊台任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 摆渡车辊台, 出库目的为固定辊台
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车反向, DeviceType.摆渡车, null, com.FRT, ItemStatus.请求执行, item.DEVICE);
                                // 固定辊台, 出库来源为摆渡车
                                DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.固定辊台反向, DeviceType.固定辊台, item.DEVICE, null, ItemStatus.请求执行, com.FRT);
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.摆渡车反向:
                                #region 完成任务 || 生成定位任务
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                if (item.LOC_TO == com.FRT)
                                {
                                    //更新Task状态——完成
                                    DataControl._mTaskTools.UpdateTaskByWCSNo(com.WCS_NO, TaskSite.完成);
                                    // 通知WMS完成
                                    //DataControl._mHttp.DoStockOutFinishTask(com.LOC_TO_1, com.TASK_UID_1);
                                    //if (!string.IsNullOrEmpty(com.TASK_UID_2))
                                    //{
                                    //    DataControl._mHttp.DoStockOutFinishTask(com.LOC_TO_2, com.TASK_UID_2);
                                    //}
                                    break;
                                }
                                // 当属于中间三方对接点固定辊台时，直接等待对接
                                if (com.FRT == DataControl._mTaskTools.GetFRTByARFLoc(AR))
                                {
                                    // 生成摆渡车任务
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车定位固定辊台, DeviceType.摆渡车, null, AR, ItemStatus.交接中, item.DEVICE);
                                }
                                else
                                {
                                    // 生成摆渡车任务
                                    string ARFloc = DataControl._mTaskTools.GetARFLoc(com.FRT);
                                    DataControl._mTaskTools.CreateTaskItem(item.WCS_NO, item.TASK_NOW, ItemId.摆渡车定位固定辊台, DeviceType.摆渡车, null, ARFloc, ItemStatus.请求执行, item.DEVICE);
                                }

                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);

                                #endregion
                                break;
                            case ItemId.摆渡车复位:
                                #region 解锁任务设备
                                if (item.STATUS != ItemStatus.交接中)
                                {
                                    break;
                                }
                                // 更新状态=> 完成
                                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
                                // 解锁当前设备
                                DataControl._mTaskTools.UnLockByDevAndWcsNo(item.DEVICE, item.WCS_NO);

                                #endregion
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("TaskOut_ARF()", "摆渡车出库任务", com.WCS_NO, null, ex.Message);
            }
        }

        #endregion

        #endregion


        #region 分配设备

        public void AllotItemDev(WCS_COMMAND_V com)
        {
            try
            {
                // 获取任务所在作业区域
                String area = DataControl._mTaskTools.GetArea(com.FRT);
                if (String.IsNullOrEmpty(area))
                {
                    return;
                }
                // 获取任务所需设备职责
                String duty = DataControl._mTaskTools.GetDeviceDuty(com.WCS_NO);
                if (String.IsNullOrEmpty(duty))
                {
                    return;
                }
                // 获取待分配设备任务
                String sql = String.Format(@"select * from WCS_TASK_ITEM where WCS_NO = '{0}' and STATUS = '{1}' order by ID", com.WCS_NO, ItemStatus.不可执行);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
                {
                    return;
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                // 遍历分配设备
                foreach (WCS_TASK_ITEM item in itemList)
                {
                    // 已分配过设备 & 无锁定
                    if (!string.IsNullOrEmpty(item.DEVICE) && !DataControl._mTaskTools.IsDeviceLock(item.DEVICE))
                    {
                        // 更新状态
                        DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.请求执行);
                        // 锁定设备
                        DataControl._mTaskTools.DeviceLock(item.WCS_NO, item.DEVICE);

                        continue;
                    }
                    else if (string.IsNullOrEmpty(item.DEVICE))
                    {
                        // 分配设备 (行车内定不用分了···)
                        String device = String.Empty;
                        switch (item.DEV_TYPE)
                        {
                            case DeviceType.运输车:
                                // 获取作业区域内的运输车
                                List<WCS_CONFIG_DEVICE> dList_RGV = DataControl._mTaskTools.GetDeviceList(area, DeviceType.运输车, duty);
                                if (dList_RGV.Count != 0)
                                {
                                    // 确认其中最适合的运输车
                                    String rgv = GetSuitableRGV(item.LOC_TO, dList_RGV);
                                    if (String.IsNullOrEmpty(rgv))
                                    {
                                        break;
                                    }
                                    device = rgv;
                                }
                                break;
                            case DeviceType.摆渡车:
                                // 获取作业区域内的摆渡车
                                List<WCS_CONFIG_DEVICE> dList_ARF = DataControl._mTaskTools.GetDeviceList(area, DeviceType.摆渡车, duty);
                                if (dList_ARF.Count != 0)
                                {
                                    // 确认其中最适合的摆渡车
                                    String arf = GetSuitableARF(item.LOC_TO, dList_ARF);
                                    if (String.IsNullOrEmpty(arf))
                                    {
                                        break;
                                    }
                                    device = arf;

                                    // 挡路就让开
                                    if (item.LOC_TO == DataControl._mStools.GetValueByKey("StandbyAR") &&
                                        (item.ITEM_ID == ItemId.摆渡车定位固定辊台 || item.ITEM_ID == ItemId.摆渡车定位运输车))
                                    {
                                        string ARFother = DataControl._mTaskTools.GetOtherDev(device, DeviceType.摆渡车);
                                        if (!String.IsNullOrEmpty(ARFother))
                                        {
                                            string arfLoc = duty == DeviceDuty.负责出库 ? DataControl._mStools.GetValueByKey("StandbyF1") :
                                                DataControl._mStools.GetValueByKey("StandbyF2");
                                            ARF _arf = new ARF(ARFother);
                                            if (_arf.CurrentSite() != Convert.ToInt32(arfLoc))
                                            {
                                                // 生成复位任务
                                                DataControl._mTaskTools.CreateTaskItem(com.WCS_NO, "ResetLoc", ItemId.摆渡车复位, DeviceType.摆渡车, null, arfLoc, ItemStatus.不可执行, ARFother);
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        // 确认设备是否锁定
                        if (String.IsNullOrEmpty(device) || DataControl._mTaskTools.IsDeviceLock(device))
                        {
                            continue;
                        }
                        // 确认任务设备
                        DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.设备编号, device);
                        // 更新状态
                        DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.请求执行);
                        // 锁定设备
                        DataControl._mTaskTools.DeviceLock(item.WCS_NO, device);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("AllotItemDev()", "分配设备执行任务", com.WCS_NO, null, ex.Message);
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

                    // 仅有一个设备可用，直接选取该设备
                    if (dList.Count == 1)
                    {
                        arf = d.DEVICE;
                        break;
                    }
                    // 当前坐标值比较目标位置
                    else if (dev.CurrentSite() == LOC)    // 当前坐标值 = 目标位置
                    {
                        // 直接选取该设备
                        arf = d.DEVICE;
                        break;
                    }
                    else if (dev.CurrentSite() < LOC) // 当前坐标值 < 目标位置
                    {
                        // 距离比较值 >=（目标位置 - 当前坐标值）
                        if (X == 0 || X > (LOC - dev.CurrentSite()))
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
                        if (X == 0 || X > (dev.CurrentSite() - LOC))
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
                if (!String.IsNullOrEmpty(arf))
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
                    /* 设备坐标偏差 */
                    if (!DataControl._mTaskTools.GetLocByLessGap(d.DEVICE, dev.GetCurrentSite().ToString(), out string result))
                    {
                        // 记录LOG
                        DataControl._mTaskTools.RecordTaskErrLog("GetSuitableRGV()", "获取当前目标对应的合适运输车[设备号,坐标]", rgv, dev.GetCurrentSite().ToString(), result);
                        return null;
                    }

                    // 仅有一个设备可用，直接选取该设备
                    if (dList.Count == 1)
                    {
                        rgv = d.DEVICE;
                        break;
                    }
                    // 比较目标位置与对接点
                    else if (LOC <= RR)
                    {
                        // 仅获取位置于运输车[外]运输范围内的 RGV
                        if (Convert.ToInt32(result) <= RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }
                    else
                    {
                        // 仅获取位置于运输车[内]运输范围内的 RGV
                        if (Convert.ToInt32(result) > RR)
                        {
                            // 锁定设备
                            rgv = d.DEVICE;
                            break;
                        }
                    }

                    dev = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(rgv))
                {
                    dev = new RGV(rgv);
                    // 设备故障, 正在运行, 货物状态不为无货, 辊台状态不为停止 ==> 不可用
                    if (dev.DeviceStatus() == RGV.DeviceError || dev.ActionStatus() == RGV.Run || dev.GoodsStatus() != RGV.GoodsNoAll || dev.CurrentStatus() != RGV.RollerStop)
                    {
                        rgv = null;
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
        public String GetSuitableABC(String loc, List<WCS_CONFIG_DEVICE> dList, String duty)
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
                    // 仅有一个设备可用，直接选取该设备
                    if (dList.Count == 1)
                    {
                        abc = d.DEVICE;
                        break;
                    }
                    // 获取行车设备资讯
                    dev = new ABC(d.DEVICE);
                    int abcX = DataControl._mStools.BytesToInt(dev.CurrentXsite());
                    /* 设备坐标偏差 */
                    if (!DataControl._mTaskTools.GetLocByLessGap(d.DEVICE, abcX.ToString(), out string result, "X"))
                    {
                        // 记录LOG
                        DataControl._mTaskTools.RecordTaskErrLog("GetSuitableABC()", "获取当前目标对应的合适行车[设备号,坐标]", d.DEVICE, abcX.ToString(), result);
                        return null;
                    }

                    // 所需分区
                    int P = DataControl._mTaskTools.GetABCPartitionParity(d.AREA, X);
                    switch (duty)
                    {
                        case DeviceDuty.负责入库:
                            // 是否同一分区
                            if (P == DataControl._mTaskTools.GetABCPartitionParity(d.AREA, Convert.ToInt32(result)))
                            {
                                // 锁定设备
                                abc = d.DEVICE;
                                break;
                            }
                            continue;
                        case DeviceDuty.负责出库:
                            // 比较目标X轴值与对接点
                            if (X <= AA)
                            {
                                // 仅获取位置于行车[外]运输范围内的 ABC
                                if (Convert.ToInt32(result) <= AA)
                                {
                                    // 锁定设备
                                    abc = d.DEVICE;
                                    break;
                                }
                            }
                            else
                            {
                                // 仅获取位置于行车[内]运输范围内的 ABC
                                if (Convert.ToInt32(result) > AA)
                                {
                                    // 锁定设备
                                    abc = d.DEVICE;
                                    break;
                                }
                            }
                            continue;
                        default:
                            break;
                    }
                    dev = null;
                }

                // 检测设备是否可用
                if (!String.IsNullOrEmpty(abc))
                {
                    dev = new ABC(abc);
                    // 设备故障, 正在运行, 货物状态不为无货 ==> 不可用
                    if (dev.DeviceStatus() == ABC.DeviceError || dev.ActionStatus() == ABC.Run || dev.GoodsStatus() != ABC.GoodsNo)
                    {
                        abc = null;
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
                        site3 = String.IsNullOrEmpty(item.LOC_TO) ? FRT.GoodsReceive : FRT.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = FRT.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == FRT.RunFront && site3 == FRT.GoodsReceive) // 正向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes1) { site1 = FRT.RollerRun2; }
                        }
                        else if (site2 == FRT.RunFront && site3 == FRT.GoodsDeliver) // 正向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes1) { site1 = FRT.RollerRun1; }
                        }
                        else if (site2 == FRT.RunObverse && site3 == FRT.GoodsReceive) // 反向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes2) { site1 = FRT.RollerRun1; }
                        }
                        else if (site2 == FRT.RunObverse && site3 == FRT.GoodsDeliver) // 反向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (frt.GoodsStatus() == FRT.GoodsYes2) { site1 = FRT.RollerRun2; }
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
                        site3 = String.IsNullOrEmpty(item.LOC_TO) ? ARF.GoodsReceive : ARF.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = ARF.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == ARF.RunFront && site3 == ARF.GoodsReceive) // 正向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes1) { site1 = ARF.RollerRun2; }
                        }
                        else if (site2 == ARF.RunFront && site3 == ARF.GoodsDeliver) // 正向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes1) { site1 = ARF.RollerRun1; }
                        }
                        else if (site2 == ARF.RunObverse && site3 == ARF.GoodsReceive) // 反向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes2) { site1 = ARF.RollerRun1; }
                        }
                        else if (site2 == ARF.RunObverse && site3 == ARF.GoodsDeliver) // 反向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (arf.GoodsStatus() == ARF.GoodsYes2) { site1 = ARF.RollerRun2; }
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
                        site3 = String.IsNullOrEmpty(item.LOC_TO) ? RGV.GoodsReceive : RGV.GoodsDeliver;

                        // 确认辊台启动方式
                        site1 = RGV.RollerRunAll;   // 初始默认辊台全启
                        if (site2 == RGV.RunFront && site3 == RGV.GoodsReceive) // 正向接货
                        {
                            // 当只有1#辊台有货则只启动2#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes1) { site1 = RGV.RollerRun2; }
                        }
                        else if (site2 == RGV.RunFront && site3 == RGV.GoodsDeliver) // 正向送货
                        {
                            // 当只有1#辊台有货则只启动1#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes1) { site1 = RGV.RollerRun1; }
                        }
                        else if (site2 == RGV.RunObverse && site3 == RGV.GoodsReceive) // 反向接货
                        {
                            // 当只有2#辊台有货则只启动1#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes2) { site1 = RGV.RollerRun1; }
                        }
                        else if (site2 == RGV.RunObverse && site3 == RGV.GoodsDeliver) // 反向送货
                        {
                            // 当只有2#辊台有货则只启动2#辊台
                            if (rgv.GoodsStatus() == RGV.GoodsYes2) { site1 = RGV.RollerRun2; }
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
                    case ItemId.摆渡车定位运输车:
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
                        /* 设备坐标偏差 */
                        string resultRGV;
                        if (!DataControl._mTaskTools.GetLocByAddGap(item.DEVICE, item.LOC_TO, out resultRGV))
                        {
                            // 记录LOG
                            DataControl._mTaskTools.RecordTaskErrLog("CreateAndAddTaskList()", "生成设备指令&加入作业链表[设备号,坐标]", item.DEVICE, item.LOC_TO, resultRGV);
                            return;
                        }
                        // 提取目的位置
                        byte[] rgvloc = DataControl._mStools.IntToBytes(Convert.ToInt32(resultRGV));
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
                        /* 设备坐标偏差 */
                        string resultABC;
                        if (!DataControl._mTaskTools.GetLocByAddGap(item.DEVICE, item.LOC_TO, out resultABC))
                        {
                            // 记录LOG
                            DataControl._mTaskTools.RecordTaskErrLog("CreateAndAddTaskList()", "生成设备指令&加入作业链表[设备号,坐标]", item.DEVICE, item.LOC_TO, resultABC);
                            return;
                        }
                        // 提取目的位置
                        String[] LOC = resultABC.Split('-');
                        byte[] locX = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[0]) ? "0" : LOC[0]));
                        byte[] locY = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[1]) ? "0" : LOC[1]));
                        byte[] locZ = DataControl._mStools.IntToBytes(Convert.ToInt32(String.IsNullOrEmpty(LOC[2]) ? "0" : LOC[2]));
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
                DataControl._mTaskTools.UpdateItem(item.ID, item.WCS_NO, ItemColumnName.作业状态, ItemStatus.任务中);
            }
            catch (Exception ex)
            {
                // 记录LOG
                DataControl._mTaskTools.RecordTaskErrLog("CreateAndAddTaskList()", "生成并加入设备指令任务链表", item.WCS_NO, item.ITEM_ID, ex.Message);
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
                //Thread.Sleep(1000);
                if (!PublicParam.IsRunTaskLogic_I && !PublicParam.IsRunTaskLogic_O)
                {
                    continue;
                }
                try
                {
                    if (PublicParam.IsRunTaskLogic_I)
                    {
                        _task.Run_InInitial();
                        _task.Run_WCStask_In();
                        _task.Run_Order();
                    }

                    if (PublicParam.IsRunTaskLogic_O)
                    {
                        _task.Run_OutInitial();
                        _task.Run_WCStask_Out();
                        _task.Run_Order();
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }

}
