using Module;
using Module.DEV;
using ModuleManager.WCS;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Windows;
using WcsManager.DevModule;
using WcsManager.DevTask;

using ADS = WcsManager.Administartor;

namespace WcsManager
{
    public class MasterFRT : BaseMaster
    {
        #region [ 构造 ]

        /// <summary>
        /// 所有固定辊台设备数据
        /// </summary>
        public List<DevInfoFRT> devices;

        /// <summary>
        /// 所有固定辊台任务数据
        /// </summary>
        public List<TaskFRT> task;

        public MasterFRT()
        {
            devices = new List<DevInfoFRT>();
            task = new List<TaskFRT>();
            AddAllFrt();
        }

        #endregion

        #region [ 设备 ]

        /// <summary>
        /// 添加所有固定辊台信息
        /// </summary>
        private void AddAllFrt()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.固定辊台);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddFrt(new DevInfoFRT()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID1 = d.LOCK_ID1,
                    lockID2 = d.LOCK_ID2,
                    taskType = (TaskTypeEnum)d.FLAG,
                    _ = new DeviceFRT()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoFRT.GetDataOrder());

                if (d.IS_USEFUL == 0)
                {
                    ADS.mSocket.UpdateUserful(d.DEVICE, false);
                }

            }
        }

        /// <summary>
        /// 添加固定辊台信息
        /// </summary>
        private void AddFrt(DevInfoFRT frt)
        {
            if (!devices.Exists(c => c.devName == frt.devName))
            {
                devices.Add(frt);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void UpdateDevice(string devName, DeviceFRT dev)
        {
            if (devices.Exists(c => c.devName == devName))
            {
                devices.Find(c => c.devName == devName)._ = dev;
            }
        }

        #endregion

        #region [ 任务 ]

        /// <summary>
        /// 初始化任务
        /// </summary>
        public void InitTask(WCS_JOB_DETAIL d, TaskStatus ts)
        {
            task.Add(new TaskFRT()
            {
                id = d.ID,
                jobid = d.JOB_ID,
                area = d.AREA,
                tasktype = (TaskTypeEnum)d.TASK_TYPE,
                taskstatus = ts,
                fromdev = ADS.GetDevTypeEnum(d.DEV_FROM),
                todev = ADS.GetDevTypeEnum(d.DEV_TO),
                goodsnum = d.TAKE_NUM,
                device = string.IsNullOrEmpty(d.DEVICE) ? new DevInfoFRT() : devices.Find(c => c.devName == d.DEVICE)
            });
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(string jobid, string area, TaskTypeEnum tasktype, int goodsnum, DevType fromdev, DevType todev, string frt = null)
        {
            int id = ADS.ID;
            ADS.PlusID();
            TaskFRT t = new TaskFRT()
            {
                id = id,
                jobid = jobid,
                area = area,
                tasktype = tasktype,
                goodsnum = goodsnum,
                fromdev = fromdev,
                todev = todev,
                taskstatus = TaskStatus.init,
                frt = frt,
                device = new DevInfoFRT()
            };
            task.Add(t);
            t.InsertDB();
        }

        /// <summary>
        /// 检测任务状态
        /// </summary>
        //public void DoTask()
        //{
        //    if (task == null || task.Count == 0) return;

        //    foreach (TaskFRT t in task)
        //    {
        //        //if (!t.activie) continue;
        //        if (t.taskstatus == TaskStatus.finish) continue;
        //        if (string.IsNullOrEmpty(t.device.devName))
        //        {
        //            DevInfoFRT device = FindFreeDevice(t.area, t.tasktype, t.frt);
        //            if (device != null)
        //            {
        //                t.device = device;
        //                t.device.IsLockUnlock(true, t.jobid);
        //                t.UpdateDev();

        //                // AGV搬运 同辊台第2托货按收2货运行
        //                if (t.fromdev == DevType.AGV &&
        //                   t.device._.GoodsStatus != GoodsEnum.辊台满货 && t.device._.GoodsStatus != GoodsEnum.辊台无货)
        //                {
        //                    t.goodsnum = 2;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // 故障&异常
        //            if (t.device._.CommandStatus == CommandEnum.命令错误 || t.device._.DeviceStatus == DeviceEnum.设备故障)
        //            {
        //                continue;
        //            }

        //            switch (t.taskstatus)
        //            {
        //                case TaskStatus.init:
        //                    t.UpdateStatus(TaskStatus.totakesite);
        //                    break;

        //                case TaskStatus.totakesite:
        //                    // 无来源设备 直接前往送货点
        //                    if (t.fromdev == DevType.空设备)
        //                    {
        //                        t.UpdateStatus(TaskStatus.togivesite);
        //                    }
        //                    else
        //                    {
        //                        t.UpdateStatus(TaskStatus.ontakesite);
        //                    }
        //                    break;

        //                case TaskStatus.ontakesite:
        //                    // 判断是否启动辊台接货
        //                    if (t.takeready)
        //                    {
        //                        if (t.device._.GoodsStatus != GoodsEnum.辊台满货 && t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.device.StartTakeRoll(t.tasktype, t.goodsnum);
        //                        }

        //                        if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taking);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // ? JOB 更新请求
        //                        t.takeready = ADS.JobPartFRT_Take(t.jobid, t.fromdev);
        //                    }
        //                    break;

        //                case TaskStatus.taking:
        //                    bool isNext = false;
        //                    switch (t.fromdev)
        //                    {
        //                        case DevType.摆渡车:
        //                            if ((t.goodsnum == 1 && t.device._.GoodsStatus != GoodsEnum.辊台无货 && t.device._.GoodsStatus != GoodsEnum.辊台满货) ||
        //                                (t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
        //                            {
        //                                isNext = true;
        //                            }
        //                            break;
        //                        case DevType.AGV:
        //                            if ((t.goodsnum == 1 && t.device._.GoodsStatus != GoodsEnum.辊台无货) ||
        //                                (t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
        //                            {
        //                                isNext = true;
        //                            }
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                    if (isNext)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taked);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.StartTakeRoll(t.tasktype, t.goodsnum);
        //                    }
        //                    break;

        //                case TaskStatus.taked:
        //                    // AGV搬运 直接完成
        //                    if (t.tasktype == TaskTypeEnum.无)
        //                    {
        //                        t.UpdateStatus(TaskStatus.gived);
        //                    }
        //                    else
        //                    {
        //                        t.UpdateStatus(TaskStatus.togivesite);
        //                    }
        //                    break;

        //                case TaskStatus.togivesite:
        //                    // 无目的设备 直接送货中(出库暂无后续流程)
        //                    if (t.todev == DevType.空设备)
        //                    {
        //                        t.UpdateStatus(TaskStatus.giving);
        //                    }
        //                    else
        //                    {
        //                        t.UpdateStatus(TaskStatus.ongivesite);
        //                    }
        //                    break;

        //                case TaskStatus.ongivesite:
        //                    // 判断是否启动辊台送货
        //                    if (t.giveready)
        //                    {
        //                        if (t.device._.GoodsStatus != GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.device.StartGiveRoll(t.tasktype);
        //                        }

        //                        if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.giving);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // ? JOB 更新请求
        //                        t.giveready = ADS.JobPartFRT_Give(t.jobid);
        //                    }
        //                    break;

        //                case TaskStatus.giving:
        //                    if (t.device._.GoodsStatus == GoodsEnum.辊台无货)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.gived);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.StartGiveRoll(t.tasktype);
        //                    }
        //                    break;

        //                case TaskStatus.gived:
        //                    // 解锁设备、完成任务
        //                    t.device.IsLockUnlock(false);
        //                    t.device = new DevInfoFRT();
        //                    t.UpdateStatus(TaskStatus.finish);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //}

        public void DoTaskNew()
        {
            if (devices == null || devices.Count == 0) return;

            foreach (DevInfoFRT d in devices)
            {
                try
                {
                    if (!d.isUseful || !ADS.mSocket.IsConnected(d.devName) ||
                        d._.CommandStatus == CommandEnum.命令错误 ||
                        d._.DeviceStatus == DeviceEnum.设备故障)
                    {
                        continue;
                    }
                    if (d._.CurrentTask == TaskEnum.辊台任务 && d._.FinishTask == TaskEnum.辊台任务)
                    {
                        d.StopRoller();
                    }

                    switch (d.taskType)
                    {
                        case TaskTypeEnum.入库:
                            InTask:
                            if (d.isLock)
                            {
                                if (d._.GoodsStatus == GoodsEnum.辊台无货)
                                {
                                    // 锁定摆渡车
                                    if (ADS.mArf.LockARF(d.area, d.taskType))
                                    {
                                        // 解锁
                                        d.IsLockUnlockNew(false);
                                        d.StopRoller();
                                    }
                                }
                                else
                                {
                                    // 判断当前运输车任务
                                    if (ADS.mRgv.GetNowTaskType(d.area) != TaskTypeEnum.出库)
                                    {
                                        // 空闲摆渡车
                                        if (ADS.mArf.IsFreeARF(d.area, d.taskType))
                                        {
                                            int site = CommonSQL.GetArfByFrt(d.devName);
                                            if (site == 0) throw new Exception("无对应固定辊台-摆渡车对接点！");

                                            // 摆渡车来接货
                                            if (ADS.mArf.MovingButtSite(d.area, d.taskType, site))
                                            {
                                                // 启动摆渡车接货
                                                if (ADS.mArf.IsTaking(d.area, d.taskType))
                                                {
                                                    // 送货
                                                    d.StartGiveRoll(d.taskType);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // agv任务
                                int aid = ADS.mNDCControl.IsExistUnLoad(d.devName);

                                if (d._.ActionStatus == ActionEnum.停止)
                                {
                                    switch (d._.GoodsStatus)
                                    {
                                        case GoodsEnum.辊台无货:
                                            if (aid != 0)
                                            {
                                                // 只启动辊台2扫码
                                                d.StartTakeRoll(d.taskType, 1, true);
                                                d.isAGV = true;
                                            }
                                            break;
                                        case GoodsEnum.辊台中间有货:
                                        case GoodsEnum.辊台2有货:
                                            if (string.IsNullOrEmpty(d.lockID1))
                                            {
                                                //MessageBox.Show(d.devName + "辊台没扫到码！");
                                                throw new Exception(d.devName + "辊台没扫到码！");
                                            }
                                            else
                                            {
                                                if (aid != 0)
                                                {
                                                    // 接货到辊台2
                                                    d.StartTakeRoll(d.taskType, 2);
                                                    d.isAGV = true;
                                                }
                                                else
                                                {
                                                    // 接货到辊台1
                                                    d.StartTakeRoll(d.taskType, 1);
                                                    d.isAGV = false;
                                                }
                                            }
                                            break;
                                        case GoodsEnum.辊台1有货:
                                            if (aid != 0)
                                            {
                                                // 接货到辊台2
                                                d.StartTakeRoll(d.taskType, 2);
                                                d.isAGV = true;
                                            }
                                            else
                                            {
                                                if (!PublicParam.IsDoJobIn) break;

                                                // 每次只能做一个辊台
                                                if (IsLockInFRT(d.area)) break;

                                                // 是否满足任务
                                                if (!string.IsNullOrEmpty(d.lockID1))
                                                {
                                                    // 是否有超时任务
                                                    if (CommonSQL.IsTimeOut(d.lockID1))
                                                    {
                                                        // 锁定
                                                        d.IsLockUnlockNew(true, d.lockID1);
                                                        goto InTask;
                                                    }
                                                }
                                            }
                                            break;
                                        case GoodsEnum.辊台满货:

                                            if (!PublicParam.IsDoJobIn) break;

                                            // 每次只能做一个辊台
                                            if (IsLockInFRT(d.area)) break;

                                            // 是否满足任务
                                            if (!string.IsNullOrEmpty(d.lockID1) && !string.IsNullOrEmpty(d.lockID2))
                                            {
                                                // 锁定
                                                d.IsLockUnlockNew(true, d.lockID1, d.lockID2);
                                                goto InTask;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    if (aid != 0)
                                    {
                                        if (d.isAGV)
                                        {
                                            // NDC 请求 AGV 启动辊台卸货
                                            if (!ADS.mNDCControl.DoUnLoad(aid, out string result))
                                            {
                                                // LOG
                                                CommonSQL.LogErr("FRT.DoTaskNew()", "AGV辊台卸货[ID]", result, aid.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TaskTypeEnum.出库:
                            int p = CommonSQL.GetArfByFrt(d.devName);
                            if (p == 0) throw new Exception("无对应固定辊台-摆渡车对接点！");
                            if (d.isLock)
                            {
                                if (d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台无货)
                                {
                                    // 解锁
                                    d.IsLockUnlock(false);
                                }

                                // 是否有锁定摆渡车
                                if (ADS.mArf.IsLockARF(d.area, d.taskType, out int goods))
                                {
                                    // 摆渡车过来待命
                                    if (ADS.mArf.MovingButtSite(d.area, d.taskType, p))
                                    {
                                        // 都只有1货，接过去
                                        //if (goods == 1 && d._.GoodsStatus == GoodsEnum.辊台2有货)
                                        //{
                                        //    // 接货
                                        //    d.StartTakeRoll(d.taskType, 2);
                                        //    if (d._.ActionStatus == ActionEnum.运行中)
                                        //    {
                                        //        // 摆渡车送货
                                        //        ADS.mArf.IsGiving(d.area, d.taskType);
                                        //    }
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                // 空闲摆渡车
                                if (ADS.mArf.IsFreeARF(d.area, d.taskType))
                                {
                                    if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                        (d._.GoodsStatus == GoodsEnum.辊台2有货 &&
                                        d._.RollerStatus != RollerStatusEnum.辊台2启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                    {
                                        d.StopRoller();
                                        // 锁定
                                        d.IsLockUnlock(true);
                                    }
                                    else
                                    {
                                        if (d._.GoodsStatus != GoodsEnum.辊台无货)
                                        {
                                            // 接货
                                            d.StartTakeRoll(d.taskType, 1);
                                        }
                                    }
                                }
                                else
                                {
                                    // 是否有锁定摆渡车
                                    if (ADS.mArf.IsLockARF(d.area, d.taskType, out int goods))
                                    {
                                        // 摆渡车来送货
                                        if (ADS.mArf.MovingButtSite(d.area, d.taskType, p))
                                        {
                                            // 试探接货，防止光电干扰
                                            d.StartTakeRoll(d.taskType, 2);

                                            if (d._.GoodsStatus != GoodsEnum.辊台满货 || goods != 0)
                                            {
                                                // 接货
                                                d.StartTakeRoll(d.taskType, 2);
                                                if (d._.ActionStatus == ActionEnum.运行中)
                                                {
                                                    // 摆渡车送货
                                                    ADS.mArf.IsGiving(d.area, d.taskType);
                                                }
                                            }
                                            else
                                            {
                                                if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                                    (d._.GoodsStatus == GoodsEnum.辊台2有货 &&
                                                    d._.RollerStatus != RollerStatusEnum.辊台2启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                                {
                                                    d.StopRoller();
                                                    // 锁定
                                                    d.IsLockUnlock(true);
                                                }
                                                else
                                                {
                                                    if (d._.GoodsStatus != GoodsEnum.辊台无货)
                                                    {
                                                        // 接货
                                                        d.StartTakeRoll(d.taskType, 1);
                                                        if (d._.RollerStatus != RollerStatusEnum.辊台停止 && goods == 1)
                                                        {
                                                            // 摆渡车送货
                                                            ADS.mArf.IsGiving(d.area, d.taskType);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // LOG
                    CommonSQL.LogErr("FRT.DoTaskNew()", "固定辊台作业[设备]", (ex.Message + ex.Source), d.devName);
                    continue;
                }
            }
        }

        #endregion


        #region [ 流程判断 ]

        /// <summary>
        /// 寻找有效设备
        /// </summary>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        private DevInfoFRT FindFreeDevice(string area, TaskTypeEnum tasktype, string frt)
        {
            if (string.IsNullOrEmpty(frt))
            {
                return devices.Find(c => !c.isLock && !c.isUseful && c.area.Equals(area) && c.taskType == tasktype);
            }
            else
            {
                return devices.Find(c => c.devName.Equals(frt) && !c.isUseful);
            }
        }

        /// <summary>
        /// 是否任务相符
        /// </summary>
        public bool IsTaskConform(string jobid, TaskTypeEnum tt, TaskStatus ts)
        {
            if (task.Exists(c => c.jobid == jobid && c.tasktype == tt && c.taskstatus == ts))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取固定辊台设备名
        /// </summary>
        public string GetFrtName(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid && c.device != null))
            {
                return task.Find(c => c.jobid == jobid).device.devName;
            }

            return null;
        }

        /// <summary>
        /// 获取固定辊台所属区域
        /// </summary>
        public string GetFrtArea(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid && c.device != null))
            {
                return task.Find(c => c.jobid == jobid).device.area;
            }

            return null;
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="jobid"></param>
        public void OverTask(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                TaskFRT t = task.Find(c => c.jobid == jobid);
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    // 解锁设备
                    t.device.IsLockUnlock(false);
                }
                task.RemoveAll(c => c.jobid == jobid);
            }
        }



        /// <summary>
        /// 获取合适辊台
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public string GetRightFRT(string area, TaskTypeEnum tt)
        {
            try
            {
                string frt = "";
                if (devices.Exists(c => c.area == area && c.taskType == tt))
                {
                    foreach (DevInfoFRT d in devices.FindAll(c => c.area == area && c.taskType == tt))
                    {
                        if (ADS.mNDCControl.IsRedirectedMax(d.devName, out int t))
                        {
                            continue;
                        }

                        if (!d.isLock && d.isUseful && d._.GoodsStatus != GoodsEnum.辊台满货)
                        {
                            frt = d.devName;

                            if ((t == 0 && d._.GoodsStatus != GoodsEnum.辊台无货) ||
                                (t == 1 &&
                                ((d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台无货) ||
                                 (d._.ActionStatus == ActionEnum.运行中 && d._.GoodsStatus != GoodsEnum.辊台满货))))
                            {
                                break;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(frt) && area == "B01") frt = "FRT02";

                return frt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否有锁定入库辊台
        /// </summary>
        /// <returns></returns>
        public bool IsLockInFRT(string area)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == TaskTypeEnum.入库))
                {
                    foreach (DevInfoFRT a in devices.FindAll(c => c.area == area && c.taskType == TaskTypeEnum.入库))
                    {
                        if (a.isUseful && ADS.mSocket.IsConnected(a.devName))
                        {
                            if (a.isLock)
                            {
                                res = true;
                                break;
                            }
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否有空闲出库辊台
        /// </summary>
        /// <returns></returns>
        public bool IsFreeOutFRT(string area)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == TaskTypeEnum.出库))
                {
                    foreach (DevInfoFRT a in devices.FindAll(c => c.area == area && c.taskType == TaskTypeEnum.出库))
                    {
                        if (a.isUseful && ADS.mSocket.IsConnected(a.devName) &&
                            a._.GoodsStatus == GoodsEnum.辊台无货 && a._.ActionStatus == ActionEnum.停止 &&
                            a._.CommandStatus != CommandEnum.命令错误 && a._.DeviceStatus != DeviceEnum.设备故障)
                        {
                            if (a.isLock)
                            {
                                // 停止&解锁
                                a.StopTask();
                                a.IsLockUnlock(false);
                                res = true;
                                break;
                            }
                            else
                            {
                                res = true;
                                break;
                            }
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 扫码数据处理
        /// </summary>
        /// <param name="devName"></param>
        /// <param name="code"></param>
        public void GetCode(string devName, string code)
        {
            try
            {
                if (!code.StartsWith("@") || code.Split('@').Length != 2) return;

                if (devices.Exists(c => c.devName == devName))
                {
                    DevInfoFRT f = devices.Find(c => c.devName == devName);

                    //是否满足任务
                    if (CommonSQL.GetInTask(code, out string tid))
                    {
                        if (!string.IsNullOrEmpty(tid))
                        {
                            if (string.IsNullOrEmpty(f.lockID1))
                            {
                                // 请求WMS分配
                                if (ADS.AssignInSite(f.area, tid))
                                {
                                    f.IsLockUnlockNew(f.isLock, tid, f.lockID2);
                                }
                            }
                            else if (string.IsNullOrEmpty(f.lockID2))
                            {
                                if (tid != f.lockID1)
                                {
                                    // 请求WMS分配
                                    if (ADS.AssignInSite(f.area, tid))
                                    {
                                        f.IsLockUnlockNew(f.isLock, f.lockID1, tid);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("读取二维码[{0}]异常.", code));
                    }
                }
            }
            catch (Exception ex)
            {
                // LOG
                CommonSQL.LogErr("FRT.DoTaskNew()", "固定辊台作业[设备]", (ex.Message + ex.Source), devName);
            }

        }

        /// <summary>
        /// 停止所有辊台任务
        /// </summary>
        public void StopALLRoll()
        {
            try
            {
                if (devices == null || devices.Count == 0) return;

                foreach (DevInfoFRT d in devices.FindAll(c=>c._.CurrentTask == TaskEnum.辊台任务))
                {
                    if (ADS.mSocket.IsConnected(d.devName))
                    {
                        d.StopRoller();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
