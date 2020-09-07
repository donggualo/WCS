using System;
using System.Collections.Generic;
using Module;
using Module.DEV;
using ModuleManager.WCS;
using PubResourceManager;
using WcsManager.DevModule;
using WcsManager.DevTask;

using ADS = WcsManager.Administartor;

namespace WcsManager
{
    public class MasterARF : BaseMaster
    {
        #region [ 构造 ]

        /// <summary>
        /// 所有摆渡车设备数据
        /// </summary>
        public List<DevInfoARF> devices;

        /// <summary>
        /// 所有摆渡车任务数据
        /// </summary>
        public List<TaskARF> task;

        public MasterARF()
        {
            devices = new List<DevInfoARF>();
            task = new List<TaskARF>();
            AddAllArf();
        }

        #endregion

        #region [ 设备 ]

        /// <summary>
        /// 添加所有摆渡车信息
        /// </summary>
        private void AddAllArf()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.摆渡车);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddArf(new DevInfoARF()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID = d.LOCK_ID1,
                    taskType = (TaskTypeEnum)d.FLAG,
                    _ = new DeviceARF()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoARF.GetDataOrder());

                if (d.IS_USEFUL == 0)
                {
                    ADS.mSocket.UpdateUserful(d.DEVICE, false);
                }

            }
        }

        /// <summary>
        /// 添加摆渡车信息
        /// </summary>
        private void AddArf(DevInfoARF arf)
        {
            if (!devices.Exists(c => c.devName == arf.devName))
            {
                devices.Add(arf);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void UpdateDevice(string devName, DeviceARF dev)
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
            task.Add(new TaskARF()
            {
                id = d.ID,
                jobid = d.JOB_ID,
                area = d.AREA,
                tasktype = (TaskTypeEnum)d.TASK_TYPE,
                taskstatus = ts,
                takesite = d.TAKE_SITE_X,
                givesite = d.GIVE_SITE_X,
                goodsnum = d.TAKE_NUM,
                device = string.IsNullOrEmpty(d.DEVICE) ? new DevInfoARF() : devices.Find(c => c.devName == d.DEVICE)
            });
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(string jobid, string area, TaskTypeEnum tasktype, int goodsnum, int takesite, int givesite)
        {
            int id = ADS.ID;
            ADS.PlusID();
            TaskARF t = new TaskARF()
            {
                id = id,
                jobid = jobid,
                area = area,
                tasktype = tasktype,
                goodsnum = goodsnum,
                takesite = takesite,
                givesite = givesite,
                taskstatus = TaskStatus.init,
                device = new DevInfoARF()
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

        //    foreach (TaskARF t in task)
        //    {
        //        //if (!t.activie) continue;
        //        if (t.taskstatus == TaskStatus.finish) continue;
        //        if (string.IsNullOrEmpty(t.device.devName))
        //        {
        //            DevInfoARF device = FindFreeDevice(t.area, t.tasktype);
        //            if (device != null)
        //            {
        //                t.device = device;
        //                t.device.IsLockUnlock(true, t.jobid);
        //                t.UpdateDev();
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
        //                    if (t.takesite == 0)
        //                    {
        //                        // 入库作业接货固定辊台
        //                        if (t.tasktype == TaskTypeEnum.入库)
        //                        {
        //                            // ? JOB 更新请求
        //                            int Tsite = ADS.JobPartARF_Site(t.jobid);
        //                            if (Tsite != 0)
        //                            {
        //                                t.UpdateSite(Tsite);
        //                            }
        //                        }
        //                        break;
        //                    }

        //                    if (t.device._.CurrentSite == t.takesite)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ontakesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 防撞
        //                        if (IsHit(t.device, t.takesite))
        //                        {
        //                            t.device.StopTask();
        //                        }
        //                        else
        //                        {
        //                            t.device.ToSite(t.takesite);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.ontakesite:
        //                    // 判断是否启动辊台接货
        //                    if (t.takeready)
        //                    {
        //                        if (t.device._.GoodsStatus == GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
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
        //                        t.takeready = ADS.JobPartARF_Take(t.jobid, t.tasktype);
        //                    }
        //                    break;

        //                case TaskStatus.taking:
        //                    if ((t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货) ||
        //                        (t.goodsnum == 1 && t.tasktype == TaskTypeEnum.入库 && t.device._.GoodsStatus == GoodsEnum.辊台1有货) ||
        //                        (t.goodsnum == 1 && t.tasktype == TaskTypeEnum.出库 && t.device._.GoodsStatus == GoodsEnum.辊台2有货))
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
        //                    t.UpdateStatus(TaskStatus.togivesite);
        //                    break;

        //                case TaskStatus.togivesite:
        //                    if (t.givesite == 0)
        //                    {
        //                        // 出库作业送货固定辊台
        //                        if (t.tasktype == TaskTypeEnum.出库)
        //                        {
        //                            // ? JOB 更新请求
        //                            int Gsite = ADS.JobPartARF_Site(t.jobid);
        //                            if (Gsite != 0)
        //                            {
        //                                t.UpdateSite(Gsite);
        //                            }
        //                        }
        //                        break;
        //                    }

        //                    if (t.device._.CurrentSite == t.givesite)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ongivesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 防撞
        //                        if (IsHit(t.device, t.givesite))
        //                        {
        //                            t.device.StopTask();
        //                        }
        //                        else
        //                        {
        //                            t.device.ToSite(t.givesite);
        //                        }
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
        //                        t.giveready = ADS.JobPartARF_Give(t.jobid, t.tasktype);
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
        //                    t.device = new DevInfoARF();
        //                    t.UpdateStatus(TaskStatus.finish);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //}

        #endregion


        #region [ 流程判断 ]

        /// <summary>
        /// 寻找有效设备
        /// </summary>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        private DevInfoARF FindFreeDevice(string area, TaskTypeEnum tasktype)
        {
            return devices.Find(c => !c.isLock && !c.isUseful && c.area.Equals(area) && c.taskType == tasktype);
        }

        /// <summary>
        /// 是否会撞车
        /// </summary>
        private bool IsHit(DevInfoARF da, int site)
        {
            DevInfoARF arf = devices.Find(c => c.area.Equals(da.area) && c.devName != da.devName);
            if (arf == null) return false;
            int safe = ADS.mDis.GetArfSafeDis(da.area);
            int loc;
            switch (da.taskType)
            {
                case TaskTypeEnum.入库:
                    loc = ADS.mDis.GetArfStandByP2(da.area);
                    if (arf._.CurrentSite <= (site + safe) && arf._.ActionStatus == ActionEnum.运行中)
                    {
                        return true;
                    }
                    break;

                case TaskTypeEnum.出库:
                    loc = ADS.mDis.GetArfStandByP1(da.area);
                    if (arf._.CurrentSite >= (site - safe) && arf._.ActionStatus == ActionEnum.运行中)
                    {
                        return true;
                    }
                    break;

                default:
                    return true;
            }
            arf.ToSite(loc);
            return false;
        }

        /// <summary>
        /// 是否任务相符
        /// </summary>
        public bool IsTaskConform(string jobid, TaskStatus ts)
        {
            if (task.Exists(c => c.jobid == jobid && c.taskstatus == ts))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取摆渡车任务状态
        /// </summary>
        public TaskStatus GetStatusARF(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                return task.Find(c => c.jobid == jobid).taskstatus;
            }

            return TaskStatus.init;
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="jobid"></param>
        public void OverTask(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                TaskARF t = task.Find(c => c.jobid == jobid);
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    // 解锁设备
                    t.device.IsLockUnlock(false);
                }
                task.RemoveAll(c => c.jobid == jobid);
            }
        }



        /// <summary>
        /// 移动对接点
        /// </summary>
        public bool MovingButtSite(string area, TaskTypeEnum tt, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);

                    if (!ADS.mSocket.IsConnected(a.devName) ||
                        a._.CommandStatus == CommandEnum.命令错误 ||
                        a._.DeviceStatus == DeviceEnum.设备故障)
                    {
                        return res;
                    }

                    if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                    {
                        a.StopRoller();
                    }

                    if (Standby(area, tt == TaskTypeEnum.入库 ? TaskTypeEnum.出库 : TaskTypeEnum.入库, site))
                    {
                        if (a._.CurrentSite != site)
                        {
                            if (a._.ActionStatus == ActionEnum.停止)
                            {
                                a.ToSite(site);
                            }
                        }
                        else
                        {
                            res = true;
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
        /// 移至待命点
        /// </summary>
        /// <returns></returns>
        public bool Standby(string area, TaskTypeEnum tt, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);

                    if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                    {
                        a.StopRoller();
                    }
                    if (a.isLock)
                    {
                        if (a._.CurrentSite != site)
                        {
                            res = true;
                        }
                    }
                    else
                    {
                        int sb = tt == TaskTypeEnum.入库 ? ADS.mDis.GetArfStandByP1(a.area) : ADS.mDis.GetArfStandByP2(a.area);
                        if (Math.Abs(a._.CurrentSite - sb) <= 2)
                        {
                            res = true;
                        }
                        else
                        {
                            if (a.isUseful && ADS.mSocket.IsConnected(a.devName) && a._.ActionStatus == ActionEnum.停止)
                            {
                                a.ToSite(sb);
                            }
                        }
                    }
                }
                else
                {
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否有空闲摆渡车
        /// </summary>
        /// <returns></returns>
        public bool IsFreeARF(string area, TaskTypeEnum tt)
        {
            try
            {
                ChangeFlag(area);

                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);

                    if (ADS.mSocket.IsConnected(a.devName) &&
                        a._.GoodsStatus != GoodsEnum.辊台满货 &&
                        a._.CommandStatus != CommandEnum.命令错误 && a._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                        {
                            a.StopRoller();
                        }
                        if (a.isLock)
                        {
                            if (a._.GoodsStatus == GoodsEnum.辊台无货 && a._.ActionStatus == ActionEnum.停止)
                            {
                                // 停止&解锁
                                a.StopTask();
                                a.IsLockUnlock(false);
                                res = true;
                            }
                        }
                        else
                        {
                            res = true;
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
        /// 是否有空闲出库摆渡车
        /// </summary>
        /// <returns></returns>
        public bool IsFreeOutARF(string area)
        {
            try
            {
                ChangeFlag(area);

                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == TaskTypeEnum.出库))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == TaskTypeEnum.出库);

                    if (a.isUseful && ADS.mSocket.IsConnected(a.devName) &&
                        a._.GoodsStatus == GoodsEnum.辊台无货 && a._.ActionStatus == ActionEnum.停止 &&
                        a._.CommandStatus != CommandEnum.命令错误 && a._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                        {
                            a.StopRoller();
                        }
                        if (a.isLock)
                        {
                            // 停止&解锁
                            a.StopTask();
                            a.IsLockUnlock(false);
                        }
                        else
                        {
                            res = true;
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
        /// 是否有锁定摆渡车
        /// </summary>
        /// <returns></returns>
        public bool IsLockARF(string area, TaskTypeEnum tt, out int goods)
        {
            try
            {
                ChangeFlag(area);

                bool res = false;
                goods = 0;
                if (devices.Exists(c => c.isUseful && c.area == area && c.taskType == tt))
                {
                    DevInfoARF a = devices.Find(c => c.isUseful && c.area == area && c.taskType == tt);

                    if (ADS.mSocket.IsConnected(a.devName) &&
                        a._.CommandStatus != CommandEnum.命令错误 && a._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                        {
                            a.StopRoller();
                        }
                        switch (a._.GoodsStatus)
                        {
                            case GoodsEnum.辊台1有货:
                            case GoodsEnum.辊台2有货:
                            case GoodsEnum.辊台中间有货:
                                goods = 1;
                                break;
                            case GoodsEnum.辊台满货:
                                goods = 2;
                                break;
                            default:
                                break;
                        }

                        if (a.isLock)
                        {
                            if (a._.GoodsStatus == GoodsEnum.辊台无货 &&
                               (a._.ActionStatus == ActionEnum.停止 || a._.FinishTask == TaskEnum.辊台任务))
                            {
                                // 停止&解锁
                                a.StopTask();
                                a.IsLockUnlock(false);
                            }
                            else
                            {
                                res = true;
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
        /// 锁定摆渡车
        /// </summary>
        /// <returns></returns>
        public bool LockARF(string area, TaskTypeEnum tt)
        {
            try
            {
                ChangeFlag(area);

                bool res = false;
                if (devices.Exists( c => c.isUseful && c.area == area && c.taskType == tt))
                {
                    DevInfoARF a = devices.Find(c => c.isUseful && c.area == area && c.taskType == tt);
                    if (ADS.mSocket.IsConnected(a.devName) &&
                        a._.CommandStatus == CommandEnum.命令正常 && a._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (a._.CurrentTask == TaskEnum.辊台任务 && a._.FinishTask == TaskEnum.辊台任务)
                        {
                            a.StopRoller();
                        }
                        if (a._.GoodsStatus == GoodsEnum.辊台无货)
                        {
                            if (a._.ActionStatus == ActionEnum.停止)
                            {
                                res = true;
                            }
                        }
                        else
                        {
                            if (a.isLock)
                            {
                                res = true;
                            }
                            else
                            {
                                bool run = false;
                                switch (tt)
                                {
                                    case TaskTypeEnum.入库:
                                        if ((a._.GoodsStatus == GoodsEnum.辊台满货 && a._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                            (a._.GoodsStatus == GoodsEnum.辊台1有货 && 
                                            a._.RollerStatus != RollerStatusEnum.辊台1启动 && a._.RollerStatus != RollerStatusEnum.辊台全启动))
                                        {
                                            run = true;
                                        }
                                        break;
                                    case TaskTypeEnum.出库:
                                        if ((a._.GoodsStatus == GoodsEnum.辊台满货 && a._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                            (a._.GoodsStatus == GoodsEnum.辊台2有货 &&
                                            a._.RollerStatus != RollerStatusEnum.辊台2启动 && a._.RollerStatus != RollerStatusEnum.辊台全启动))
                                        {
                                            run = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (run)
                                {
                                    // 停止 & 锁定
                                    a.StopTask();
                                    a.IsLockUnlock(true);
                                    res = true;
                                }
                                else
                                {
                                    if (a._.ActionStatus == ActionEnum.停止)
                                    {
                                        a.StartTakeRoll(tt, 2);
                                    }
                                }
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
        /// 摆渡车载货状态
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public GoodsEnum GetGoodsStatus(string area, TaskTypeEnum tt)
        {
            try
            {
                return devices.Find(c => c.area == area && c.taskType == tt && c.isUseful)._.GoodsStatus;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否锁定
        /// </summary>
        /// <returns></returns>
        public bool IsLocking(string area, string lockid)
        {
            try
            {
                return devices.Exists(c => c.area == area && c.isLock && c.lockID == lockid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 锁定摆渡车
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public bool IsLock(string area, TaskTypeEnum tt, string lockid)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);
                    switch (tt)
                    {
                        case TaskTypeEnum.入库:
                            if (a.isLock && a.lockID == lockid)
                            {
                                res = true;
                            }
                            else if (a._.GoodsStatus == GoodsEnum.辊台无货)
                            {
                                a.IsLockUnlock(true, lockid);
                            }
                            break;
                        case TaskTypeEnum.出库:
                            if (a.isLock && a.lockID == lockid)
                            {
                                res = true;
                            }
                            else if (a._.GoodsStatus != GoodsEnum.辊台无货)
                            {
                                a.IsLockUnlock(true, lockid);
                            }
                            break;
                        default:
                            break;
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
        /// 摆渡车是否到位
        /// </summary>
        public bool IsArfOk(string area, TaskTypeEnum tt, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful && c.isLock))
                {
                    DevInfoARF arf = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful && c.isLock);
                    if (arf._.ActionStatus == ActionEnum.停止 || ADS.mSocket.IsConnected(arf.devName) ||
                        arf._.CommandStatus == CommandEnum.命令正常 || arf._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (arf._.CurrentSite == site)
                        {
                            res = true;
                        }
                        else
                        {
                            int safe = ADS.mDis.GetArfSafeDis(area);
                            if (devices.Exists(c => c.area == area && c.taskType != tt && !c.isLock))
                            {
                                DevInfoARF a = devices.Find(c => c.area == area && c.taskType != tt && !c.isLock);
                                if (ADS.mSocket.IsConnected(a.devName) || a._.CommandStatus == CommandEnum.命令正常 || a._.DeviceStatus == DeviceEnum.设备正常)
                                {
                                    int loc = 0;
                                    switch (a.taskType)
                                    {
                                        case TaskTypeEnum.入库:
                                            loc = ADS.mDis.GetArfStandByP1(area);
                                            if (a._.CurrentSite <= (site - safe))
                                            {
                                                arf.ToSite(site);
                                            }
                                            else if (a._.ActionStatus == ActionEnum.停止)
                                            {
                                                a.ToSite(loc);
                                            }
                                            break;
                                        case TaskTypeEnum.出库:
                                            loc = ADS.mDis.GetArfStandByP2(area);
                                            if (a._.CurrentSite >= (site + safe))
                                            {
                                                arf.ToSite(site);
                                            }
                                            else if (a._.ActionStatus == ActionEnum.停止)
                                            {
                                                a.ToSite(loc);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
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
        /// 摆渡车是否接货中
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public bool IsTaking(string area, TaskTypeEnum tt)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);
                    if (ADS.mSocket.IsConnected(a.devName) && a._.CommandStatus == CommandEnum.命令正常 && a._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (a._.ActionStatus == ActionEnum.停止)
                        {
                            a.StartTakeRoll(tt, 2);
                        }
                        else
                        {
                            if (a._.CurrentTask == TaskEnum.辊台任务) res = true;
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
        /// 停止摆渡车接货
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <param name="goodsNum"></param>
        public bool IsStopTaking(string area, TaskTypeEnum tt, int goodsNum)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt))
                {
                    DevInfoARF arf = devices.Find(c => c.area == area && c.taskType == tt);
                    if ((goodsNum == 2 && arf._.GoodsStatus == GoodsEnum.辊台满货) ||
                        (goodsNum == 1 && arf._.GoodsStatus == GoodsEnum.辊台1有货 && tt == TaskTypeEnum.入库) ||
                        (goodsNum == 1 && arf._.GoodsStatus == GoodsEnum.辊台2有货 && tt == TaskTypeEnum.出库))
                    {
                        if (arf._.ActionStatus == ActionEnum.停止 && arf._.FinishTask == TaskEnum.停止辊台任务)
                        {
                            res = true;
                        }
                        else
                        {
                            arf.StopRoller();
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
        /// 摆渡车是否送货中
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        public bool IsGiving(string area, TaskTypeEnum tt)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt && c.isUseful))
                {
                    DevInfoARF a = devices.Find(c => c.area == area && c.taskType == tt && c.isUseful);
                    if (ADS.mSocket.IsConnected(a.devName) && a._.CommandStatus == CommandEnum.命令正常 && a._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (a._.ActionStatus == ActionEnum.停止)
                        {
                            a.StartGiveRoll(tt);
                        }
                        else
                        {
                            if (a._.CurrentTask == TaskEnum.辊台任务)
                            {
                                if (a._.GoodsStatus != GoodsEnum.辊台无货)
                                {
                                    if (a._.CurrentTask == TaskEnum.辊台任务) a.StartGiveRoll(tt);
                                }
                                else
                                {
                                    if (a._.FinishTask == TaskEnum.辊台任务)
                                    {
                                        a.StopRoller();
                                    }
                                }
                                res = true;
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
        /// 停止摆渡车送货
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tt"></param>
        /// <param name="goodsNum"></param>
        public bool IsStopGiving(string area, TaskTypeEnum tt)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.taskType == tt))
                {
                    DevInfoARF arf = devices.Find(c => c.area == area && c.taskType == tt);
                    if (arf._.GoodsStatus == GoodsEnum.辊台无货)
                    {
                        if (arf._.ActionStatus == ActionEnum.停止 && arf._.FinishTask == TaskEnum.停止辊台任务)
                        {
                            if (!arf.isLock)
                            {
                                res = true;
                            }
                            else
                            {
                                // 送完货直接解锁
                                arf.IsLockUnlock(false);
                            }
                        }
                        else
                        {
                            arf.StopRoller();
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
        /// 停止所有辊台任务
        /// </summary>
        public void StopALLRoll()
        {
            try
            {
                if (devices == null || devices.Count == 0) return;

                foreach (DevInfoARF d in devices.FindAll(c => c._.CurrentTask == TaskEnum.辊台任务))
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


        /// <summary>
        /// 改变属性
        /// </summary>
        /// <param name="area"></param>
        private void ChangeFlag(string area)
        {
            try
            {
                if (devices == null || devices.Count == 0) return;

                if (devices.Exists(c => c.area == area && c.isUseful))
                {
                    if (devices.FindAll(c => c.area == area && c.isUseful).Count == 1)
                    {
                        DevInfoARF a = devices.Find(c => c.area == area && c.isUseful);
                        if (!ADS.mSocket.IsConnected(a.devName) || a.isLock ||
                            a._.ActionStatus == ActionEnum.运行中 || a._.GoodsStatus != GoodsEnum.辊台无货)
                        {
                            return;
                        }

                        if (PublicParam.IsDoJobIn && !PublicParam.IsDoJobOut)
                        {
                            if (a.taskType == TaskTypeEnum.入库) return;

                            // 判断当前运输车任务
                            if (ADS.mRgv.GetNowTaskType(area) != TaskTypeEnum.出库)
                            {
                                a.UpdateFlag(TaskTypeEnum.入库);
                            }
                        }
                        else if (!PublicParam.IsDoJobIn && PublicParam.IsDoJobOut)
                        {
                            if (a.taskType == TaskTypeEnum.出库) return;

                            a.UpdateFlag(TaskTypeEnum.出库);
                        }
                    }
                    else
                    {
                        if (devices.FindAll(c => c.area == area && c.taskType == TaskTypeEnum.入库).Count != 1 ||
                            devices.FindAll(c => c.area == area && c.taskType == TaskTypeEnum.出库).Count != 1)
                        {
                            DevInfoARF a1 = devices.Find(c => c.area == area);
                            DevInfoARF a2 = devices.Find(c => c.area == area && c.devName != a1.devName);
                            if (a1._.CurrentSite == 0 || a2._.CurrentSite == 0)
                            {
                                throw new Exception(area + " 区域有摆渡车无效坐标0！");
                            }

                            if (a1._.GoodsStatus != GoodsEnum.辊台无货 || a2._.GoodsStatus != GoodsEnum.辊台无货)
                            {
                                throw new Exception(area + " 区域有停启摆渡车有货，无法分配属性！");
                            }

                            if (a1._.CurrentSite <= a2._.CurrentSite)
                            {
                                a1.UpdateFlag( TaskTypeEnum.入库);
                                a2.UpdateFlag( TaskTypeEnum.出库);
                            }
                            else
                            {
                                a1.UpdateFlag(TaskTypeEnum.出库);
                                a2.UpdateFlag(TaskTypeEnum.入库);
                            }
                        }
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
