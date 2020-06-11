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
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID = d.LOCK_ID,
                    flag = (DevFlag)d.FLAG,
                    gapX = d.GAP_X,
                    gapY = d.GAP_Y,
                    gapZ = d.GAP_Z,
                    limitX = d.LIMIT_X,
                    limitY = d.LIMIT_Y,
                    _ = new DeviceAWC()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoAWC.GetDataOrder());
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
        public void DoTask()
        {
            if (task == null || task.Count == 0) return;

            foreach (TaskAWC t in task)
            {
                //if (!t.activie) continue;
                if (t.taskstatus == TaskStatus.finish) continue;
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    DevInfoAWC device = FindFreeDevice(t.area, t.flag);
                    if (device != null)
                    {
                        t.device = device;
                        t.device.IsLockUnlock(true, t.jobid);
                        t.UpdateDev();

                        // 更新接送点 
                        t.takesiteX = t.takesiteX + t.device.gapX;
                        t.takesiteY = t.takesiteY + t.device.gapY;
                        t.takesiteZ = t.takesiteZ + t.device.gapZ;

                        t.givesiteX = t.givesiteX + t.device.gapX;
                        t.givesiteY = t.givesiteY + t.device.gapY;
                        t.givesiteZ = t.givesiteZ + t.device.gapZ;
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
                            // |接货点 - 当前坐标| <= 允许范围
                            if (Math.Abs(t.takesiteX - t.device._.CurrentSiteX) <= t.device.limitX &&
                                Math.Abs(t.takesiteY - t.device._.CurrentSiteY) <= t.device.limitY)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ontakesite);
                                }
                            }
                            else
                            {
                                t.device.ToSite(t.takesiteX, t.takesiteY);
                            }
                            break;

                        case TaskStatus.ontakesite:
                            if (t.takeready)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止 && t.device._.GoodsStatus == AwcGoodsEnum.无货)
                                {
                                    t.device.StartTake(t.takesiteZ);
                                }

                                if (t.device._.ActionStatus == ActionEnum.运行中 && t.device._.CurrentTask == AwcTaskEnum.取货任务)
                                {
                                    t.UpdateStatus(TaskStatus.taking);
                                }
                            }
                            else
                            {
                                // 出库接货(取库存货位)直接执行； 入库接货(取运输车辊台)需判断执行
                                if (t.tasktype == TaskTypeEnum.出库)
                                {
                                    t.takeready = true;
                                }
                                else
                                {
                                    // ? JOB 更新请求
                                    t.takeready = ADS.JobPartAWC_Take(t.taskid);
                                }
                            }
                            break;

                        case TaskStatus.taking:
                            if (t.device._.GoodsStatus == AwcGoodsEnum.有货)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.taked);
                                }
                            }
                            else
                            {
                                t.device.StartTake(t.takesiteZ);
                            }
                            break;

                        case TaskStatus.taked:
                            t.UpdateStatus(TaskStatus.togivesite);
                            break;

                        case TaskStatus.togivesite:
                            // |送货点 - 当前坐标| <= 允许范围
                            if (Math.Abs(t.givesiteX - t.device._.CurrentSiteX) <= t.device.limitX &&
                                Math.Abs(t.givesiteY - t.device._.CurrentSiteY) <= t.device.limitY)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ongivesite);
                                }
                            }
                            else
                            {
                                t.device.ToSite(t.givesiteX, t.givesiteY);
                            }
                            break;

                        case TaskStatus.ongivesite:
                            if (t.giveready)
                            {
                                if (t.device._.GoodsStatus == AwcGoodsEnum.有货 && t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.device.StartGive(t.givesiteZ);
                                }

                                if (t.device._.ActionStatus == ActionEnum.运行中 && t.device._.CurrentTask == AwcTaskEnum.放货任务)
                                {
                                    t.UpdateStatus(TaskStatus.giving);
                                }
                            }
                            else
                            {
                                // 入库送货(放库存货位)直接执行； 出库送货(放运输车辊台)需判断执行
                                if (t.tasktype == TaskTypeEnum.入库)
                                {
                                    t.giveready = true;
                                }
                                else
                                {
                                    // ? JOB 更新请求
                                    t.giveready = ADS.JobPartAWC_Give(t.taskid);
                                }
                            }
                            break;

                        case TaskStatus.giving:
                            if (t.device._.GoodsStatus == AwcGoodsEnum.无货)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.gived);
                                }
                            }
                            else
                            {
                                t.device.StartGive(t.givesiteZ);
                            }
                            break;

                        case TaskStatus.gived:
                            if (t.tasktype == TaskTypeEnum.入库)
                            {
                                // ? JOB 更新请求
                                ADS.JobPartAWC_FinishIn(t.taskid);
                            }
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

        #endregion

    }

}
