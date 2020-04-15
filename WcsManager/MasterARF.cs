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
                    lockID = d.LOCK_ID,
                    taskType = (TaskTypeEnum)d.FLAG,
                    _ = new DeviceARF()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoARF.GetDataOrder());
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
                device = string.IsNullOrEmpty(d.DEVICE) ? null : devices.Find(c => c.devName == d.DEVICE)
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
                taskstatus = TaskStatus.init
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

            foreach (TaskARF t in task)
            {
                //if (!t.activie) continue;
                if (t.taskstatus == TaskStatus.finish) continue;
                if (t.device == null)
                {
                    DevInfoARF device = FindFreeDevice(t.area, t.tasktype);
                    if (device != null)
                    {
                        t.device = device;
                        t.device.IsLockUnlock(true, t.jobid);
                        t.UpdateDev();
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
                            if (t.takesite == 0)
                            {
                                // 入库作业接货固定辊台
                                if (t.tasktype == TaskTypeEnum.入库)
                                {
                                    // ? JOB 更新请求
                                    int Tsite = ADS.JobPartARF_Site(t.jobid);
                                    if (Tsite != 0)
                                    {
                                        t.UpdateSite(Tsite);
                                    }
                                }
                                break;
                            }

                            if (t.device._.CurrentSite == t.takesite)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ontakesite);
                                }
                            }
                            else
                            {
                                // 防撞
                                if (IsHit(t.device, t.takesite))
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
                            // 判断是否启动辊台接货
                            if (t.takeready)
                            {
                                if (t.device._.GoodsStatus == GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.device.StartTakeRoll(t.tasktype, t.goodsnum);
                                }

                                if (t.device._.RollerStatus != RollerStatusEnum.辊台停止 && t.device._.CurrentTask == TaskEnum.辊台任务)
                                {
                                    t.UpdateStatus(TaskStatus.taking);
                                }
                            }
                            else
                            {
                                // ? JOB 更新请求
                                t.takeready = ADS.JobPartARF_Take(t.jobid, t.tasktype);
                            }
                            break;

                        case TaskStatus.taking:
                            if ((t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货) ||
                                (t.goodsnum == 1 && t.tasktype == TaskTypeEnum.入库 && t.device._.GoodsStatus == GoodsEnum.辊台1有货) ||
                                (t.goodsnum == 1 && t.tasktype == TaskTypeEnum.出库 && t.device._.GoodsStatus == GoodsEnum.辊台2有货))
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.taked);
                                }
                            }
                            else
                            {
                                t.device.StartTakeRoll(t.tasktype, t.goodsnum);
                            }
                            break;

                        case TaskStatus.taked:
                            t.UpdateStatus(TaskStatus.togivesite);
                            break;

                        case TaskStatus.togivesite:
                            if (t.givesite == 0)
                            {
                                // 出库作业送货固定辊台
                                if (t.tasktype == TaskTypeEnum.出库)
                                {
                                    // ? JOB 更新请求
                                    int Gsite = ADS.JobPartARF_Site(t.jobid);
                                    if (Gsite != 0)
                                    {
                                        t.UpdateSite(Gsite);
                                    }
                                }
                                break;
                            }

                            if (t.device._.CurrentSite == t.givesite)
                            {
                                if (t.device._.ActionStatus == ActionEnum.停止)
                                {
                                    t.UpdateStatus(TaskStatus.ongivesite);
                                }
                            }
                            else
                            {
                                // 防撞
                                if (IsHit(t.device, t.givesite))
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
                                t.giveready = ADS.JobPartARF_Give(t.jobid, t.tasktype);
                            }
                            break;

                        case TaskStatus.giving:
                            if (t.device._.GoodsStatus == GoodsEnum.辊台无货)
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
        /// 寻找有效设备
        /// </summary>
        /// <param name="tasktype"></param>
        /// <returns></returns>
        private DevInfoARF FindFreeDevice(string area, TaskTypeEnum tasktype)
        {
            return devices.Find(c => !c.isLock && c.area.Equals(area) && c.taskType == tasktype);
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

        #endregion

    }

}
