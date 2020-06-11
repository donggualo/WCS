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
    public class MasterRGV : BaseMaster
    {
        #region [ 构造 ]

        /// <summary>
        /// 所有运输车设备数据
        /// </summary>
        public List<DevInfoRGV> devices;

        /// <summary>
        /// 所有运输车任务数据
        /// </summary>
        public List<TaskRGV> task;

        public MasterRGV()
        {
            devices = new List<DevInfoRGV>();
            task = new List<TaskRGV>();
            AddAllRgv();
        }

        #endregion

        #region [ 设备 ]

        /// <summary>
        /// 添加所有运输车信息
        /// </summary>
        private void AddAllRgv()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.运输车);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddRgv(new DevInfoRGV()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID = d.LOCK_ID,
                    flag = (DevFlag)d.FLAG,
                    gap = d.GAP_X,
                    limit = d.LIMIT_X,
                    _ = new DeviceRGV()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoRGV.GetDataOrder());
            }
        }

        /// <summary>
        /// 添加运输车信息
        /// </summary>
        private void AddRgv(DevInfoRGV rgv)
        {
            if (!devices.Exists(c => c.devName == rgv.devName))
            {
                devices.Add(rgv);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void UpdateDevice(string devName, DeviceRGV dev)
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
            task.Add(new TaskRGV()
            {
                id = d.ID,
                jobid = d.JOB_ID,
                taskid = d.TASK_ID,
                area = d.AREA,
                tasktype = (TaskTypeEnum)d.TASK_TYPE,
                taskstatus = ts,
                takesite = d.TAKE_SITE_X,
                givesite = d.GIVE_SITE_X,
                takeNum = d.TAKE_NUM,
                giveNum = d.GIVE_NUM,
                fromdev = ADS.GetDevTypeEnum(d.DEV_FROM),
                todev = ADS.GetDevTypeEnum(d.DEV_TO),
                flag = (DevFlag)d.DEV_FLAG,
                device = string.IsNullOrEmpty(d.DEVICE) ? new DevInfoRGV() : devices.Find(c => c.devName == d.DEVICE)
            });
        }

        /// <summary>
        /// 按类添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(string jobid, MTask taskid, string area, TaskTypeEnum tasktype, MPoint goods1, MPoint goods2, int takeNum, int giveNum, DevFlag flag = DevFlag.不参考)
        {
            switch (tasktype)
            {
                case TaskTypeEnum.入库:
                    AddTaskIN(jobid, taskid, area, tasktype, goods1, goods2, takeNum, giveNum);
                    break;
                case TaskTypeEnum.出库:
                    AddTaskOUT(jobid, taskid, area, tasktype, goods1, goods2, takeNum, giveNum, flag);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 添加入库任务
        /// </summary>
        private void AddTaskIN(string jobid, MTask taskid, string area, TaskTypeEnum tasktype, MPoint goodsFirst, MPoint goodsSecond, int takeNum, int giveNum)
        {
            // 获取运输车轨道中点对接坐标
            int CentetNear = ADS.mDis.GetRgvCenterP(area);
            int CenterFar = CentetNear + ADS.mDis.GetRgvButtDis(area);
            switch (takeNum)
            {
                case 1:
                    if (goodsFirst.givesite < CenterFar) // 近
                    {
                        AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 1, 1, DevType.摆渡车, DevType.行车, DevFlag.靠近入库口);
                    }
                    else  // 远
                    {
                        AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst.takesite, CentetNear, 1, 1, DevType.摆渡车, DevType.运输车, DevFlag.靠近入库口);
                        AddRGV(jobid, taskid.task2, area, tasktype, CenterFar, goodsFirst.givesite, 1, 1, DevType.运输车, DevType.行车, DevFlag.远离入库口);
                    }
                    break;
                case 2:
                    if (goodsSecond.givesite < CenterFar) // 全近
                    {
                        AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 2, 1, DevType.摆渡车, DevType.行车, DevFlag.靠近入库口);
                        AddRGV(jobid, taskid.task2, area, tasktype, goodsSecond, 1, 1, DevType.空设备, DevType.行车, DevFlag.靠近入库口);
                    }
                    else if (goodsFirst.givesite >= CenterFar) // 全远
                    {
                        AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst.takesite, CentetNear, 2, 2, DevType.摆渡车, DevType.运输车, DevFlag.靠近入库口);
                        AddRGV(jobid, taskid.task1, area, tasktype, CenterFar, goodsFirst.givesite, 2, 1, DevType.运输车, DevType.行车, DevFlag.远离入库口);
                        AddRGV(jobid, taskid.task2, area, tasktype, goodsSecond, 1, 1, DevType.空设备, DevType.行车, DevFlag.远离入库口);
                    }
                    else  // 一近一远
                    {
                        AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 2, 1, DevType.摆渡车, DevType.行车, DevFlag.靠近入库口);
                        AddRGV(jobid, taskid.task2, area, tasktype, goodsFirst.givesite, CentetNear, 1, 1, DevType.空设备, DevType.运输车, DevFlag.靠近入库口);
                        AddRGV(jobid, taskid.task2, area, tasktype, CenterFar, goodsSecond.givesite, 1, 1, DevType.运输车, DevType.行车, DevFlag.远离入库口);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 添加出库任务
        /// </summary>
        private void AddTaskOUT(string jobid, MTask taskid, string area, TaskTypeEnum tasktype, MPoint goodsFirst, MPoint goodsSecond, int takeNum, int giveNum, DevFlag flag)
        {
            // 计算运输车轨道对接坐标
            int RGVDistance = 100;
            int NearP;
            int FarP;

            switch (takeNum)
            {
                case 1:
                    switch (flag)
                    {
                        case DevFlag.靠近入库口:
                            AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 1, 1, DevType.行车, DevType.摆渡车, DevFlag.靠近入库口);
                            break;
                        case DevFlag.远离入库口:
                            FarP = (goodsFirst.takesite + goodsFirst.givesite) / 2;
                            NearP = FarP - RGVDistance;
                            AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst.takesite, FarP, 1, 1, DevType.行车, DevType.运输车, DevFlag.远离入库口);
                            AddRGV(jobid, taskid.task1, area, tasktype, NearP, goodsFirst.givesite, 1, 1, DevType.运输车, DevType.摆渡车, DevFlag.靠近入库口);
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    switch (flag)
                    {
                        case DevFlag.靠近入库口:
                            AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 1, 1, DevType.行车, DevType.空设备, DevFlag.靠近入库口);
                            AddRGV(jobid, taskid.task2, area, tasktype, goodsSecond, 2, 2, DevType.行车, DevType.摆渡车, DevFlag.靠近入库口);
                            break;
                        case DevFlag.远离入库口:
                            FarP = (goodsSecond.takesite + goodsSecond.givesite) / 2;
                            NearP = FarP - RGVDistance;
                            AddRGV(jobid, taskid.task1, area, tasktype, goodsFirst, 1, 1, DevType.行车, DevType.空设备, DevFlag.远离入库口);
                            AddRGV(jobid, taskid.task2, area, tasktype, goodsSecond.takesite, FarP, 2, 2, DevType.行车, DevType.运输车, DevFlag.远离入库口);
                            AddRGV(jobid, taskid.task2, area, tasktype, NearP, goodsSecond.givesite, 2, 2, DevType.运输车, DevType.摆渡车, DevFlag.靠近入库口);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        private void AddRGV(string jobid, string taskid, string area, TaskTypeEnum tasktype, MPoint goods, int takeNum, int giveNum, DevType fromd, DevType tod, DevFlag flag)
        {
            int id = ADS.ID;
            ADS.PlusID();
            TaskRGV t = new TaskRGV()
            {
                id = id,
                jobid = jobid,
                taskid = taskid,
                area = area,
                tasktype = tasktype,
                takeNum = takeNum,
                giveNum = giveNum,
                takesite = goods.takesite,
                givesite = goods.givesite,
                taskstatus = TaskStatus.init,
                fromdev = fromd,
                todev = tod,
                flag = flag,
                device = new DevInfoRGV()
            };
            task.Add(t);
            t.InsertDB();
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        private void AddRGV(string jobid, string taskid, string area, TaskTypeEnum tasktype, int take, int give, int takeNum, int giveNum, DevType fromd, DevType tod, DevFlag flag)
        {
            AddRGV(jobid, taskid, area, tasktype, new MPoint() { takesite = take, givesite = give }, takeNum, giveNum, fromd, tod, flag);
        }


        /// <summary>
        /// 检测任务状态
        /// </summary>
        public void DoTask()
        {
            if (task == null || task.Count == 0) return;

            foreach (TaskRGV t in task)
            {
                //if (!t.activie) continue;
                if (t.taskstatus == TaskStatus.finish) continue;
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    DevInfoRGV device = FindFreeDevice(t.area, t.flag);
                    if (device != null)
                    {
                        t.device = device;
                        t.device.IsLockUnlock(true, t.jobid);
                        t.UpdateDev();

                        // 更新接送点 
                        t.takesite = t.takesite + t.device.gap;
                        t.givesite = t.givesite + t.device.gap;
                        t.UpdateSite();
                    }
                }
                else
                {
                    // 故障&异常
                    if (t.device._.CommandStatus == CommandEnum.命令错误 || t.device._.DeviceStatus == DeviceEnum.设备故障)
                    {
                        continue;
                    }

                    switch (t.taskstatus)
                    {
                        case TaskStatus.init:
                            t.UpdateStatus(TaskStatus.totakesite);
                            break;

                        case TaskStatus.totakesite:
                            // 无来源设备 直接前往送货点
                            if (t.fromdev == DevType.空设备)
                            {
                                t.UpdateStatus(TaskStatus.togivesite);
                                break;
                            }
                            // |接货点 - 当前坐标| <= 允许范围
                            if (Math.Abs(t.takesite - t.device._.CurrentSite) <= t.device.limit)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ontakesite);
                                }
                            }
                            else
                            {
                                // 防撞
                                if (IsHit(t.device, t.takesite, t.fromdev))
                                {
                                    t.device.StopTask();
                                }
                                else
                                {
                                    t.device.ToSite(t.takesite);
                                }
                            }
                            break;

                        case TaskStatus.ontakesite:
                            // 来源为行车 不作为
                            if (t.fromdev == DevType.行车)
                            {
                                t.UpdateStatus(TaskStatus.taking);
                                break;
                            }
                            // 判断是否启动辊台接货
                            if (t.takeready)
                            {
                                if (t.device._.GoodsStatus == GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.device.StartTakeRoll(t.tasktype, t.takeNum);
                                }

                                if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
                                {
                                    t.UpdateStatus(TaskStatus.taking);
                                }
                            }
                            else
                            {
                                // ? JOB 更新请求
                                if (t.fromdev == DevType.运输车)
                                {
                                    t.takeready = IsTakeButtRGV(t.jobid, DevType.运输车);
                                }
                                else
                                {
                                    t.takeready = ADS.JobPartRGV_Take(t.jobid);
                                }
                            }
                            break;

                        case TaskStatus.taking:
                            // 来源为行车 判断是否提前安全离开
                            if (t.fromdev == DevType.行车)
                            {
                                if (t.isLeaveAble)
                                {
                                    t.UpdateStatus(TaskStatus.taked);
                                }
                                else
                                {
                                    // ? JOB 更新请求
                                    t.isLeaveAble = ADS.JobPartRGV_TakeLeave(t.area, t.taskid);
                                }
                            }
                            else
                            {
                                if ((t.takeNum == 1 && t.tasktype == TaskTypeEnum.入库 && t.device._.GoodsStatus == GoodsEnum.辊台1有货) ||
                                    (t.takeNum == 1 && t.tasktype == TaskTypeEnum.出库 && t.device._.GoodsStatus == GoodsEnum.辊台2有货) ||
                                    (t.takeNum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
                                {
                                    if (t.device._.ActionStatus == ActionEnum.停止)
                                    {
                                        t.UpdateStatus(TaskStatus.taked);
                                    }
                                }
                                else
                                {
                                    t.device.StartTakeRoll(t.tasktype, t.takeNum);
                                }
                            }
                            break;

                        case TaskStatus.taked:
                            t.UpdateStatus(TaskStatus.togivesite);
                            break;

                        case TaskStatus.togivesite:
                            // 无目的设备 直接完成送货
                            if (t.todev == DevType.空设备)
                            {
                                t.UpdateStatus(TaskStatus.gived);
                                break;
                            }
                            // |送货点 - 当前坐标| <= 允许范围
                            if (Math.Abs(t.givesite - t.device._.CurrentSite) <= t.device.limit)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ongivesite);
                                }
                            }
                            else
                            {
                                // 防撞
                                if (IsHit(t.device, t.givesite, t.todev))
                                {
                                    t.device.StopTask();
                                }
                                else
                                {
                                    t.device.ToSite(t.givesite);
                                }
                            }
                            break;

                        case TaskStatus.ongivesite:
                            // 目的为行车 不作为
                            if (t.todev == DevType.行车)
                            {
                                t.UpdateStatus(TaskStatus.giving);
                                break;
                            }
                            // 判断是否启动辊台送货
                            if (t.giveready)
                            {
                                if (t.device._.GoodsStatus != GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.device.StartGiveRoll(t.tasktype);
                                }

                                if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
                                {
                                    t.UpdateStatus(TaskStatus.giving);
                                }
                            }
                            else
                            {
                                // ? JOB 更新请求
                                if (t.todev == DevType.运输车)
                                {
                                    t.giveready = IsGiveButtRGV(t.jobid, DevType.运输车);
                                }
                                else
                                {
                                    t.giveready = ADS.JobPartRGV_Give(t.jobid);
                                }
                            }
                            break;

                        case TaskStatus.giving:
                            // 目的为行车 判断是否提前安全离开
                            if (t.todev == DevType.行车)
                            {
                                if (t.isLeaveAble)
                                {
                                    t.UpdateStatus(TaskStatus.gived);
                                }
                                else
                                {
                                    // ? JOB 更新请求
                                    t.isLeaveAble = ADS.JobPartRGV_GiveLeave(t.area, t.taskid);
                                }
                            }
                            else
                            {
                                if ((t.takeNum == t.giveNum && t.device._.GoodsStatus == GoodsEnum.辊台无货) ||
                                   (t.takeNum != t.giveNum && (t.device._.GoodsStatus == GoodsEnum.辊台1有货 ||
                                                               t.device._.GoodsStatus == GoodsEnum.辊台2有货)))
                                {
                                    if (t.device._.ActionStatus == ActionEnum.停止)
                                    {
                                        t.UpdateStatus(TaskStatus.gived);
                                    }
                                }
                                else
                                {
                                    t.device.StartGiveRoll(t.tasktype);
                                }
                            }
                            break;

                        case TaskStatus.gived:
                            // 解锁设备、完成任务
                            t.device.IsLockUnlock(false);
                            t.device = null;
                            t.UpdateStatus(TaskStatus.finish);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion


        #region [ 流程判断 ]

        /// <summary>
        /// 是否会撞车
        /// </summary>
        private bool IsHit(DevInfoRGV dev, int site, DevType type)
        {
            // 同侧设备同侧目的
            if (dev.flag == DevFlag.靠近入库口 && dev._.CurrentSite >= site)
            {
                return false;
            }
            if (dev.flag == DevFlag.远离入库口 && dev._.CurrentSite <= site)
            {
                return false;
            }

            // 安全距离外
            int safe = ADS.mDis.GetRgvSafeDis(dev.area);
            DevInfoRGV dev2 = devices.Find(c => c.area.Equals(dev.area) && c.flag != dev.flag);
            if (dev2 == null) return false;
            if (Math.Abs(dev._.CurrentSite - dev2._.CurrentSite) >= safe)
            {
                return false;
            }

            // 优先判断
            int loc = dev.flag == DevFlag.靠近入库口 ? (site + safe) : (site - safe);
            if (!dev.isLock)
            {
                dev.ToSite(loc);
                return false;
            }

            TaskRGV t = task.Find(c => c.device == dev2);
            int result;
            int disboth;
            int dis1 = Math.Abs(dev._.CurrentSite - site);
            int dis2;
            switch (t.taskstatus)
            {
                case TaskStatus.init:
                case TaskStatus.totakesite:
                    result = GetPriorityByType(type, t.fromdev);
                    if (result == 1)
                    {
                        dev.ToSite(loc);
                        return false;
                    }
                    else if (result == 2)
                    {
                        break;
                    }
                    else
                    {
                        disboth = Math.Abs(site - t.takesite);
                        dis2 = Math.Abs(dev2._.CurrentSite - t.takesite);
                        if (disboth >= safe)
                        {
                            return false;
                        }

                        if (dis1 <= dis2)
                        {
                            dev.ToSite(loc);
                            return false;
                        }
                    }
                    break;
                case TaskStatus.ontakesite:
                case TaskStatus.taking:
                case TaskStatus.ongivesite:
                case TaskStatus.giving:
                    break;
                case TaskStatus.taked:
                case TaskStatus.togivesite:
                    result = GetPriorityByType(type, t.todev);
                    if (result == 1)
                    {
                        dev.ToSite(loc);
                        return false;
                    }
                    else if (result == 2)
                    {
                        break;
                    }
                    else
                    {
                        disboth = Math.Abs(site - t.givesite);
                        dis2 = Math.Abs(dev2._.CurrentSite - t.givesite);
                        if (disboth >= safe)
                        {
                            return false;
                        }

                        if (dis1 <= dis2)
                        {
                            dev.ToSite(loc);
                            return false;
                        }
                    }
                    break;
                default:
                    return false;
            }
            // 会撞，停止任务
            dev.StopTask();
            return true;
        }

        /// <summary>
        /// 获取优先
        /// 0:相同，1:优先1，2:优先2
        /// </summary>
        private int GetPriorityByType(DevType type1, DevType type2)
        {
            if (type1 == type2)
            {
                return 0;
            }
            else
            {
                if (type1 == DevType.行车)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        /// <summary>
        /// 寻找有效设备
        /// </summary>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        private DevInfoRGV FindFreeDevice(string area, DevFlag flag)
        {
            return devices.Find(c => !c.isLock && !c.isUseful && c.area.Equals(area) && c.flag == flag);
        }


        /// <summary>
        /// 是否满足从运输车接货的条件(运输车送)
        /// </summary>
        public bool IsTakeButtRGV(string JTid, DevType dev)
        {
            bool restul = false;
            switch (dev)
            {
                case DevType.行车:
                    if (task.Exists(c => c.taskid == JTid && c.todev == dev && c.taskstatus == TaskStatus.giving))
                    {
                        restul = true;
                    }
                    break;
                case DevType.摆渡车:
                case DevType.运输车:
                    if (task.Exists(c => c.jobid == JTid && c.todev == dev && c.taskstatus == TaskStatus.ongivesite))
                    {
                        restul = true;
                    }
                    break;
                default:
                    break;
            }
            return restul;
        }

        /// <summary>
        /// 是否满足向运输车送货的条件(运输车接)
        /// </summary>
        public bool IsGiveButtRGV(string JTid, DevType dev)
        {
            bool restul = false;
            switch (dev)
            {
                case DevType.行车:
                    if (task.Exists(c => c.taskid == JTid && c.fromdev == dev && c.taskstatus == TaskStatus.taking))
                    {
                        restul = true;
                    }
                    break;
                case DevType.摆渡车:
                case DevType.运输车:
                    if (task.Exists(c => c.jobid == JTid && c.fromdev == dev && c.taskstatus == TaskStatus.taking))
                    {
                        restul = true;
                    }
                    break;
                default:
                    break;
            }
            return restul;
        }

        #endregion

    }

    #region [ 其他 ]

    /// <summary>
    /// 接送坐标
    /// </summary>
    public class MPoint
    {
        public int takesite;
        public int givesite;
    }

    /// <summary>
    /// WMS任务号
    /// </summary>
    public class MTask
    {
        public string task1;
        public string task2;
    }

    #endregion

}
