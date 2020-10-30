using Module;
using Module.DEV;
using ModuleManager.NDC;
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
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.包装线辊台);
            if (list == null || list.Count == 0) return;

            foreach (WCS_CONFIG_DEVICE d in list)
            {
                AddPkl(new DevInfoPKL()
                {
                    devName = d.DEVICE,
                    area = d.AREA,
                    isLock = d.IS_LOCK == 1 ? true : false,
                    isUseful = d.IS_USEFUL == 1 ? true : false,
                    lockID1 = d.LOCK_ID1,
                    lockID2 = d.LOCK_ID2,
                    _ = new DevicePKL()
                });

                ADS.mSocket.AddClient(d.DEVICE, d.IP, d.PORT, DevInfoPKL.GetDataOrder());

                if (d.IS_USEFUL == 0)
                {
                    ADS.mSocket.UpdateUserful(d.DEVICE, false);
                }

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
                device = string.IsNullOrEmpty(d.DEVICE) ? new DevInfoPKL() : devices.Find(c => c.devName == d.DEVICE)
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
                taskstatus = TaskStatus.init,
                device = new DevInfoPKL()
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

        //    foreach (TaskPKL t in task)
        //    {
        //        //if (!t.activie) continue;
        //        if (t.taskstatus == TaskStatus.finish) continue;
        //        if (string.IsNullOrEmpty(t.device.devName))
        //        {
        //            DevInfoPKL device = FindFreeDevice(t.area);
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
        //                    t.UpdateStatus(TaskStatus.ongivesite);
        //                    break;

        //                case TaskStatus.totakesite:
        //                    break;

        //                case TaskStatus.ontakesite:
        //                    break;

        //                case TaskStatus.taking:
        //                    break;

        //                case TaskStatus.taked:
        //                    break;

        //                case TaskStatus.togivesite:
        //                    break;

        //                case TaskStatus.ongivesite:
        //                    // 判断是否启动辊台送货
        //                    if (t.giveready)
        //                    {
        //                        if (t.device._.GoodsStatus != GoodsEnum.辊台无货 && t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.device.StartGiveRoll();
        //                        }

        //                        if (t.device._.CurrentTask == TaskEnum.辊台任务)
        //                        {
        //                            t.UpdateStatus(TaskStatus.giving);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // ? JOB 更新请求
        //                        t.giveready = ADS.JobPartPKL_Give(t.jobid);
        //                    }
        //                    break;

        //                case TaskStatus.giving:
        //                    if (t.device._.GoodsStatus == GoodsEnum.辊台无货)
        //                    {
        //                        if (t.device._.ActionStatus == ActionEnum.停止)
        //                        {
        //                            t.UpdateStatus(TaskStatus.gived);
        //                        }
        //                        else
        //                        {
        //                            t.device.StopTask();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        t.device.StartGiveRoll();
        //                    }
        //                    break;

        //                case TaskStatus.gived:
        //                    // 解锁设备、完成任务
        //                    t.device.IsLockUnlock(false);
        //                    t.device = new DevInfoPKL();
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

            foreach (DevInfoPKL d in devices)
            {
                try
                {
                    bool isCanWork = true;
                    if (!d.isUseful || !ADS.mSocket.IsConnected(d.devName) ||
                        d._.CommandStatus == CommandEnum.命令错误 ||
                        d._.DeviceStatus == DeviceEnum.设备故障 ||
                        string.IsNullOrEmpty(d.lockID2))
                    {
                        isCanWork = false;
                        //continue;
                    }
                    int aid = 0;
                    if (!d.isLock)
                    {
                        if (!PublicParam.IsDoJobAGV || !isCanWork) continue;

                        if (d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台1有货)
                        {
                            if (string.IsNullOrEmpty(d.lockID1))
                            {
                                aid = ADS.mNDCControl.IsExistLoad(d.devName);
                                if (aid == 0)
                                {
                                    // 没锁定就派车锁定
                                    aid = CommonSQL.GetAGVid();
                                    // NDC 任务是否存在
                                    if (!ADS.mNDCControl.IsExist(aid))
                                    {
                                        // ? NDC 生成 AGV 搬运任务  
                                        if (!ADS.mNDCControl.AddNDCTask(aid, d.devName, PublicParam.AgvUnLoadArea, out string result))
                                        {
                                            // LOG
                                            CommonSQL.LogErr("PKL.DoTaskNew()", "AGV生成任务[ID，包装线辊台]", result, aid.ToString(), d.devName);
                                            continue;
                                        }
                                    }
                                }

                                if (aid != 0) d.IsLockUnlockNew(true, aid.ToString(), d.lockID2);
                            }
                            else
                            {
                                d.IsLockUnlockNew(true, d.lockID1, d.lockID2);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(d.lockID1))
                        {
                            if (!isCanWork) continue;
                            // 解锁
                            d.IsLockUnlockNew(false);
                            continue;
                        }
                        else
                        {
                            aid = Convert.ToInt32(d.lockID1);

                            // NDC 任务是否存在
                            if (!ADS.mNDCControl.IsExist(aid))
                            {
                                if (!isCanWork) continue;
                                // 解锁
                                d.IsLockUnlockNew(false);
                                continue;
                            }

                            // NDC 任务状态
                            switch (ADS.mNDCControl.GetStatus(aid))
                            {
                                case NDCPlcStatus.LoadUnReady:
                                case NDCPlcStatus.LoadReady:
                                    if (!isCanWork) break;

                                    if (d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台1有货)
                                    {
                                        // NDC 请求 AGV 启动辊台装货
                                        if (!ADS.mNDCControl.DoLoad(aid, out string result))
                                        {
                                            // LOG
                                            if (!string.IsNullOrEmpty(result))
                                            {
                                                CommonSQL.LogErr("PKL.DoTaskNew()", "AGV辊台装货[ID]", result, aid.ToString());
                                            }
                                        }
                                    }
                                    break;
                                case NDCPlcStatus.Loading:
                                    if (!isCanWork) break;

                                    if (d._.ActionStatus == ActionEnum.停止 && d._.GoodsStatus == GoodsEnum.辊台1有货)
                                    {
                                        d.StartGiveRoll();
                                    }
                                    break;
                                case NDCPlcStatus.Loaded:
                                case NDCPlcStatus.UnloadUnReady:
                                    // NDC 任务是否需要重定向
                                    if (!ADS.mNDCControl.IsRedirected(aid) && !string.IsNullOrEmpty(d.lockID2))
                                    {
                                        // 获取WMS任务目的区域
                                        string area = ADS.GetInAreaWMS(d.area, d.lockID2, out string tid);
                                        string frt = ADS.mFrt.GetRightFRT(area, TaskTypeEnum.入库);

                                        if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(frt)) break;

                                        // NDC 更新 AGV搬运卸货点 
                                        if (!ADS.mNDCControl.DoReDerect(aid, frt, out string result))
                                        {
                                            // LOG
                                            CommonSQL.LogErr("PKL.DoTaskNew()", "AGV更新卸货点[ID]", result, aid.ToString());
                                            break;
                                        }

                                        // 更新
                                        CommonSQL.UpdateWms(tid, (int)WmsTaskStatus.待分配);
                                    }
                                    else
                                    {
                                        // 解锁
                                        d.IsLockUnlockNew(false);
                                    }
                                    break;
                                default:
                                    if (!isCanWork) break;
                                    // 解锁
                                    d.IsLockUnlockNew(false);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //if (ex.ToString().Contains("重复"))
                    //{
                    //    MessageBox.Show(d.devName + ex.ToString());
                    //}
                    // LOG
                    CommonSQL.LogErr("PKL.DoTaskNew()", "包装线作业[设备]", (ex.Message + ex.Source), d.devName);
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
        private DevInfoPKL FindFreeDevice(string area)
        {
            return devices.Find(c => !c.isLock && !c.isUseful && c.area.Equals(area));
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

        /// <summary>
        /// 结束任务
        /// </summary>
        /// <param name="jobid"></param>
        public void OverTask(string jobid)
        {
            if (task.Exists(c => c.jobid == jobid))
            {
                TaskPKL t = task.Find(c => c.jobid == jobid);
                if (string.IsNullOrEmpty(t.device.devName))
                {
                    // 解锁设备
                    t.device.IsLockUnlock(false);
                }
                task.RemoveAll(c => c.jobid == jobid);
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
                    DevInfoPKL p = devices.Find(c => c.devName == devName);

                    //if (p._.ActionStatus == ActionEnum.运行中)
                    //{
                    //    throw new Exception(string.Format("辊台未停止，不处理扫码.", code));
                    //}

                    if (p.lockID2 != code)
                    {
                        //是否存在任务
                        if (!CommonSQL.IsExistsInTask(code))
                        {
                            // 请求WMS分配
                            if (ADS.AssignInArea(p.area, code))
                            {
                                p.IsLockUnlockNew(p.isLock, p.lockID1, code);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("读取二维码[{0}]异常.",code));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // LOG
                CommonSQL.LogErr("PKL.DoTaskNew()", "包装线作业[设备]", (ex.Message + ex.Source), devName);
            }
        }

        #endregion

    }
}
