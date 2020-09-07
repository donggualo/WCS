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

        /// <summary>
        /// 近车 绝对距离设定
        /// </summary>
        internal const int dis1 = 6000;

        /// <summary>
        /// 远车 绝对距离设定
        /// </summary>
        internal const int dis2 = 282000;

        /// <summary>
        /// 作业最小距离设定
        /// </summary>
        internal const int disMin = 7500;


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
                    taskType = (TaskTypeEnum)d.TASK_TYPE,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID1 = d.LOCK_ID1,
                    lockID2 = d.LOCK_ID2,
                    flag = (DevFlag)d.FLAG,
                    gap = d.GAP_X,
                    limit = d.LIMIT_X,
                    _ = new DeviceRGV()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoRGV.GetDataOrder());

                if (d.IS_USEFUL == 0)
                {
                    ADS.mSocket.UpdateUserful(d.DEVICE, false);
                }

            }

            foreach (AreaDistance a in ADS.mDis.distances)
            {
                ChangeFlag(a.area);
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
        //public void DoTask()
        //{
        //    if (task == null || task.Count == 0) return;

        //    foreach (TaskRGV t in task)
        //    {
        //        //if (!t.activie) continue;
        //        if (t.taskstatus == TaskStatus.finish) continue;
        //        if (string.IsNullOrEmpty(t.device.devName))
        //        {
        //            DevInfoRGV device = FindFreeDevice(t.area, t.flag);
        //            if (device != null)
        //            {
        //                t.device = device;
        //                t.device.IsLockUnlock(true, t.jobid);
        //                t.UpdateDev();

        //                // 更新接送点 
        //                t.takesite = t.takesite + t.device.gap;
        //                t.givesite = t.givesite + t.device.gap;
        //                t.UpdateSite();
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
        //                        break;
        //                    }
        //                    // |接货点 - 当前坐标| <= 允许范围
        //                    if (Math.Abs(t.takesite - t.device._.CurrentSite) <= t.device.limit)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ontakesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 防撞
        //                        if (IsHit(t.device, t.takesite, t.fromdev))
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
        //                    // 来源为行车 不作为
        //                    if (t.fromdev == DevType.行车)
        //                    {
        //                        t.UpdateStatus(TaskStatus.taking);
        //                        break;
        //                    }
        //                    // 判断是否启动辊台接货
        //                    if (t.takeready)
        //                    {
        //                        if (t.device._.GoodsStatus == GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.device.StartTakeRoll(t.tasktype, t.takeNum);
        //                        }

        //                        if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taking);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // ? JOB 更新请求
        //                        if (t.fromdev == DevType.运输车)
        //                        {
        //                            t.takeready = IsTakeButtRGV(t.jobid, DevType.运输车);
        //                        }
        //                        else
        //                        {
        //                            t.takeready = ADS.JobPartRGV_Take(t.jobid);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.taking:
        //                    // 来源为行车 判断是否提前安全离开
        //                    if (t.fromdev == DevType.行车)
        //                    {
        //                        if (t.isLeaveAble)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taked);
        //                        }
        //                        else
        //                        {
        //                            // ? JOB 更新请求
        //                            t.isLeaveAble = ADS.JobPartRGV_TakeLeave(t.area, t.taskid);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if ((t.takeNum == 1 && t.tasktype == TaskTypeEnum.入库 && t.device._.GoodsStatus == GoodsEnum.辊台1有货) ||
        //                            (t.takeNum == 1 && t.tasktype == TaskTypeEnum.出库 && t.device._.GoodsStatus == GoodsEnum.辊台2有货) ||
        //                            (t.takeNum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
        //                        {
        //                            if (t.device._.ActionStatus == ActionEnum.停止)
        //                            {
        //                                t.UpdateStatus(TaskStatus.taked);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            t.device.StartTakeRoll(t.tasktype, t.takeNum);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.taked:
        //                    t.UpdateStatus(TaskStatus.togivesite);
        //                    break;

        //                case TaskStatus.togivesite:
        //                    // 无目的设备 直接完成送货
        //                    if (t.todev == DevType.空设备)
        //                    {
        //                        t.UpdateStatus(TaskStatus.gived);
        //                        break;
        //                    }
        //                    // |送货点 - 当前坐标| <= 允许范围
        //                    if (Math.Abs(t.givesite - t.device._.CurrentSite) <= t.device.limit)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ongivesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 防撞
        //                        if (IsHit(t.device, t.givesite, t.todev))
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
        //                    // 目的为行车 不作为
        //                    if (t.todev == DevType.行车)
        //                    {
        //                        t.UpdateStatus(TaskStatus.giving);
        //                        break;
        //                    }
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
        //                        if (t.todev == DevType.运输车)
        //                        {
        //                            t.giveready = IsGiveButtRGV(t.jobid, DevType.运输车);
        //                        }
        //                        else
        //                        {
        //                            t.giveready = ADS.JobPartRGV_Give(t.jobid);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.giving:
        //                    // 目的为行车 判断是否提前安全离开
        //                    if (t.todev == DevType.行车)
        //                    {
        //                        if (t.isLeaveAble)
        //                        {
        //                            t.UpdateStatus(TaskStatus.gived);
        //                        }
        //                        else
        //                        {
        //                            // ? JOB 更新请求
        //                            t.isLeaveAble = ADS.JobPartRGV_GiveLeave(t.area, t.taskid);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if ((t.takeNum == t.giveNum && t.device._.GoodsStatus == GoodsEnum.辊台无货) ||
        //                           (t.takeNum != t.giveNum && (t.device._.GoodsStatus == GoodsEnum.辊台1有货 ||
        //                                                       t.device._.GoodsStatus == GoodsEnum.辊台2有货)))
        //                        {
        //                            if (t.device._.ActionStatus == ActionEnum.停止)
        //                            {
        //                                t.UpdateStatus(TaskStatus.gived);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            t.device.StartGiveRoll(t.tasktype);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.gived:
        //                    // 解锁设备、完成任务
        //                    t.device.IsLockUnlock(false);
        //                    t.device = new DevInfoRGV();
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

            foreach (DevInfoRGV d in devices)
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

                    switch (d.flag)
                    {
                        case DevFlag.靠近入库口:
                            switch (d.taskType)
                            {
                                case TaskTypeEnum.无:
                                    if (d._.ActionStatus == ActionEnum.运行中) continue;
                                    // 出库摆渡车&出库固定辊台 全解锁
                                    if (PublicParam.IsDoJobOut && ADS.mArf.IsFreeOutARF(d.area))
                                    {
                                        d.StopTask();
                                        // 远车解锁
                                        if (!IsUnlock(d.area, DevFlag.远离入库口)) continue;
                                        int rgvS = GetCurrentSite(d.area, DevFlag.远离入库口);
                                        if (rgvS == 0) continue;
                                        int num = ADS.GetOutTaskWMS(d.area, out string[] tasks);
                                        WCS_CONFIG_LOC loc1 = new WCS_CONFIG_LOC();
                                        WCS_CONFIG_LOC loc2 = new WCS_CONFIG_LOC();
                                        WCS_CONFIG_LOC loc3 = new WCS_CONFIG_LOC();
                                        WCS_CONFIG_LOC loc4 = new WCS_CONFIG_LOC();
                                        int site1 = 0;
                                        int site2 = 0;
                                        int site3 = 0;
                                        int site4 = 0;
                                        TaskTypeEnum tt = TaskTypeEnum.出库;
                                        // 任务：先2后1
                                        switch (num)
                                        {
                                            case 1:
                                                if (rgvS == -1) // 远车不用了
                                                {
                                                    // 直接锁任务
                                                    d.IsLockUnlockNew(tt, false, "", tasks[0]);
                                                }
                                                else
                                                {
                                                    loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                    site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_2) ? "0" : loc1.RGV_LOC_2);
                                                    if (site1 != 0)
                                                    {
                                                        if (site1 <= dis1)
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(tt, false, "", tasks[0]);
                                                            break;
                                                        }

                                                        if (site1 >= dis2 || Math.Abs(site1 - d._.CurrentSite) > Math.Abs(site1 - rgvS))
                                                        {
                                                            // 对接距离
                                                            int s = ADS.mDis.GetRgvButtDis(d.area);
                                                            if (s == 0) throw new Exception("无对应运输车对接距离！");
                                                            // 锁远车
                                                            if (LockRGV(d.area, DevFlag.远离入库口, tt, "", tasks[0], 0, 0))
                                                            {
                                                                // 锁
                                                                d.IsLockUnlockNew(tt, false);
                                                                d.TakeSite = site1 - s + d.gap;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(tt, false, "", tasks[0]);
                                                        }
                                                    }
                                                }
                                                break;
                                            case 2:
                                                onlyARF:
                                                if (rgvS == -1) // 远车不用了
                                                {
                                                    // 直接锁任务
                                                    d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                }
                                                else
                                                {
                                                    loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                    loc2 = CommonSQL.GetWcsLocByTask(tasks[1]);
                                                    site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_2) ? "0" : loc1.RGV_LOC_2);
                                                    site2 = int.Parse(string.IsNullOrEmpty(loc2.RGV_LOC_1) ? "0" : loc2.RGV_LOC_1);
                                                    if (site1 != 0 && site2 != 0)
                                                    {
                                                        if (site1 <= dis1 || site2 <= dis1)
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                            break;
                                                        }

                                                        if (site2 >= dis2 || Math.Abs(site1 - d._.CurrentSite) > Math.Abs(site1 - rgvS))
                                                        {
                                                            // 对接距离
                                                            int s = ADS.mDis.GetRgvButtDis(d.area);
                                                            if (s == 0) throw new Exception("无对应运输车对接距离！");
                                                            // 锁远车
                                                            if (LockRGV(d.area, DevFlag.远离入库口, tt, tasks[1], tasks[0], 0, 0))
                                                            {
                                                                // 锁
                                                                d.IsLockUnlockNew(tt, false);
                                                                d.TakeSite = site2 - s + d.gap;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("无对应运输车任务（" + tasks.ToString() + "）坐标！");
                                                    }
                                                }
                                                break;
                                            case 3:
                                                if (!ADS.mFrt.IsFreeOutFRT(d.area))
                                                {
                                                    // 辊台有货则送到摆渡车
                                                    goto onlyARF;
                                                }

                                                if (rgvS == -1) // 远车不用了
                                                {
                                                    // 直接锁任务
                                                    d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                }
                                                else
                                                {
                                                    loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                    loc2 = CommonSQL.GetWcsLocByTask(tasks[1]);
                                                    loc3 = CommonSQL.GetWcsLocByTask(tasks[2]);
                                                    site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_2) ? "0" : loc1.RGV_LOC_2);
                                                    site2 = int.Parse(string.IsNullOrEmpty(loc2.RGV_LOC_1) ? "0" : loc2.RGV_LOC_1);
                                                    site3 = int.Parse(string.IsNullOrEmpty(loc3.RGV_LOC_2) ? "0" : loc3.RGV_LOC_2);
                                                    if (site1 != 0 && site2 != 0 && site3 != 0)
                                                    {
                                                        if (site3 >= dis2 || site1 <= dis1 || Math.Abs(site2 - site3) <= disMin) // 行车间最小安全距离
                                                        {
                                                            // 单车
                                                            if (site1 <= dis1 || site2 <= dis1)
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                                break;
                                                            }

                                                            if (site2 >= dis2 || Math.Abs(site1 - d._.CurrentSite) > Math.Abs(site1 - rgvS))
                                                            {
                                                                // 对接距离
                                                                int s = ADS.mDis.GetRgvButtDis(d.area);
                                                                if (s == 0) throw new Exception("无对应运输车对接距离！");
                                                                // 锁远车
                                                                if (LockRGV(d.area, DevFlag.远离入库口, tt, tasks[1], tasks[0], 0, 0))
                                                                {
                                                                    // 锁
                                                                    d.IsLockUnlockNew(tt, false);
                                                                    d.TakeSite = site2 - s + d.gap;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                            }
                                                        }
                                                        else // 双车
                                                        {
                                                            // 锁远车
                                                            if (LockRGV(d.area, DevFlag.远离入库口, tt, tasks[1], tasks[0], 0, 0))
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, "", tasks[2]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("无对应运输车任务（" + tasks.ToString() + "）坐标！");
                                                    }
                                                }
                                                break;
                                            case 4:
                                                if (!ADS.mFrt.IsFreeOutFRT(d.area))
                                                {
                                                    // 辊台有货则送到摆渡车
                                                    goto onlyARF;
                                                }

                                                if (rgvS == -1) // 远车不用了
                                                {
                                                    // 直接锁任务
                                                    d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                }
                                                else
                                                {
                                                    loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                    loc2 = CommonSQL.GetWcsLocByTask(tasks[1]);
                                                    loc3 = CommonSQL.GetWcsLocByTask(tasks[2]);
                                                    loc4 = CommonSQL.GetWcsLocByTask(tasks[3]);
                                                    site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_2) ? "0" : loc1.RGV_LOC_2);
                                                    site2 = int.Parse(string.IsNullOrEmpty(loc2.RGV_LOC_1) ? "0" : loc2.RGV_LOC_1);
                                                    site3 = int.Parse(string.IsNullOrEmpty(loc3.RGV_LOC_2) ? "0" : loc3.RGV_LOC_2);
                                                    site4 = int.Parse(string.IsNullOrEmpty(loc4.RGV_LOC_1) ? "0" : loc4.RGV_LOC_1);
                                                    if (site1 != 0 && site2 != 0 && site3 != 0 && site4 != 0)
                                                    {
                                                        if (site4 >= dis2 || site1 <= dis1 || Math.Abs(site2 - site3) <= disMin)  // 行车间最小安全距离
                                                        {
                                                            // 单车
                                                            if (site1 <= dis1 || site2 <= dis1)
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                                break;
                                                            }

                                                            if (site2 >= dis2 || Math.Abs(site1 - d._.CurrentSite) > Math.Abs(site1 - rgvS))
                                                            {
                                                                // 对接距离
                                                                int s = ADS.mDis.GetRgvButtDis(d.area);
                                                                if (s == 0) throw new Exception("无对应运输车对接距离！");
                                                                // 锁远车
                                                                if (LockRGV(d.area, DevFlag.远离入库口, tt, tasks[1], tasks[0], 0, 0))
                                                                {
                                                                    // 锁
                                                                    d.IsLockUnlockNew(tt, false);
                                                                    d.TakeSite = site2 - s + d.gap;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, tasks[1], tasks[0]);
                                                            }
                                                        }
                                                        else // 双车
                                                        {
                                                            // 锁远车
                                                            if (LockRGV(d.area, DevFlag.远离入库口, tt, tasks[1], tasks[0], 0, 0))
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(tt, false, tasks[3], tasks[2]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("无对应运输车任务（" + tasks.ToString() + "）坐标！");
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //int site = ADS.mDis.GetRgvButtP(d.area);
                                        //if (site == 0) throw new Exception("无对应运输车-摆渡车对接点！");
                                        //if (d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台无货)
                                        //{
                                        //    // 定位对接点
                                        //    d.ToSite(site);
                                        //}

                                        // 远车解锁
                                        //if (!IsUnlock(d.area, DevFlag.远离入库口)) continue;

                                        // 入库摆渡车 锁定
                                        if (ADS.mArf.IsLockARF(d.area, TaskTypeEnum.入库, out int goods))
                                        {
                                            if (goods > 0)
                                            {
                                                // 锁定
                                                d.IsLockUnlockNew(TaskTypeEnum.入库, false);
                                                goto InTask;
                                            }
                                        }
                                    }
                                    break;
                                case TaskTypeEnum.入库:
                                    InTask:
                                    if (d.isLock)
                                    {
                                        if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                        {
                                            if (d._.ActionStatus == ActionEnum.停止)
                                            {
                                                d.StopTask();
                                                if (!IsUnlock(d.area, DevFlag.远离入库口)) continue;
                                                string[] tasks = null;
                                                int rgvS = GetCurrentSite(d.area, DevFlag.远离入库口);
                                                WCS_CONFIG_LOC loc1 = new WCS_CONFIG_LOC();
                                                int site1 = 0;
                                                WCS_CONFIG_LOC loc2 = new WCS_CONFIG_LOC();
                                                int site2 = 0;
                                                switch (d._.GoodsStatus)
                                                {
                                                    case GoodsEnum.辊台无货:
                                                        d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                        d.GiveSite = 0;
                                                        break;
                                                    case GoodsEnum.辊台2有货:
                                                    case GoodsEnum.辊台中间有货:
                                                        // 接到1去
                                                        d.StartTakeRoll(d.taskType, 1);
                                                        break;
                                                    case GoodsEnum.辊台1有货:
                                                        if (d.GiveSite != 0) break;
                                                        if (ADS.GetInTaskWMS(d.area, 1, out tasks))
                                                        {
                                                            if (rgvS == 0) break;
                                                            if (rgvS == -1) // 远车不用了
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(d.taskType, true, tasks[0]);
                                                                goto InTask;
                                                            }
                                                            else
                                                            {
                                                                loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                                site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_1) ? "0" : loc1.RGV_LOC_1);
                                                                if (site1 != 0)
                                                                {
                                                                    if (site1 <= dis1)
                                                                    {
                                                                        // 直接锁任务
                                                                        d.IsLockUnlockNew(d.taskType, true, tasks[0]);
                                                                        goto InTask;
                                                                    }

                                                                    if (site1 >= dis2 || Math.Abs(site1 - d._.CurrentSite) > Math.Abs(site1 - rgvS))
                                                                    {
                                                                        // 中间点
                                                                        int butt = Math.Abs(d._.CurrentSite - d.gap - rgvS) / 2;
                                                                        if (site1 < dis2 && site1 <= (butt + dis1))
                                                                        {
                                                                            // 直接锁任务
                                                                            d.IsLockUnlockNew(d.taskType, true, tasks[0]);
                                                                            goto InTask;
                                                                        }
                                                                        // 对接距离
                                                                        int s = ADS.mDis.GetRgvButtDis(d.area);
                                                                        if (s == 0) throw new Exception("无对应运输车对接距离！");
                                                                        // 对接点
                                                                        if (Math.Abs(d._.CurrentSite - d.gap - rgvS - s) > d.limit)
                                                                        {
                                                                            butt = butt + s;
                                                                        }
                                                                        else
                                                                        {
                                                                            butt = rgvS;
                                                                        }
                                                                        // 锁远车
                                                                        if (IsUnlock(d.area, DevFlag.远离入库口))
                                                                        {
                                                                            if (LockRGV(d.area, DevFlag.远离入库口, d.taskType, tasks[0], "", butt, 0))
                                                                            {
                                                                                d.GiveSite = butt - s + d.gap;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // 直接锁任务
                                                                        d.IsLockUnlockNew(d.taskType, true, tasks[0]);
                                                                        goto InTask;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台满货:
                                                        if (d.GiveSite != 0) break;
                                                        if (ADS.GetInTaskWMS(d.area, 2, out tasks))
                                                        {
                                                            if (rgvS == 0) break;
                                                            if (rgvS == -1) // 远车不用了
                                                            {
                                                                // 直接锁任务
                                                                d.IsLockUnlockNew(d.taskType, true, tasks[0], tasks[1]);
                                                                goto InTask;
                                                            }
                                                            else
                                                            {
                                                                loc1 = CommonSQL.GetWcsLocByTask(tasks[0]);
                                                                loc2 = CommonSQL.GetWcsLocByTask(tasks[1]);
                                                                site1 = int.Parse(string.IsNullOrEmpty(loc1.RGV_LOC_1) ? "0" : loc1.RGV_LOC_1);
                                                                site2 = int.Parse(string.IsNullOrEmpty(loc2.RGV_LOC_2) ? "0" : loc2.RGV_LOC_2);
                                                                if (site1 != 0 && site2 != 0)
                                                                {
                                                                    // 优先位置
                                                                    int f = site1 > site2 ? site2 : site1;
                                                                    // 后置位
                                                                    int l = f == site1 ? site2 : site1;

                                                                    // 对接距离
                                                                    int s = ADS.mDis.GetRgvButtDis(d.area);
                                                                    if (s == 0) throw new Exception("无对应运输车对接距离！");

                                                                    if (l <= dis1 || Math.Abs(site1 - site2) <= disMin)  // 行车间最小安全距离
                                                                    {
                                                                        // 单车解决
                                                                        if (l <= dis1)
                                                                        {
                                                                            // 直接锁任务
                                                                            d.IsLockUnlockNew(d.taskType, true, tasks[0], tasks[1]);
                                                                            goto InTask;
                                                                        }
                                                                        if (f >= dis2 || Math.Abs(f - d._.CurrentSite) > Math.Abs(f - rgvS))
                                                                        {
                                                                            // 中间点
                                                                            int butt = Math.Abs(d._.CurrentSite - d.gap - rgvS) / 2;
                                                                            if (f < dis2 && f < (butt + dis1))
                                                                            {
                                                                                // 直接锁任务
                                                                                d.IsLockUnlockNew(d.taskType, true, tasks[0], tasks[1]);
                                                                                goto InTask;
                                                                            }
                                                                            // 对接点
                                                                            if (Math.Abs(d._.CurrentSite - d.gap - rgvS - s) > d.limit)
                                                                            {
                                                                                butt = butt + s;
                                                                            }
                                                                            else
                                                                            {
                                                                                butt = rgvS;
                                                                            }
                                                                            // 锁远车
                                                                            if (IsUnlock(d.area, DevFlag.远离入库口))
                                                                            {
                                                                                if (LockRGV(d.area, DevFlag.远离入库口, d.taskType, tasks[0], tasks[1], butt, 0))
                                                                                {
                                                                                    d.GiveSite = butt - s + d.gap;
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // 直接锁任务
                                                                            d.IsLockUnlockNew(d.taskType, true, tasks[0], tasks[1]);
                                                                            goto InTask;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // 双车
                                                                        if (site1 > site2)
                                                                        {
                                                                            // 锁远车
                                                                            if (IsUnlock(d.area, DevFlag.远离入库口))
                                                                            {
                                                                                if (LockRGV(d.area, DevFlag.远离入库口, d.taskType, tasks[0], "", site2 + s, 0))
                                                                                {
                                                                                    // 直接锁任务
                                                                                    d.IsLockUnlockNew(d.taskType, true, "", tasks[1]);
                                                                                    goto InTask;
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // 锁远车
                                                                            if (IsUnlock(d.area, DevFlag.远离入库口))
                                                                            {
                                                                                if (LockRGV(d.area, DevFlag.远离入库口, d.taskType, tasks[1], "", site1 + s, 0))
                                                                                {
                                                                                    // 直接锁任务
                                                                                    d.IsLockUnlockNew(d.taskType, true, tasks[0], "");
                                                                                    goto InTask;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    throw new Exception("无对应运输车任务（" + tasks.ToString() + "）坐标！");
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (d._.CurrentTask == TaskEnum.辊台任务 && d._.GoodsStatus == GoodsEnum.辊台无货)
                                                {
                                                    d.StopRoller();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ADS.mAwc.IsOk(d.area, d.flag) && d._.ActionStatus == ActionEnum.停止)
                                            {
                                                switch (d._.GoodsStatus)
                                                {
                                                    case GoodsEnum.辊台无货:
                                                    case GoodsEnum.辊台中间有货:
                                                        // 解任务
                                                        d.StopTask();
                                                        d.IsLockUnlockNew(d.taskType, true);
                                                        break;
                                                    case GoodsEnum.辊台1有货:
                                                        if (string.IsNullOrEmpty(d.lockID1))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台2有货:
                                                        if (string.IsNullOrEmpty(d.lockID2))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台满货:
                                                        if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            string s1 = CommonSQL.GetWcsLocByName(d.lockID1, "RGV_LOC_1");
                                                            string s2 = CommonSQL.GetWcsLocByName(d.lockID2, "RGV_LOC_2");
                                                            int site1 = int.Parse(string.IsNullOrEmpty(s1) ? "0" : s1);
                                                            int site2 = int.Parse(string.IsNullOrEmpty(s2) ? "0" : s2);

                                                            // 安排行车
                                                            if (site1 == 0 && site2 != 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                            }
                                                            else if (site1 != 0 && site2 == 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                            }
                                                            else if (site1 != 0 && site2 != 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, site1 > site2 ? d.lockID2 : d.lockID1);
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("无对应任务坐标！");
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 空闲摆渡车
                                        if (ADS.mArf.IsFreeARF(d.area, d.taskType))
                                        {
                                            if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                                       (d._.GoodsStatus == GoodsEnum.辊台1有货 &&
                                                       d._.RollerStatus != RollerStatusEnum.辊台1启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                            {
                                                d.StopRoller();
                                                // 锁定
                                                d.IsLockUnlockNew(d.taskType, true);
                                                goto InTask;
                                            }
                                            else
                                            {
                                                if (d._.GoodsStatus == GoodsEnum.辊台无货)
                                                {
                                                    // 全解
                                                    d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                    d.GiveSite = 0;
                                                }
                                                else
                                                {
                                                    // 接货
                                                    d.StartTakeRoll(d.taskType, 1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int site = ADS.mDis.GetRgvButtP(d.area);
                                            //if (site == 0) throw new Exception("无对应运输车-摆渡车对接点！");
                                            //if (d._.ActionStatus == ActionEnum.停止)
                                            //{
                                            //    // 定位对接点
                                            //    d.ToSite(site);
                                            //}

                                            int p = ADS.mDis.GetArfButtP(d.area);
                                            if (p == 0) throw new Exception("无对应摆渡车-运输车对接点！");
                                            // 对应摆渡车
                                            if (ADS.mArf.IsLockARF(d.area, d.taskType, out int goods))
                                            {
                                                // 对接点送货
                                                if (ADS.mArf.MovingButtSite(d.area, d.taskType, p))
                                                {
                                                    if (d._.GoodsStatus != GoodsEnum.辊台满货 || goods != 0)
                                                    {
                                                        // |对接点 - 当前坐标| > 允许范围
                                                        if (Math.Abs(site - d._.CurrentSite) > d.limit)
                                                        {
                                                            if (d._.ActionStatus == ActionEnum.停止)
                                                            {
                                                                // 定位对接点
                                                                d.ToSite(site);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // 接货
                                                            d.StartTakeRoll(d.taskType, 2);
                                                            if (d._.ActionStatus == ActionEnum.运行中)
                                                            {
                                                                // 摆渡车送货
                                                                ADS.mArf.IsGiving(d.area, d.taskType);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                                       (d._.GoodsStatus == GoodsEnum.辊台1有货 &&
                                                       d._.RollerStatus != RollerStatusEnum.辊台1启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                                        {
                                                            d.StopRoller();
                                                            // 锁定
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                            goto InTask;
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
                                case TaskTypeEnum.出库:
                                    if (d.isLock)
                                    {
                                        if (d._.GoodsStatus == GoodsEnum.辊台无货)
                                        {
                                            // 解锁远车 && 锁定摆渡车
                                            if (ADS.mArf.LockARF(d.area, d.taskType))
                                            {
                                                if (IsUnlock(d.area, DevFlag.远离入库口))
                                                {
                                                    // 解锁
                                                    d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                }
                                                else
                                                {
                                                    // 解锁
                                                    d.IsLockUnlockNew(d.taskType, false);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int site = ADS.mDis.GetRgvButtP(d.area);
                                            if (site == 0) throw new Exception("无对应运输车-摆渡车对接点！");
                                            if (d._.ActionStatus == ActionEnum.停止)
                                            {
                                                // 定位对接点
                                                d.ToSite(site);
                                            }
                                            // 空闲摆渡车
                                            if (ADS.mArf.IsFreeARF(d.area, d.taskType))
                                            {
                                                int p = ADS.mDis.GetArfButtP(d.area);
                                                if (p == 0) throw new Exception("无对应摆渡车-运输车对接点！");
                                                // 摆渡车来接货
                                                if (ADS.mArf.MovingButtSite(d.area, d.taskType, p))
                                                {
                                                    // 启动摆渡车接货
                                                    if (ADS.mArf.IsTaking(d.area, d.taskType))
                                                    {
                                                        // |对接点 - 当前坐标| > 允许范围
                                                        if (Math.Abs(site - d._.CurrentSite) > d.limit)
                                                        {
                                                            if (d._.ActionStatus == ActionEnum.停止)
                                                            {
                                                                // 定位对接点
                                                                d.ToSite(site);
                                                            }
                                                        }
                                                        else
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
                                        if (d.TakeSite != 0)
                                        {
                                            if (d._.ActionStatus == ActionEnum.停止)
                                            {
                                                // |对接点 - 当前坐标| > 允许范围
                                                if (Math.Abs(d.TakeSite - d._.CurrentSite) > d.limit)
                                                {
                                                    // 定位对接点
                                                    d.ToSite(d.GiveSite);
                                                }
                                                else
                                                {
                                                    d.TakeSite = 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                            {
                                                if (d._.GoodsStatus != GoodsEnum.辊台无货 && d._.ActionStatus == ActionEnum.停止)
                                                {
                                                    if (d._.GoodsStatus == GoodsEnum.辊台满货)
                                                    {
                                                        // 锁定
                                                        d.StopTask();
                                                        d.IsLockUnlockNew(d.taskType, true);
                                                    }
                                                    else
                                                    {
                                                        if (GetGoodsStatus(d.area, DevFlag.远离入库口) != 1)
                                                        {
                                                            // 锁定
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (ADS.mAwc.IsOk(d.area, d.flag) && d._.ActionStatus == ActionEnum.停止)
                                                {
                                                    switch (d._.GoodsStatus)
                                                    {
                                                        case GoodsEnum.辊台1有货:
                                                        case GoodsEnum.辊台中间有货:
                                                            // 移到2去
                                                            d.StartTakeRoll(d.taskType, 1);
                                                            break;
                                                        case GoodsEnum.辊台无货:
                                                            if (string.IsNullOrEmpty(d.lockID2))
                                                            {
                                                                // 锁定
                                                                d.StopTask();
                                                                d.IsLockUnlockNew(d.taskType, true);
                                                            }
                                                            else
                                                            {
                                                                // 安排行车
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                            }
                                                            break;
                                                        case GoodsEnum.辊台2有货:
                                                            if (string.IsNullOrEmpty(d.lockID1))
                                                            {
                                                                // 锁定
                                                                d.StopTask();
                                                                d.IsLockUnlockNew(d.taskType, true);
                                                            }
                                                            else
                                                            {
                                                                // 安排行车
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                            }
                                                            break;
                                                        case GoodsEnum.辊台满货:
                                                            // 锁定
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (d._.CurrentTask == TaskEnum.辊台任务 && d._.GoodsStatus == GoodsEnum.辊台无货)
                                                    {
                                                        d.StopRoller();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DevFlag.远离入库口:
                            switch (d.taskType)
                            {
                                case TaskTypeEnum.无:
                                    break;
                                case TaskTypeEnum.入库:
                                    InTask:
                                    if (d.isLock)
                                    {
                                        if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                        {
                                            if (d._.ActionStatus == ActionEnum.停止)
                                            {
                                                d.StopTask();
                                                string[] tasks = null;
                                                switch (d._.GoodsStatus)
                                                {
                                                    case GoodsEnum.辊台无货:
                                                        // 解锁
                                                        d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                        break;
                                                    case GoodsEnum.辊台2有货:
                                                    case GoodsEnum.辊台中间有货:
                                                        // 接到1去
                                                        d.StartTakeRoll(d.taskType, 1);
                                                        break;
                                                    case GoodsEnum.辊台1有货:
                                                        // 近车解锁
                                                        if (!IsUnlock(d.area, DevFlag.靠近入库口)) break;
                                                        if (ADS.mAwc.IsOk(d.area, d.flag) && ADS.GetInTaskWMS(d.area, 1, out tasks))
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(d.taskType, true, tasks[0]);
                                                            goto InTask;
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台满货:
                                                        // 近车解锁
                                                        if (!IsUnlock(d.area, DevFlag.靠近入库口)) break;
                                                        if (ADS.mAwc.IsOk(d.area, d.flag) && ADS.GetInTaskWMS(d.area, 2, out tasks))
                                                        {
                                                            // 直接锁任务
                                                            d.IsLockUnlockNew(d.taskType, true, tasks[0], tasks[1]);
                                                            goto InTask;
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (d._.CurrentTask == TaskEnum.辊台任务 && d._.GoodsStatus == GoodsEnum.辊台无货)
                                                {
                                                    d.StopRoller();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ADS.mAwc.IsOk(d.area, d.flag) && d._.ActionStatus == ActionEnum.停止)
                                            {
                                                switch (d._.GoodsStatus)
                                                {
                                                    case GoodsEnum.辊台无货:
                                                    case GoodsEnum.辊台中间有货:
                                                        // 解任务
                                                        d.StopTask();
                                                        d.IsLockUnlockNew(d.taskType, true);
                                                        break;
                                                    case GoodsEnum.辊台1有货:
                                                        if (string.IsNullOrEmpty(d.lockID1))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台2有货:
                                                        if (string.IsNullOrEmpty(d.lockID2))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台满货:
                                                        if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                                        {
                                                            // 解任务
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            string s1 = CommonSQL.GetWcsLocByName(d.lockID1, "RGV_LOC_1");
                                                            string s2 = CommonSQL.GetWcsLocByName(d.lockID2, "RGV_LOC_2");
                                                            int site1 = int.Parse(string.IsNullOrEmpty(s1) ? "0" : s1);
                                                            int site2 = int.Parse(string.IsNullOrEmpty(s2) ? "0" : s2);

                                                            // 安排行车
                                                            if (site1 == 0 && site2 != 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                            }
                                                            else if (site1 != 0 && site2 == 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                            }
                                                            else if (site1 != 0 && site2 != 0)
                                                            {
                                                                ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, site1 > site2 ? d.lockID2 : d.lockID1);
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("无对应任务坐标！");
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 空闲近车
                                        if (IsFreeRGV(d.area, DevFlag.靠近入库口))
                                        {
                                            if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                                       (d._.GoodsStatus == GoodsEnum.辊台1有货 &&
                                                       d._.RollerStatus != RollerStatusEnum.辊台1启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                            {
                                                d.StopRoller();
                                                // 锁定
                                                d.IsLockUnlockNew(d.taskType, true, d.lockID1, d.lockID2);
                                                d.TakeSite = 0;
                                                goto InTask;
                                            }
                                            else
                                            {
                                                if (d._.GoodsStatus == GoodsEnum.辊台无货)
                                                {
                                                    // 解锁
                                                    if (d._.ActionStatus == ActionEnum.停止)
                                                    {
                                                        d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                    }
                                                }
                                                else
                                                {
                                                    // 接货
                                                    d.StartTakeRoll(d.taskType, 1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int goods = 0;
                                            if (!string.IsNullOrEmpty(d.lockID1))
                                            {
                                                goods++;
                                            }
                                            if (!string.IsNullOrEmpty(d.lockID2))
                                            {
                                                goods++;
                                            }
                                            if (goods == 0)
                                            {
                                                // 解锁
                                                d.StopTask();
                                                d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                continue;
                                            }

                                            int dis = ADS.mDis.GetRgvButtDis(d.area);
                                            if (dis == 0) throw new Exception("无对应运输车对接距离！");
                                            if (d._.ActionStatus == ActionEnum.停止 && d.TakeSite != 0)
                                            {
                                                // 定位对接点
                                                d.ToSite(d.TakeSite);
                                            }

                                            // 对应运输车
                                            if (IsLockRGV(d.area, DevFlag.靠近入库口, out int gds))
                                            {
                                                if (d.TakeSite == 0) d.TakeSite = d._.CurrentSite;

                                                // 对接点送货
                                                if (MovingButtSite(d.area, DevFlag.靠近入库口, d.TakeSite - d.gap - dis))
                                                {
                                                    if (d._.GoodsStatus != GoodsEnum.辊台满货 || goods != 0)
                                                    {
                                                        // |对接点 - 当前坐标| > 允许范围
                                                        if (Math.Abs(d.TakeSite - d._.CurrentSite) > d.limit)
                                                        {
                                                            if (d._.ActionStatus == ActionEnum.停止)
                                                            {
                                                                // 定位对接点
                                                                d.ToSite(d.TakeSite);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // 接货
                                                            d.StartTakeRoll(d.taskType, 2);
                                                            if (d._.ActionStatus == ActionEnum.运行中)
                                                            {
                                                                // 近车送货
                                                                IsGiving(d.area, d.taskType, DevFlag.靠近入库口);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if ((d._.GoodsStatus == GoodsEnum.辊台满货 && d._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                                          (d._.GoodsStatus == GoodsEnum.辊台1有货 &&
                                                          d._.RollerStatus != RollerStatusEnum.辊台1启动 && d._.RollerStatus != RollerStatusEnum.辊台全启动))
                                                        {
                                                            d.StopRoller();
                                                            // 锁定
                                                            d.IsLockUnlockNew(d.taskType, true, d.lockID1, d.lockID2);
                                                            d.TakeSite = 0;
                                                            goto InTask;
                                                        }
                                                        else
                                                        {
                                                            if (d._.GoodsStatus != GoodsEnum.辊台无货)
                                                            {
                                                                // 接货
                                                                d.StartTakeRoll(d.taskType, 1);
                                                                if (d._.RollerStatus != RollerStatusEnum.辊台停止 && goods == 1)
                                                                {
                                                                    // 近车送货
                                                                    IsGiving(d.area, d.taskType, DevFlag.靠近入库口);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case TaskTypeEnum.出库:
                                    if (d.isLock)
                                    {
                                        if (d._.GoodsStatus == GoodsEnum.辊台无货)
                                        {
                                            // 锁定近车
                                            if (LockRGV(d.area, DevFlag.靠近入库口))
                                            {
                                                d.StopRoller();
                                                // 解锁
                                                d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                            }
                                        }
                                        else
                                        {
                                            // 空闲近车
                                            if (IsFreeRGV(d.area, DevFlag.靠近入库口))
                                            {
                                                int dis = ADS.mDis.GetRgvButtDis(d.area);
                                                if (dis == 0) throw new Exception("无对应运输车对接距离！");
                                                if (d.GiveSite != 0)
                                                {
                                                    if (d._.ActionStatus == ActionEnum.停止)
                                                    {
                                                        // 定位对接点
                                                        d.ToSite(d.GiveSite);
                                                    }
                                                }
                                                else
                                                {
                                                    int rgvS = GetCurrentSite(d.area, DevFlag.靠近入库口) + d.gap;
                                                    if (rgvS < 0) continue;
                                                    if (Math.Abs(d._.CurrentSite - rgvS - dis) > d.limit)
                                                    {
                                                        d.GiveSite = Math.Abs(d._.CurrentSite - rgvS) / 2 + dis;
                                                    }
                                                    else
                                                    {
                                                        d.GiveSite = d._.CurrentSite;
                                                    }
                                                    break;
                                                }
                                                // 来接货
                                                if (MovingButtSite(d.area, DevFlag.靠近入库口, d.GiveSite - d.gap - dis))
                                                {
                                                    if (IsTaking(d.area, d.taskType, DevFlag.靠近入库口))
                                                    {
                                                        // |对接点 - 当前坐标| > 允许范围
                                                        if (Math.Abs(d.GiveSite - d._.CurrentSite) > d.limit)
                                                        {
                                                            if (d._.ActionStatus == ActionEnum.停止)
                                                            {
                                                                // 定位对接点
                                                                d.ToSite(d.GiveSite);
                                                            }
                                                        }
                                                        else
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
                                        if (string.IsNullOrEmpty(d.lockID1) && string.IsNullOrEmpty(d.lockID2))
                                        {
                                            if (d._.GoodsStatus != GoodsEnum.辊台无货 && d._.ActionStatus == ActionEnum.停止)
                                            {
                                                // 锁定
                                                d.StopTask();
                                                d.IsLockUnlockNew(d.taskType, true);
                                            }
                                        }
                                        else
                                        {
                                            if (ADS.mAwc.IsOk(d.area, d.flag) && d._.ActionStatus == ActionEnum.停止)
                                            {
                                                switch (d._.GoodsStatus)
                                                {
                                                    case GoodsEnum.辊台1有货:
                                                    case GoodsEnum.辊台中间有货:
                                                        // 移到2去
                                                        d.StartTakeRoll(d.taskType, 1);
                                                        break;
                                                    case GoodsEnum.辊台无货:
                                                        if (string.IsNullOrEmpty(d.lockID2))
                                                        {
                                                            // 锁定
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID2);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台2有货:
                                                        if (string.IsNullOrEmpty(d.lockID1))
                                                        {
                                                            // 锁定
                                                            d.StopTask();
                                                            d.IsLockUnlockNew(d.taskType, true);
                                                        }
                                                        else
                                                        {
                                                            // 安排行车
                                                            ADS.mAwc.LockAWC(d.area, d.flag, d.taskType, d.lockID1);
                                                        }
                                                        break;
                                                    case GoodsEnum.辊台满货:
                                                        // 锁定
                                                        d.StopTask();
                                                        d.IsLockUnlockNew(d.taskType, true);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // LOG
                    CommonSQL.LogErr("RGV.DoTaskNew()", "运输车作业[设备]", (ex.Message + ex.Source), d.devName);
                    continue;
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

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="jobid"></param>
        public void OverTask(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                TaskRGV t = task.Find(c => c.jobid == jobid);
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    // 解锁设备
                    t.device.IsLockUnlock(false);
                }
                task.RemoveAll(c => c.jobid == jobid);
            }
        }



        /// <summary>
        /// 锁定运输车
        /// </summary>
        /// <returns></returns>
        public bool LockRGV(string area, DevFlag df, TaskTypeEnum tt, string lock1, string lock2, int t, int g)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.isUseful && c.flag == df))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.isUseful && c.flag == df);
                    if (ADS.mSocket.IsConnected(r.devName))
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        r.IsLockUnlockNew(tt, false, lock1, lock2);
                        r.TakeSite = t == 0 ? 0 : t + r.gap;
                        r.GiveSite = g == 0 ? 0 : g + r.gap;
                        res = true;
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
        /// 锁定运输车
        /// </summary>
        /// <returns></returns>
        public bool LockRGV(string area, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) &&
                        r._.CommandStatus == CommandEnum.命令正常 && r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        if (r._.GoodsStatus == GoodsEnum.辊台无货)
                        {
                            if (r._.ActionStatus == ActionEnum.停止)
                            {
                                res = true;
                            }
                        }
                        else
                        {
                            if (r.isLock)
                            {
                                res = true;
                            }
                            else
                            {
                                bool run = false;
                                switch (r.taskType)
                                {
                                    case TaskTypeEnum.入库:
                                        if ((r._.GoodsStatus == GoodsEnum.辊台满货 && r._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                            (r._.GoodsStatus == GoodsEnum.辊台1有货 &&
                                            r._.RollerStatus != RollerStatusEnum.辊台1启动 && r._.RollerStatus != RollerStatusEnum.辊台全启动))
                                        {
                                            run = true;
                                        }
                                        break;
                                    case TaskTypeEnum.出库:
                                        if ((r._.GoodsStatus == GoodsEnum.辊台满货 && r._.RollerStatus == RollerStatusEnum.辊台停止) ||
                                            (r._.GoodsStatus == GoodsEnum.辊台2有货 &&
                                            r._.RollerStatus != RollerStatusEnum.辊台2启动 && r._.RollerStatus != RollerStatusEnum.辊台全启动))
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
                                    r.StopTask();
                                    r.IsLockUnlockNew(r.taskType, true, r.lockID1, r.lockID2);
                                    res = true;
                                }
                                else
                                {
                                    r.StartTakeRoll(r.taskType, 2);
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
        /// 获取当前运输车坐标
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSite(string area, DevFlag df)
        {
            try
            {
                int dis = 0;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);

                    if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                    {
                        r.StopRoller();
                    }

                    if (ADS.mSocket.IsConnected(r.devName) && r._.ActionStatus == ActionEnum.停止)
                    {
                        dis = r._.CurrentSite - r.gap;
                    }
                }
                else
                {
                    // 不在线随意给呗
                    dis = -1;
                }
                return dis;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前运输车载货
        /// </summary>
        /// <returns></returns>
        public int GetGoodsStatus(string area, DevFlag df)
        {
            try
            {
                int g = 0;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) && r._.ActionStatus == ActionEnum.停止)
                    {
                        switch (r._.GoodsStatus)
                        {
                            case GoodsEnum.辊台无货:
                                if (!string.IsNullOrEmpty(r.lockID1))
                                {
                                    g++;
                                }
                                if (!string.IsNullOrEmpty(r.lockID2))
                                {
                                    g++;
                                }
                                break;
                            case GoodsEnum.辊台1有货:
                                g = 1;
                                if (!string.IsNullOrEmpty(r.lockID2))
                                {
                                    g++;
                                }
                                break;
                            case GoodsEnum.辊台2有货:
                                g = 1;
                                if (!string.IsNullOrEmpty(r.lockID1))
                                {
                                    g++;
                                }
                                break;
                            case GoodsEnum.辊台中间有货:
                                g = 1;
                                if (r.taskType == TaskTypeEnum.入库 && !string.IsNullOrEmpty(r.lockID2))
                                {
                                    g++;
                                }
                                if (r.taskType == TaskTypeEnum.出库 && !string.IsNullOrEmpty(r.lockID1))
                                {
                                    g++;
                                }
                                break;
                            case GoodsEnum.辊台满货:
                                g = 2;
                                break;
                            default:
                                break;
                        }
                    }
                }
                return g;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前运输任务类型
        /// </summary>
        /// <returns></returns>
        public TaskTypeEnum GetNowTaskType(string area)
        {
            try
            {
                TaskTypeEnum tt = TaskTypeEnum.无;
                if (devices.Exists(c => c.area == area && c.flag == DevFlag.靠近入库口 && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == DevFlag.靠近入库口 && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) &&
                        r._.CommandStatus != CommandEnum.命令错误 && r._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        tt = r.taskType;
                    }
                }
                return tt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否解锁 By Task
        /// </summary>
        /// <returns></returns>
        public bool IsUnlock(string lockid)
        {
            try
            {
                bool res = true;
                if (devices.Exists(c => c.lockID1 == lockid || c.lockID2 == lockid))
                {
                    res = false;
                    DevInfoRGV r = devices.Find(c => c.lockID1 == lockid || c.lockID2 == lockid);
                    if (r.lockID1 == lockid)
                    {
                        r.IsLockUnlockNew(r.taskType, r.isLock, "", r.lockID2);
                    }
                    if (r.lockID2 == lockid)
                    {
                        r.IsLockUnlockNew(r.taskType, r.isLock, r.lockID1, "");
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
        /// 是否解锁
        /// </summary>
        /// <returns></returns>
        public bool IsUnlock(string area, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);

                    if (ADS.mSocket.IsConnected(r.devName) && !r.isLock && r.taskType == TaskTypeEnum.无 &&
                        r._.CommandStatus == CommandEnum.命令正常 && r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        res = true;
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
        /// 是否有空闲运输车
        /// </summary>
        /// <returns></returns>
        public bool IsFreeRGV(string area, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);

                    if (ADS.mSocket.IsConnected(r.devName) &&
                        string.IsNullOrEmpty(r.lockID1) && string.IsNullOrEmpty(r.lockID2) &&
                        r._.CommandStatus != CommandEnum.命令错误 && r._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        if (!r.isLock)
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
        /// 是否有锁定运输车
        /// </summary>
        /// <returns></returns>
        public bool IsLockRGV(string area, DevFlag df, out int goods)
        {
            try
            {
                bool res = false;
                goods = 0;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);

                    if (ADS.mSocket.IsConnected(r.devName) &&
                        string.IsNullOrEmpty(r.lockID1) && string.IsNullOrEmpty(r.lockID2) &&
                        r._.CommandStatus != CommandEnum.命令错误 && r._.DeviceStatus != DeviceEnum.设备故障)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        switch (r._.GoodsStatus)
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

                        if (r.isLock)
                        {
                            if (r._.GoodsStatus == GoodsEnum.辊台无货 && r._.ActionStatus == ActionEnum.停止)
                            {
                                // 停止&解锁
                                r.StopTask();
                                r.IsLockUnlockNew(r.taskType, false);
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
        /// 是否有锁定运输车 BY Task
        /// </summary>
        /// <returns></returns>
        public bool IsLockRGV(string lockid, out int num)
        {
            try
            {
                bool res = false;
                num = 0;
                if (devices.Exists(c => c.lockID1 == lockid || c.lockID2 == lockid))
                {
                    DevInfoRGV a = devices.Find(c => c.lockID1 == lockid || c.lockID2 == lockid);

                    num = a.lockID1 == lockid ? 1 : 2;
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
        /// 移动对接点
        /// </summary>
        public bool MovingButtSite(string area, DevFlag df, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);

                    if (ADS.mSocket.IsConnected(r.devName) &&
                        r._.CommandStatus == CommandEnum.命令正常 &&
                        r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        // |对接点 - 当前坐标| > 允许范围
                        if (Math.Abs(site + r.gap - r._.CurrentSite) > r.limit)
                        {
                            if (r._.ActionStatus == ActionEnum.停止)
                            {
                                // 定位对接点
                                r.ToSite(site + r.gap);
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
        /// 移动对接点 By Task
        /// </summary>
        public bool MovingButtSite(string lockid, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.lockID1 == lockid || c.lockID2 == lockid))
                {
                    DevInfoRGV r = devices.Find(c => c.lockID1 == lockid || c.lockID2 == lockid);

                    if (r.isUseful && ADS.mSocket.IsConnected(r.devName) &&
                        r._.CommandStatus == CommandEnum.命令正常 &&
                        r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }

                        // |对接点 - 当前坐标| > 允许范围
                        if (Math.Abs(site + r.gap - r._.CurrentSite) > r.limit)
                        {
                            if (r._.ActionStatus == ActionEnum.停止)
                            {
                                if (BeSafe(r.area, r.flag == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口, site))
                                {
                                    // 定位对接点
                                    r.ToSite(site + r.gap);
                                }
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
        /// 防撞
        /// </summary>
        /// <returns></returns>
        public bool BeSafe(string area, DevFlag df, int site)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) &&
                        r._.CommandStatus == CommandEnum.命令正常 && r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.CurrentTask == TaskEnum.辊台任务 && r._.FinishTask == TaskEnum.辊台任务)
                        {
                            r.StopRoller();
                        }
                        int dis = ADS.mDis.GetRgvButtDis(area);
                        if (dis == 0) throw new Exception("无对应运输车对接距离！");

                        if (Math.Abs(r._.CurrentSite - site + r.gap + r.limit) >= dis)
                        {
                            res = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(r.lockID1) && string.IsNullOrEmpty(r.lockID2))
                            {
                                if (r._.ActionStatus == ActionEnum.停止)
                                {
                                    switch (df)
                                    {
                                        case DevFlag.靠近入库口:
                                            r.ToSite(site + r.gap - dis);
                                            res = true;
                                            break;
                                        case DevFlag.远离入库口:
                                            r.ToSite(site + r.gap + dis);
                                            res = true;
                                            break;
                                        default:
                                            break;
                                    }
                                }
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
        /// 是否接货中
        /// </summary>
        /// <returns></returns>
        public bool IsTaking(string area, TaskTypeEnum tt, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) && r._.CommandStatus == CommandEnum.命令正常 && r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.ActionStatus == ActionEnum.停止)
                        {
                            r.StartTakeRoll(tt, 2);
                        }
                        else
                        {
                            if (r._.CurrentTask == TaskEnum.辊台任务) res = true;
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
        /// 是否送货中
        /// </summary>
        /// <returns></returns>
        public bool IsGiving(string area, TaskTypeEnum tt, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df && c.isUseful))
                {
                    DevInfoRGV r = devices.Find(c => c.area == area && c.flag == df && c.isUseful);
                    if (ADS.mSocket.IsConnected(r.devName) && r._.CommandStatus == CommandEnum.命令正常 && r._.DeviceStatus == DeviceEnum.设备正常)
                    {
                        if (r._.ActionStatus == ActionEnum.停止)
                        {
                            r.StartGiveRoll(tt);
                        }
                        else
                        {
                            if (r._.CurrentTask == TaskEnum.辊台任务)
                            {
                                if (r._.GoodsStatus != GoodsEnum.辊台无货)
                                {
                                    if (r._.CurrentTask == TaskEnum.辊台任务) r.StartGiveRoll(tt);
                                }
                                else
                                {
                                    if (r._.FinishTask == TaskEnum.辊台任务)
                                    {
                                        r.StopRoller();
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
        /// 停止所有辊台任务
        /// </summary>
        public void StopALLRoll()
        {
            try
            {
                if (devices == null || devices.Count == 0) return;

                foreach (DevInfoRGV d in devices.FindAll(c => c._.CurrentTask == TaskEnum.辊台任务))
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
        /// 仅单车作业时改变属性
        /// </summary>
        /// <param name="area"></param>
        public void ChangeFlag(string area)
        {
            try
            {
                if (devices == null || devices.Count == 0) return;

                if (devices.Exists(c => c.area == area && c.isUseful))
                {
                    if (devices.FindAll(c => c.area == area && c.isUseful).Count == 1)
                    {
                        devices.Find(c => c.area == area && c.isUseful).flag = DevFlag.靠近入库口;
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
