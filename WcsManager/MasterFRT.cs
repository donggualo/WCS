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
                    lockID = d.LOCK_ID,
                    taskType = (TaskTypeEnum)d.FLAG,
                    _ = new DeviceFRT()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoFRT.GetDataOrder());
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
        public void DoTask()
        {
            if (task == null || task.Count == 0) return;

            foreach (TaskFRT t in task)
            {
                //if (!t.activie) continue;
                if (t.taskstatus == TaskStatus.finish) continue;
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    DevInfoFRT device = FindFreeDevice(t.area, t.tasktype, t.frt);
                    if (device != null)
                    {
                        t.device = device;
                        t.device.IsLockUnlock(true, t.jobid);
                        t.UpdateDev();

                        // AGV搬运 同辊台第2托货按收2货运行
                        if(t.fromdev == DevType.AGV && 
                           t.device._.GoodsStatus != GoodsEnum.辊台满货 && t.device._.GoodsStatus != GoodsEnum.辊台无货)
                        {
                            t.goodsnum = 2;
                        }
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
                            }
                            else
                            {
                                t.UpdateStatus(TaskStatus.ontakesite);
                            }
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
                                // ? JOB 更新请求
                                t.takeready = ADS.JobPartFRT_Take(t.jobid, t.fromdev);
                            }
                            break;

                        case TaskStatus.taking:
                            bool isNext = false;
                            switch (t.fromdev)
                            {
                                case DevType.摆渡车:
                                    if ((t.goodsnum == 1 && t.device._.GoodsStatus != GoodsEnum.辊台无货 && t.device._.GoodsStatus != GoodsEnum.辊台满货) ||
                                        (t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
                                    {
                                        isNext = true;
                                    }
                                    break;
                                case DevType.AGV:
                                    if ((t.goodsnum == 1 && t.device._.GoodsStatus != GoodsEnum.辊台无货) ||
                                        (t.goodsnum == 2 && t.device._.GoodsStatus == GoodsEnum.辊台满货))
                                    {
                                        isNext = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            if (isNext)
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
                            // AGV搬运 直接完成
                            if (t.tasktype == TaskTypeEnum.AGV搬运)
                            {
                                t.UpdateStatus(TaskStatus.gived);
                            }
                            else
                            {
                                t.UpdateStatus(TaskStatus.togivesite);
                            }
                            break;

                        case TaskStatus.togivesite:
                            // 无目的设备 直接送货中(出库暂无后续流程)
                            if (t.todev == DevType.空设备)
                            {
                                t.UpdateStatus(TaskStatus.giving);
                            }
                            else
                            {
                                t.UpdateStatus(TaskStatus.ongivesite);
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
                                t.giveready = ADS.JobPartFRT_Give(t.jobid);
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

        #endregion

    }

}
