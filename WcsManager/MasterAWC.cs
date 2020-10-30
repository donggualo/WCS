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
    public class MasterAWC : BaseMaster
    {
        #region [ 构造 ]

        /// <summary>
        /// 所有行车设备数据
        /// </summary>
        public List<DevInfoAWC> devices;

        /// <summary>
        /// 所有行车任务数据
        /// </summary>
        public List<TaskAWC> task;

        public MasterAWC()
        {
            devices = new List<DevInfoAWC>();
            task = new List<TaskAWC>();
            AddAllAwc();
        }

        #endregion

        #region [ 设备 ]

        /// <summary>
        /// 添加所有行车信息
        /// </summary>
        private void AddAllAwc()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.行车);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddAwc(new DevInfoAWC()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    taskType = (TaskTypeEnum)d.TASK_TYPE,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID = d.LOCK_ID1,
                    flag = (DevFlag)d.FLAG,
                    gapX = d.GAP_X,
                    gapY = d.GAP_Y,
                    gapZ = d.GAP_Z,
                    limitX = d.LIMIT_X,
                    limitY = d.LIMIT_Y,
                    _ = new DeviceAWC()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoAWC.GetDataOrder());

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
        /// 添加行车信息
        /// </summary>
        private void AddAwc(DevInfoAWC awc)
        {
            if (!devices.Exists(c => c.devName == awc.devName))
            {
                devices.Add(awc);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void UpdateDevice(string devName, DeviceAWC dev)
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
            task.Add(new TaskAWC()
            {
                id = d.ID,
                jobid = d.JOB_ID,
                taskid = d.TASK_ID,
                area = d.AREA,
                tasktype = (TaskTypeEnum)d.TASK_TYPE,
                taskstatus = ts,
                takesiteX = d.TAKE_SITE_X,
                takesiteY = d.TAKE_SITE_Y,
                takesiteZ = d.TAKE_SITE_Z,
                givesiteX = d.GIVE_SITE_X,
                givesiteY = d.GIVE_SITE_Y,
                givesiteZ = d.GIVE_SITE_Z,
                flag = (DevFlag)d.DEV_FLAG,
                device = string.IsNullOrEmpty(d.DEVICE) ? new DevInfoAWC() : devices.Find(c => c.devName == d.DEVICE)
            });
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(string jobid, string taskid, string area, TaskTypeEnum tasktype, DevFlag flag, string takesite, string givesite)
        {
            int id = ADS.ID;
            ADS.PlusID();
            string[] take = takesite.Split('-');
            string[] give = givesite.Split('-');
            TaskAWC t = new TaskAWC()
            {
                id = id,
                jobid = jobid,
                taskid = taskid,
                area = area,
                tasktype = tasktype,
                flag = flag,
                takesiteX = int.Parse(take[0]),
                takesiteY = int.Parse(take[1]),
                takesiteZ = int.Parse(take[2]),
                givesiteX = int.Parse(give[0]),
                givesiteY = int.Parse(give[1]),
                givesiteZ = int.Parse(give[2]),
                taskstatus = TaskStatus.init,
                device = new DevInfoAWC()
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

        //    foreach (TaskAWC t in task)
        //    {
        //        //if (!t.activie) continue;
        //        if (t.taskstatus == TaskStatus.finish) continue;
        //        if (string.IsNullOrEmpty(t.device.devName))
        //        {
        //            DevInfoAWC device = FindFreeDevice(t.area, t.flag);
        //            if (device != null)
        //            {
        //                t.device = device;
        //                t.device.IsLockUnlock(true, t.jobid);
        //                t.UpdateDev();

        //                // 更新接送点 
        //                t.takesiteX = t.takesiteX + t.device.gapX;
        //                t.takesiteY = t.takesiteY + t.device.gapY;
        //                t.takesiteZ = t.takesiteZ + t.device.gapZ;

        //                t.givesiteX = t.givesiteX + t.device.gapX;
        //                t.givesiteY = t.givesiteY + t.device.gapY;
        //                t.givesiteZ = t.givesiteZ + t.device.gapZ;
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
        //                    // |接货点 - 当前坐标| <= 允许范围
        //                    if (Math.Abs(t.takesiteX - t.device._.CurrentSiteX) <= t.device.limitX &&
        //                        Math.Abs(t.takesiteY - t.device._.CurrentSiteY) <= t.device.limitY)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ontakesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.ToSite(t.takesiteX, t.takesiteY);
        //                    }
        //                    break;

        //                case TaskStatus.ontakesite:
        //                    if (t.takeready)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止 && t.device._.GoodsStatus == AwcGoodsEnum.无货)
        //                        {
        //                            t.device.StartTake(t.takesiteZ);
        //                        }

        //                        if (t.device._.ActionStatus == ActionEnum.运行中 && t.device._.CurrentTask == AwcTaskEnum.取货任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taking);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 出库接货(取库存货位)直接执行； 入库接货(取运输车辊台)需判断执行
        //                        if (t.tasktype == TaskTypeEnum.出库)
        //                        {
        //                            t.takeready = true;
        //                        }
        //                        else
        //                        {
        //                            // ? JOB 更新请求
        //                            t.takeready = ADS.JobPartAWC_Take(t.taskid);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.taking:
        //                    if (t.device._.GoodsStatus == AwcGoodsEnum.有货)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.taked);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.StartTake(t.takesiteZ);
        //                    }
        //                    break;

        //                case TaskStatus.taked:
        //                    t.UpdateStatus(TaskStatus.togivesite);
        //                    break;

        //                case TaskStatus.togivesite:
        //                    // |送货点 - 当前坐标| <= 允许范围
        //                    if (Math.Abs(t.givesiteX - t.device._.CurrentSiteX) <= t.device.limitX &&
        //                        Math.Abs(t.givesiteY - t.device._.CurrentSiteY) <= t.device.limitY)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.ongivesite);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.ToSite(t.givesiteX, t.givesiteY);
        //                    }
        //                    break;

        //                case TaskStatus.ongivesite:
        //                    if (t.giveready)
        //                    {
        //                        if (t.device._.GoodsStatus == AwcGoodsEnum.有货 && t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.device.StartGive(t.givesiteZ);
        //                        }

        //                        if (t.device._.ActionStatus == ActionEnum.运行中 && t.device._.CurrentTask == AwcTaskEnum.放货任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.giving);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // 入库送货(放库存货位)直接执行； 出库送货(放运输车辊台)需判断执行
        //                        if (t.tasktype == TaskTypeEnum.入库)
        //                        {
        //                            t.giveready = true;
        //                        }
        //                        else
        //                        {
        //                            // ? JOB 更新请求
        //                            t.giveready = ADS.JobPartAWC_Give(t.taskid);
        //                        }
        //                    }
        //                    break;

        //                case TaskStatus.giving:
        //                    if (t.device._.GoodsStatus == AwcGoodsEnum.无货)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.gived);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.StartGive(t.givesiteZ);
        //                    }
        //                    break;

        //                case TaskStatus.gived:
        //                    if (t.tasktype == TaskTypeEnum.入库)
        //                    {
        //                        // ? JOB 更新请求
        //                        ADS.JobPartAWC_FinishIn(t.taskid);
        //                    }
        //                    // 解锁设备、完成任务
        //                    t.device.IsLockUnlock(false);
        //                    t.device = new DevInfoAWC();
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

            foreach (DevInfoAWC d in devices)
            {
                try
                {
                    if (!d.isUseful || !ADS.mSocket.IsConnected(d.devName) ||
                        d._.CommandStatus == CommandEnum.命令错误 ||
                        d._.DeviceStatus == DeviceEnum.设备故障)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(d.lockID))
                    {
                        switch (d.taskType)
                        {
                            case TaskTypeEnum.入库:
                                InTask:
                                if (d.IsOkLoc(false))
                                {
                                    if (d.isLock)
                                    {
                                        if (d._.ActionStatus == ActionEnum.停止)
                                        {
                                            if (d._.GoodsStatus == AwcGoodsEnum.无货 &&
                                                (d._.FinishTask == AwcTaskEnum.放货任务 || d._.FinishTask == AwcTaskEnum.复位任务))
                                            {
                                                // 通知WMS完成
                                                if (ADS.mHttp.DoStockInFinishTask(d.lockLocWMS, d.lockID).Contains("OK"))
                                                {
                                                    // 解锁
                                                    if (d.IsOkLoc(true))
                                                    {
                                                        CommonSQL.UpdateWms(d.lockID, (int)WmsTaskStatus.完成);
                                                        d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // |送货点 - 当前坐标| <= 允许范围
                                                if (Math.Abs(d.GiveSiteX - d._.CurrentSiteX) <= d.limitX &&
                                                    Math.Abs(d.GiveSiteY - d._.CurrentSiteY) <= d.limitY)
                                                {
                                                    // 放货
                                                    d.StartGive(d.GiveSiteZ);
                                                }
                                                else
                                                {
                                                    if (IsSafeDis(d.area, d.flag == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口,
                                                        d._.CurrentSiteX - d.gapX, d.GiveSiteX - d.gapX))
                                                    {
                                                        // 定位
                                                        d.ToSite(d.GiveSiteX, d.GiveSiteY);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool isOkRgv = false;
                                        // 是否对应运输车
                                        if (ADS.mRgv.IsLockRGV(d.lockID, out int num))
                                        {
                                            if (ADS.mRgv.MovingButtSite(d.lockID, num == 1 ? d.lockLocRGV1 : d.lockLocRGV2))
                                            {
                                                isOkRgv = true;
                                            }
                                        }
                                        else
                                        {
                                            if (d._.GoodsStatus == AwcGoodsEnum.无货 && d._.ActionStatus == ActionEnum.停止)
                                            {
                                                // 解任务
                                                d.IsLockUnlock(false);
                                                break;
                                            }
                                        }

                                        if (d._.ActionStatus == ActionEnum.停止)
                                        {
                                            if (d._.GoodsStatus == AwcGoodsEnum.有货 && 
                                                (d._.FinishTask == AwcTaskEnum.取货任务 || d._.FinishTask == AwcTaskEnum.复位任务))
                                            {
                                                if (ADS.mRgv.IsUnlock(d.lockID))
                                                {
                                                    // 锁定
                                                    d.IsLockUnlock(true, d.lockID);
                                                    goto InTask;
                                                }
                                            }
                                            else
                                            {
                                                // |接货点 - 当前坐标| <= 允许范围
                                                if (Math.Abs(d.TakeSiteX - d._.CurrentSiteX) <= d.limitX &&
                                                    Math.Abs(d.TakeSiteY - d._.CurrentSiteY) <= d.limitY)
                                                {
                                                    if (isOkRgv)
                                                    {
                                                        // 取货
                                                        d.StartTake(d.TakeSiteZ);
                                                    }
                                                }
                                                else
                                                {
                                                    if (IsSafeDis(d.area, d.flag == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口, 
                                                        d._.CurrentSiteX - d.gapX, d.TakeSiteX - d.gapX))
                                                    {
                                                        // 定位
                                                        d.ToSite(d.TakeSiteX, d.TakeSiteY);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (d._.CurrentTask == AwcTaskEnum.取货任务 && d._.GoodsStatus == AwcGoodsEnum.有货)
                                            {
                                                // 安全取货距离
                                                int s = ADS.mDis.GetAwcTakeRgvDis(d.area);
                                                if (s == 0) throw new Exception("无对应行车安全取完货距离！");
                                                // 解锁任务运输车
                                                if (d._.CurrentSiteZ >= s)
                                                {
                                                    if (ADS.mRgv.IsUnlock(d.lockID))
                                                    {
                                                        // 锁定
                                                        //d.IsLockUnlock(true, d.lockID);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case TaskTypeEnum.出库:
                                OutTask:
                                if (d.IsOkLoc(false))
                                {
                                    if (d.isLock)
                                    {
                                        bool isOkRgv = false;
                                        // 是否对应运输车
                                        if (ADS.mRgv.IsLockRGV(d.lockID, out int num))
                                        {
                                            if (ADS.mRgv.MovingButtSite(d.lockID, num == 1 ? d.lockLocRGV1 : d.lockLocRGV2))
                                            {
                                                isOkRgv = true;
                                            }
                                        }
                                        if (d._.ActionStatus == ActionEnum.停止)
                                        {
                                            if (d._.GoodsStatus == AwcGoodsEnum.无货 && 
                                                (d._.FinishTask == AwcTaskEnum.放货任务 || d._.FinishTask == AwcTaskEnum.复位任务))
                                            {
                                                // 通知WMS完成
                                                if (ADS.mHttp.DoStockOutFinishTask(d.lockLocWMS, d.lockID).Contains("OK"))
                                                {
                                                    if (ADS.mRgv.IsUnlock(d.lockID))
                                                    {
                                                        // 解锁
                                                        if (d.IsOkLoc(true))
                                                        {
                                                            CommonSQL.UpdateWms(d.lockID, (int)WmsTaskStatus.完成);
                                                            d.IsLockUnlockNew(TaskTypeEnum.无, false);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // |放货点 - 当前坐标| <= 允许范围
                                                if (Math.Abs(d.GiveSiteX - d._.CurrentSiteX) <= d.limitX &&
                                                    Math.Abs(d.GiveSiteY - d._.CurrentSiteY) <= d.limitY)
                                                {
                                                    if (isOkRgv)
                                                    {
                                                        // 送货
                                                        d.StartGive(d.GiveSiteZ);
                                                    }
                                                }
                                                else
                                                {
                                                    if (IsSafeDis(d.area, d.flag == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口,
                                                        d._.CurrentSiteX - d.gapX, d.GiveSiteX - d.gapX))
                                                    {
                                                        // 定位
                                                        d.ToSite(d.GiveSiteX, d.GiveSiteY);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (d._.CurrentTask == AwcTaskEnum.放货任务 && d._.GoodsStatus == AwcGoodsEnum.无货)
                                            {
                                                // 安全放货距离
                                                int s = ADS.mDis.GetAwcGiveRgvDis(d.area);
                                                if (s == 0) throw new Exception("无对应行车安全放完货距离！");
                                                // 解锁任务运输车
                                                if (d._.CurrentSiteZ >= s)
                                                {
                                                    //ADS.mRgv.IsUnlock(d.lockID);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (d._.ActionStatus == ActionEnum.停止)
                                        {
                                            if (d._.GoodsStatus == AwcGoodsEnum.有货 && 
                                                (d._.CurrentTask == AwcTaskEnum.取货任务 || d._.CurrentTask == AwcTaskEnum.复位任务))
                                            {
                                                // 锁定
                                                d.IsLockUnlock(true, d.lockID);
                                                goto OutTask;
                                            }
                                            else
                                            {
                                                // 是否对应运输车
                                                if (ADS.mRgv.IsLockRGV(d.lockID, out int num))
                                                {
                                                    ADS.mRgv.MovingButtSite(d.lockID, num == 1 ? d.lockLocRGV1 : d.lockLocRGV2);
                                                }
                                                else
                                                {
                                                    if (d._.GoodsStatus == AwcGoodsEnum.无货)
                                                    {
                                                        // 解任务
                                                        d.IsLockUnlock(false);
                                                        break;
                                                    }
                                                }

                                                // |取货点 - 当前坐标| <= 允许范围
                                                if (Math.Abs(d.TakeSiteX - d._.CurrentSiteX) <= d.limitX &&
                                                    Math.Abs(d.TakeSiteY - d._.CurrentSiteY) <= d.limitY)
                                                {
                                                    // 取货
                                                    d.StartTake(d.TakeSiteZ);
                                                }
                                                else
                                                {
                                                    if (IsSafeDis(d.area, d.flag == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口, 
                                                        d._.CurrentSiteX - d.gapX, d.TakeSiteX - d.gapX))
                                                    {
                                                        // 定位
                                                        d.ToSite(d.TakeSiteX, d.TakeSiteY);
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
                }
                catch (Exception ex)
                {
                    // LOG
                    CommonSQL.LogErr("AWC.DoTaskNew()", "行车作业[设备]", (ex.Message + ex.Source), d.devName);
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
        private DevInfoAWC FindFreeDevice(string area, DevFlag flag)
        {
            return devices.Find(c => !c.isLock && !c.isUseful && c.area.Equals(area) && c.flag == flag);
        }

        /// <summary>
        /// 获取行车设备参考值
        /// </summary>
        public DevFlag[] GetAwcFlag(string area, params int[] x)
        {
            if (x.Length == 0 || x.Length > 4) return null;

            DevFlag[] flags;
            // 安全距离
            int safe = ADS.mDis.GetAwcSafeDis(area);
            // 2货位间距；
            int dis1, dis2, dis3;
            // 行车1/2最近作业距离
            int mindis1, mindis2;
            // 行车1/2单独作业距离
            int alonedis1, alonedis2;
            // 行车1/2同时作业距离；
            int sumdis;

            switch (x.Length)
            {
                case 1:
                    flags = new DevFlag[1];
                    flags[0] = GetNearFlag(x[0], out mindis1);
                    break;

                case 2:
                    flags = new DevFlag[2];
                    mindis1 = GetDisXByFlag(x[0], DevFlag.靠近入库口);
                    mindis2 = GetDisXByFlag(x[1], DevFlag.远离入库口);
                    dis1 = Math.Abs(x[0] - x[1]);
                    alonedis1 = mindis1 + dis1;
                    alonedis2 = mindis2 + dis1;
                    sumdis = mindis1 + mindis2;
                    // 2货位间距 < 安全距离 || 行车1/2同时作业距离 > 行车1/2单独作业距离
                    if (dis1 < safe || sumdis > alonedis1 || sumdis > alonedis2)
                    {
                        if (alonedis1 > alonedis2)
                        {
                            flags[0] = DevFlag.远离入库口;
                            flags[1] = DevFlag.远离入库口;
                        }
                        else
                        {
                            flags[0] = DevFlag.靠近入库口;
                            flags[1] = DevFlag.靠近入库口;
                        }
                    }
                    break;

                case 3:
                    // 第一货与第三货距离 对比 安全距离
                    dis1 = Math.Abs(x[0] - x[2]);
                    if (dis1 < safe)
                    {
                        flags = new DevFlag[2];
                        flags[0] = GetNearFlag(x[0], out mindis1);
                        flags[1] = flags[0];
                    }
                    else
                    {
                        flags = new DevFlag[3];
                        mindis1 = GetDisXByFlag(x[0], DevFlag.靠近入库口);
                        mindis2 = GetDisXByFlag(x[1], DevFlag.远离入库口);
                        dis2 = Math.Abs(x[0] - x[1]);
                        dis3 = Math.Abs(x[1] - x[2]);
                        alonedis1 = mindis1 + dis2;
                        alonedis2 = mindis2 + dis3;
                        if (alonedis1 > alonedis2)
                        {
                            flags[0] = DevFlag.靠近入库口;
                            flags[1] = DevFlag.远离入库口;
                            flags[2] = DevFlag.远离入库口;
                        }
                        else
                        {
                            flags[0] = DevFlag.靠近入库口;
                            flags[1] = DevFlag.靠近入库口;
                            flags[2] = DevFlag.远离入库口;
                        }
                    }
                    break;

                case 4:
                    flags = new DevFlag[2];
                    flags[0] = DevFlag.靠近入库口;
                    flags[1] = DevFlag.远离入库口;
                    break;

                default:
                    flags = null;
                    break;
            }

            return flags;
        }

        /// <summary>
        /// 获取距离最近行车设备参考值
        /// </summary>
        private DevFlag GetNearFlag(int x, out int mindis)
        {
            DevFlag flag = DevFlag.不参考;
            int mindistance = 1000000000;
            foreach (DevInfoAWC awc in devices)
            {
                if (mindistance > awc._.CurrentSiteX - x)
                {
                    mindistance = Math.Abs(awc._.CurrentSiteX - x);
                    flag = awc.flag;
                }
            }
            mindis = mindistance;
            return flag;
        }

        /// <summary>
        /// 获取 X轴值距离
        /// </summary>
        private int GetDisXByFlag(int x, DevFlag flag)
        {
            DevInfoAWC awc = devices.Find(c => c.flag == flag);
            if (awc == null) return 999999;
            return Math.Abs(awc._.CurrentSiteX - x);
        }

        /// <summary>
        /// 添加或更新行车 X轴值
        /// </summary>
        public void AddUpdateDev(string devname, DevFlag flag, int x)
        {
            if (devices.Exists(c => c.devName.Equals(devname)))
            {
                devices.Find(c => c.devName.Equals(devname))._.CurrentSiteX = x;
            }
            else
            {
                devices.Add(new DevInfoAWC() { devName = devname, flag = flag, _ = new DeviceAWC() { CurrentSiteX = x } });
            }
        }

        /// <summary>
        /// 是否任务相符
        /// </summary>
        public bool IsTaskConform(string taskid, TaskStatus ts)
        {
            if (task.Exists(c => c.taskid == taskid && c.taskstatus == ts))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否取货安全
        /// </summary>
        public bool IsTakedOrSafeDis(string taskid, int dis)
        {
            if (task.Exists(c => c.taskid == taskid))
            {
                TaskAWC awc = task.Find(c => c.taskid == taskid);

                switch (awc.taskstatus)
                {
                    case TaskStatus.taking:
                        if (awc.device._.GoodsStatus == AwcGoodsEnum.有货 && awc.device._.CurrentSiteX >= dis)
                        {
                            return true;
                        }
                        break;
                    case TaskStatus.taked:
                    case TaskStatus.togivesite:
                    case TaskStatus.ongivesite:
                    case TaskStatus.giving:
                    case TaskStatus.gived:
                    case TaskStatus.finish:
                        return true;
                    default:
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否放货安全
        /// </summary>
        public bool IsGivedOrSafeDis(string taskid, int dis)
        {
            if (task.Exists(c => c.taskid == taskid))
            {
                TaskAWC awc = task.Find(c => c.taskid == taskid);

                switch (awc.taskstatus)
                {
                    case TaskStatus.giving:
                        if (awc.device._.GoodsStatus == AwcGoodsEnum.无货 && awc.device._.CurrentSiteX >= dis)
                        {
                            return true;
                        }
                        break;
                    case TaskStatus.gived:
                    case TaskStatus.finish:
                        return true;
                    default:
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="jobid"></param>
        public void OverTask(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                TaskAWC t = task.Find(c => c.jobid == jobid);
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    // 解锁设备
                    t.device.IsLockUnlock(false);
                }
                task.RemoveAll(c => c.jobid == jobid);
            }
        }



        /// <summary>
        /// 获取最优行车
        /// </summary>
        /// <param name="area"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public DevFlag GetRightAWC(string area, int x)
        {
            try
            {
                DevFlag df = DevFlag.不参考;
                int y = 0;
                // 2端就不比较了
                if (x <= 15000) return DevFlag.靠近入库口;
                if (x >= 29000) return DevFlag.远离入库口;

                if (devices.Exists(c => c.area == area && c.flag == DevFlag.靠近入库口))
                {
                    df = DevFlag.靠近入库口;
                    y = Math.Abs(devices.Find(c => c.area == area && c.flag == DevFlag.靠近入库口)._.CurrentSiteX - x);
                }

                if (devices.Exists(c => c.area == area && c.flag == DevFlag.远离入库口))
                {
                    if (y == 0 || y > Math.Abs(devices.Find(c => c.area == area && c.flag == DevFlag.远离入库口)._.CurrentSiteX - x))
                    {
                        df = DevFlag.远离入库口;
                    }
                }

                return df;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否抵达取货点
        /// </summary>
        /// <returns></returns>
        public bool IsInTakeSite(string area, DevFlag df, string loc)
        {
            try
            {
                bool res = false;
                if (string.IsNullOrEmpty(loc)) return res;

                if (devices.Exists(c => c.area == area && !c.isLock && c.isUseful && c.flag == df))
                {
                    DevInfoAWC a = devices.Find(c => c.area == area && !c.isLock && c.isUseful && c.flag == df);
                    if (ADS.mSocket.IsConnected(a.devName) && a._.CommandStatus == CommandEnum.命令正常 &&
                        a._.DeviceStatus == DeviceEnum.设备正常 && a._.ActionStatus == ActionEnum.停止)
                    {
                        int x = Convert.ToInt32(loc.Split('-')[0]) + a.gapX;
                        int y = Convert.ToInt32(loc.Split('-')[1]) + a.gapY;
                        // |接货点 - 当前坐标| <= 允许范围
                        if (Math.Abs(x - a._.CurrentSiteX) <= a.limitX &&
                            Math.Abs(y - a._.CurrentSiteY) <= a.limitY)
                        {
                            res = true;
                        }
                        else
                        {
                            if (IsSafeDis(a.area, df == DevFlag.远离入库口 ? DevFlag.靠近入库口 : DevFlag.远离入库口, 
                                a._.CurrentSiteX - a.gapX, x - a.gapX))
                            {
                                a.ToSite(x, y);
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
        /// 行车间是否安全距离
        /// </summary>
        /// <param name="area"></param>
        /// <param name="df"></param>
        /// <param name="tx"></param>
        /// <returns></returns>
        public bool IsSafeDis(string area, DevFlag df, int cx, int tx)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == df))
                {
                    DevInfoAWC a = devices.Find(c => c.area == area && c.flag == df);
                    if (a.isUseful)
                    {
                        if (ADS.mSocket.IsConnected(a.devName) && a._.CommandStatus == CommandEnum.命令正常 && a._.DeviceStatus == DeviceEnum.设备正常)
                        {
                            int disOK = 6000;

                            int dis = ADS.mDis.GetAwcSafeDis(a.area);
                            if (dis == 0) throw new Exception("无对应行车安全距离！");

                            tx = tx + a.gapX;
                            if (Math.Abs(tx - a._.CurrentSiteX) >= disOK)
                            {
                                res = true;
                            }
                            else
                            {
                                if (a._.ActionStatus == ActionEnum.停止)
                                {
                                    if (string.IsNullOrEmpty(a.lockID))
                                    {
                                        a.ToSite(df == DevFlag.靠近入库口 ? (tx - dis) : (tx + dis), a._.CurrentSiteY);
                                    }
                                }
                                else
                                {
                                    if (a._.CurrentTask == AwcTaskEnum.定位任务 && string.IsNullOrEmpty(a.lockID))
                                    {
                                        cx = cx + a.gapX;
                                        if (Math.Abs(cx - a._.CurrentSiteX) >= disOK)
                                        {
                                            res = true;
                                        }
                                        else
                                        {
                                            a.ToSite(df == DevFlag.靠近入库口 ? (tx - dis) : (tx + dis), a._.CurrentSiteY);
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
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 当前行车任务
        /// </summary>
        /// <param name="area"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        public string GetLockid(string area, DevFlag df)
        {
            try
            {
                string mes = "";
                if (devices.Exists(c => c.area == area && c.flag == DevFlag.不参考))
                {
                    mes = devices.Find(c => c.area == area && c.flag == DevFlag.不参考).lockID ?? "";
                }
                else
                {
                    if (devices.Exists(c => c.area == area && c.flag == df))
                    {
                        mes = devices.Find(c => c.area == area && c.flag == df).lockID ?? "";
                    }
                }
                return mes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否能进行行车作业
        /// </summary>
        /// <returns></returns>
        public bool IsOk(string area, DevFlag df)
        {
            try
            {
                bool res = false;
                if (devices.Exists(c => c.area == area && c.flag == DevFlag.不参考))
                {
                    DevInfoAWC a = devices.Find(c => c.area == area && !c.isLock && c.isUseful && c.flag == DevFlag.不参考);
                    if (ADS.mSocket.IsConnected(a.devName) && string.IsNullOrEmpty(a.lockID) &&
                        a._.CommandStatus == CommandEnum.命令正常 && a._.GoodsStatus == AwcGoodsEnum.无货 &&
                        a._.DeviceStatus == DeviceEnum.设备正常 && a._.ActionStatus == ActionEnum.停止)
                    {
                        res = true;
                    }
                }
                else
                {
                    if (devices.Exists(c => c.area == area && !c.isLock && c.isUseful && c.flag == df))
                    {
                        DevInfoAWC a = devices.Find(c => c.area == area && !c.isLock && c.isUseful && c.flag == df);
                        if (ADS.mSocket.IsConnected(a.devName) && string.IsNullOrEmpty(a.lockID) &&
                            a._.CommandStatus == CommandEnum.命令正常 && a._.GoodsStatus == AwcGoodsEnum.无货 &&
                            a._.DeviceStatus == DeviceEnum.设备正常 && a._.ActionStatus == ActionEnum.停止)
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
        /// 安排进行行车作业
        /// </summary>
        /// <returns></returns>
        public void LockAWC(string area, DevFlag df, TaskTypeEnum tt, string lockid)
        {
            try
            {
                if (devices.Exists(c => c.area == area && c.flag == DevFlag.不参考))
                {
                    DevInfoAWC a = devices.Find(c => c.area == area && !c.isLock && c.isUseful && c.flag == DevFlag.不参考);
                    if (ADS.mSocket.IsConnected(a.devName) && string.IsNullOrEmpty(a.lockID) &&
                        a._.CommandStatus == CommandEnum.命令正常 && a._.GoodsStatus == AwcGoodsEnum.无货 &&
                        a._.DeviceStatus == DeviceEnum.设备正常 && a._.ActionStatus == ActionEnum.停止)
                    {
                        a.IsLockUnlockNew(tt, false, lockid);
                    }
                }
                else
                {
                    if (devices.Exists(c => c.area == area && !c.isLock && c.isUseful && c.flag == df))
                    {
                        DevInfoAWC a = devices.Find(c => c.area == area && !c.isLock && c.isUseful && c.flag == df);
                        if (ADS.mSocket.IsConnected(a.devName) && string.IsNullOrEmpty(a.lockID) &&
                            a._.CommandStatus == CommandEnum.命令正常 && a._.GoodsStatus == AwcGoodsEnum.无货 &&
                            a._.DeviceStatus == DeviceEnum.设备正常 && a._.ActionStatus == ActionEnum.停止)
                        {
                            a.IsLockUnlockNew(tt, false, lockid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 是否能取消行车出库任务
        /// </summary>
        /// <returns></returns>
        public bool IsCanDelOut(string tid)
        {
            try
            {
                bool res = true;
                if (devices.Exists(c => c.lockID == tid))
                {
                    DevInfoAWC a = devices.Find(c => c.lockID == tid);
                    if (a._.ActionStatus == ActionEnum.停止)
                    {
                        if (a.isLock || a._.GoodsStatus == AwcGoodsEnum.有货)
                        {
                            res = false;
                        }
                    }
                    else
                    {
                        if (a.isLock || a._.GoodsStatus == AwcGoodsEnum.有货 || a._.CurrentTask != AwcTaskEnum.取货任务)
                        {
                            res = false;
                        }
                    }

                    // 清任务
                    if (res)
                    {
                        if (ADS.mRgv.IsUnlock(tid))
                        {
                            a.StopTask();
                            a.IsLockUnlockNew(TaskTypeEnum.无, false);
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
                        devices.Find(c => c.area == area && c.isUseful).flag = DevFlag.不参考;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 坐标全清
        /// </summary>
        public void ClearLoc(string loc)
        {
            try
            {
                if (devices == null || devices.Count == 0)
                {
                    return;
                }
                foreach (DevInfoAWC a in devices.FindAll(c=>c.lockLocWMS == loc))
                {
                    a.IsOkLoc(true);
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
