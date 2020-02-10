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
    public class MasterPKL : BaseMaster
    {
        #region [ 构造 ]

        /// <summary>
        /// 所有包装线设备数据
        /// </summary>
        public List<DevInfoPKL> devices;

        /// <summary>
        /// 所有包装线任务数据
        /// </summary>
        public List<TaskPKL> task;

        public MasterPKL()
        {
            devices = new List<DevInfoPKL>();
            task = new List<TaskPKL>();
            AddAllPkl();
        }

        #endregion

        #region [ 设备 ]

        /// <summary>
        /// 添加所有包装线信息
        /// </summary>
        private void AddAllPkl()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.包装线);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddPkl(new DevInfoPKL()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    lockID = d.LOCK_ID,
                    _ = new DevicePKL()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoPKL.GetDataOrder());
            }
        }

        /// <summary>
        /// 添加包装线信息
        /// </summary>
        private void AddPkl(DevInfoPKL pkl)
        {
            if (!devices.Exists(c => c.devName == pkl.devName))
            {
                devices.Add(pkl);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="dev"></param>
        public void UpdateDevice(string devName, DevicePKL dev)
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
            task.Add(new TaskPKL()
            {
                id = d.ID,
                jobid = d.JOB_ID,
                area = d.AREA,
                tasktype = (TaskTypeEnum)d.TASK_TYPE,
                taskstatus = ts,
                fromdev = ADS.GetDevTypeEnum(d.DEV_FROM),
                todev = ADS.GetDevTypeEnum(d.DEV_TO),
                goodsnum = d.TAKE_NUM,
                device = string.IsNullOrEmpty(d.DEVICE) ? null : devices.Find(c => c.devName == d.DEVICE)
            });
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(string jobid, string area, TaskTypeEnum tasktype, int goodsnum, DevType fromdev, DevType todev)
        {
            int id = ADS.ID;
            ADS.PlusID();
            TaskPKL t = new TaskPKL()
            {
                id = id,
                jobid = jobid,
                area = area,
                tasktype = tasktype,
                goodsnum = goodsnum,
                fromdev = fromdev,
                todev = todev,
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

            foreach (TaskPKL t in task)
            {
                //if (!t.activie) continue;
                if (t.taskstatus == TaskStatus.finish) continue;
                if (t.device == null)
                {
                    DevInfoPKL device = FindFreeDevice(t.area);
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
                            //t.UpdateStatus(TaskStatus.totakesite);
                            t.UpdateStatus(TaskStatus.togivesite);
                            break;

                        case TaskStatus.totakesite:
                            t.UpdateStatus(TaskStatus.ontakesite);
                            break;

                        case TaskStatus.ontakesite:
                            // 判断是否启动辊台接货
                            if (t.takeready)
                            {
                                if (t.device._.GoodsStatus != GoodsEnum.辊台满货 && t.device._.ActionStatus == ActionEnum.停止)
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
                                // ?请求JOB更新
                                //t.takeready = ADS.JobPartPKL_Take(t.jobid);
                            }
                            break;

                        case TaskStatus.taking:
                            if ((t.goodsnum == 1 && t.device._.GoodsStatus != GoodsEnum.辊台无货) ||
                                (t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
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
                            if (t.todev == DevType.AGV)
                            {
                                t.UpdateStatus(TaskStatus.ongivesite);
                            }
                            else
                            {
                                t.UpdateStatus(TaskStatus.giving);
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
                                // ?请求JOB更新
                                t.giveready = ADS.JobPartPKL_Give(t.jobid);
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
        private DevInfoPKL FindFreeDevice(string area)
        {
            return devices.Find(c => !c.isLock && c.area.Equals(area));
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
        /// 获取包装线设备名
        /// </summary>
        public string GetPklName(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid && c.device != null))
            {
                return task.Find(c => c.jobid == jobid).device.devName;
            }

            return null;
        }

        #endregion

    }
}
