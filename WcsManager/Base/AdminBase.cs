using Module;
using System;
using System.Collections.Generic;
using System.Threading;
using ModuleManager.WCS;
using PubResourceManager;
using Socket;
using WcsHttpManager;
using WcsManager.DevModule;
using NdcManager;

namespace WcsManager.Base
{
    public abstract class AdminBase
    {
        #region [ 参数1 ]

        private static bool init = false;//是否已经初始化

        private readonly object _objJ = new object();
        private readonly object _objT = new object();

        /// <summary>
        /// 执行等待间隔
        /// </summary>
        internal const int REFRESH_TIMEOUT = 1 * 1000;

        /// <summary>
        /// 线程开关
        /// </summary>
        private static bool PowerSwitch = true;

        private delegate void DoJobDelegate();
        private static event DoJobDelegate DoJob;

        private delegate void DoTaskDelegate();
        private static event DoTaskDelegate DoTask;

        #endregion

        #region [ 参数2 ]

        /// <summary>
        /// 唯一标识
        /// </summary>
        public static int ID;

        /// <summary>
        /// 设备通讯
        /// </summary>
        public static SocketServer mSocket;

        /// <summary>
        /// 控制激光小车任务和调度
        /// </summary>
        public static NDCControl mNDCControl;

        /// <summary>
        /// 控制提供给WMS的服务
        /// </summary>
        public static HttpServerControl mHttpServer;

        /// <summary>
        /// 请求WMS
        /// </summary>
        public static HttpControl mHttp;

        /// <summary>
        /// 摆渡车
        /// </summary>
        public static MasterARF mArf;

        /// <summary>
        /// 行车
        /// </summary>
        public static MasterAWC mAwc;

        /// <summary>
        /// 固定辊台
        /// </summary>
        public static MasterFRT mFrt;

        /// <summary>
        /// 运输车
        /// </summary>
        public static MasterRGV mRgv;

        /// <summary>
        /// 包装线
        /// </summary>
        public static MasterPKL mPkl;

        /// <summary>
        /// WMS任务
        /// </summary>
        public static List<WmsTask> mWmsTask;

        /// <summary>
        /// 入库作业
        /// </summary>
        public static List<Job> mInJob;

        /// <summary>
        /// 出库作业
        /// </summary>
        public static List<Job> mOutJob;

        /// <summary>
        /// AGV作业
        /// </summary>
        public static List<Job> mAgvJob;

        /// <summary>
        /// 距离点位管理
        /// </summary>
        public static MasterDistance mDis;

        /// <summary>
        /// 临时入库作业
        /// </summary>
        public static List<FrtTempJob> mTempInJob;

        /// <summary>
        /// 临时出库任务
        /// </summary>
        public static List<AwcTempTask> mTempOutJob;

        /// <summary>
        /// 临时搬运任务
        /// </summary>
        public static List<AgvTempTask> mTempAgvJob;

        #endregion

        #region [ 构造函数 ]

        public AdminBase()
        {
            if (!init)
            {
                init = true;

                ID = CommonSQL.GetNewJobDetailID();

                mSocket = new SocketServer();

                mNDCControl = new NDCControl();

                mHttpServer = new HttpServerControl();

                mHttp = new HttpControl();

                mArf = new MasterARF();
                mAwc = new MasterAWC();
                mFrt = new MasterFRT();
                mRgv = new MasterRGV();
                mPkl = new MasterPKL();

                mDis = new MasterDistance();

                mWmsTask = new List<WmsTask>();

                mTempInJob = new List<FrtTempJob>();
                mTempOutJob = new List<AwcTempTask>();
                mTempAgvJob = new List<AgvTempTask>();

                mInJob = new List<Job>();
                mOutJob = new List<Job>();
                mAgvJob = new List<Job>();

                InitData();

                mSocket.AwcDataRecive += mAwc.UpdateDevice;
                mSocket.RgvDataRecive += mRgv.UpdateDevice;
                mSocket.ArfDataRecive += mArf.UpdateDevice;
                mSocket.FrtDataRecive += mFrt.UpdateDevice;
                mSocket.PklDataRecive += mPkl.UpdateDevice;

                DoJob += DoWcsJobIn;
                DoJob += DoWcsJobOut;
                DoJob += DoAgvJob;

                DoTask += mAwc.DoTask;
                DoTask += mRgv.DoTask;
                DoTask += mArf.DoTask;
                DoTask += mFrt.DoTask;
                DoTask += mPkl.DoTask;

                new Thread(RunJob).Start();
                new Thread(RunTask).Start();
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            // WMS 任务资讯
            List<WCS_WMS_TASK> wmsList = CommonSQL.GetTaskInfo();
            if (wmsList != null)
            {
                foreach (WCS_WMS_TASK wms in wmsList)
                {
                    WmsTask wt = new WmsTask()
                    {
                        taskuid = wms.TASK_ID,
                        barcode = wms.BARCODE,
                        tasktype = (TaskTypeEnum)wms.TASK_TYPE,
                        taskstatus = (WmsTaskStatus)wms.TASK_STATUS,
                        dev = wms.FRT,
                        takesite = wms.WMS_LOC_FROM,
                        givesite = wms.WMS_LOC_TO
                    };
                    wt.UpdateSite();

                    if (wt.taskstatus == WmsTaskStatus.init)
                    {
                        switch (wt.tasktype)
                        {
                            case TaskTypeEnum.AGV搬运:
                                AddAgvTempTask(wt);
                                break;
                            case TaskTypeEnum.入库:
                                AddFrtTempJob(wt);
                                break;
                            case TaskTypeEnum.出库:
                                AddAwcTempJob(wt, false);
                                break;
                            default:
                                break;
                        }
                    }

                    AddWms(wt);
                }
            }

            // WCS 作业表头资讯
            List<WCS_JOB_HEADER> headerList = CommonSQL.GetJobHeader();
            if (headerList != null)
            {
                foreach (WCS_JOB_HEADER header in headerList)
                {
                    Job j = new Job()
                    {
                        jobid = header.JOB_ID,
                        area = header.AREA,
                        flag = (DevFlag)header.DEV_FLAG,
                        jobtype = (TaskTypeEnum)header.JOB_TYPE,
                        wmstask1 = string.IsNullOrEmpty(header.TASK_ID1) ? null : mWmsTask.Find(c => c.taskuid == header.TASK_ID1),
                        wmstask2 = string.IsNullOrEmpty(header.TASK_ID2) ? null : mWmsTask.Find(c => c.taskuid == header.TASK_ID2)
                    };
                    switch ((TaskTypeEnum)header.JOB_TYPE)
                    {
                        case TaskTypeEnum.AGV搬运:
                            j.agvstatus = (WcsAgvStatus)header.JOB_STATUS;
                            mAgvJob.Add(j);
                            break;
                        case TaskTypeEnum.入库:
                            j.instatus = (WcsInStatus)header.JOB_STATUS;
                            mInJob.Add(j);
                            break;
                        case TaskTypeEnum.出库:
                            j.outstatus = (WcsOutStatus)header.JOB_STATUS;
                            mOutJob.Add(j);
                            break;
                        default:
                            break;
                    }
                }
            }

            // WCS 作业表体资讯
            List<WCS_JOB_DETAIL> detailList = CommonSQL.GetJobDetail();
            if (detailList != null)
            {
                foreach (WCS_JOB_DETAIL detail in detailList)
                {
                    TaskStatus ts = (TaskStatus)detail.TASK_STATUS;
                    switch (ts)
                    {
                        case TaskStatus.ontakesite:
                        case TaskStatus.taking:
                            ts = TaskStatus.totakesite;
                            break;
                        case TaskStatus.ongivesite:
                        case TaskStatus.giving:
                            ts = TaskStatus.togivesite;
                            break;
                        default:
                            break;
                    }

                    switch (detail.DEV_TYPE)
                    {
                        case DeviceType.包装线:
                            mPkl.InitTask(detail, ts);
                            break;
                        case DeviceType.固定辊台:
                            mFrt.InitTask(detail, ts);
                            break;
                        case DeviceType.摆渡车:
                            mArf.InitTask(detail, ts);
                            break;
                        case DeviceType.运输车:
                            mRgv.InitTask(detail, ts);
                            break;
                        case DeviceType.行车:
                            mAwc.InitTask(detail, ts);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 递增ID
        /// </summary>
        public static void PlusID()
        {
            if (ID >= 999999)
            {
                ID = 0;
            }
            else
            {
                ID++;
            }
        }

        /// <summary>
        /// 运行作业
        /// </summary>
        private void RunJob()
        {
            while (PowerSwitch)
            {
                Thread.Sleep(REFRESH_TIMEOUT);
                lock (_objJ)
                {
                    DoJob?.Invoke();
                }
            }
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        private void RunTask()
        {
            while (PowerSwitch)
            {
                Thread.Sleep(REFRESH_TIMEOUT);

                lock (_objT)
                {
                    if (!PublicParam.IsDoTask) continue;

                    DoTask?.Invoke();
                }
            }
        }

        /// <summary>
        /// 关闭操作
        /// </summary>
        public bool BeforeClose()
        {
            if (init)
            {
                PublicParam.IsDoJobIn = false;
                PublicParam.IsDoJobOut = false;
                PublicParam.IsDoTask = false;
                PowerSwitch = false;

                if (DoJob != null)
                {
                    DoJob -= DoWcsJobIn;
                    DoJob -= DoWcsJobOut;
                    DoJob -= DoAgvJob;
                }

                if (DoTask != null)
                {
                    DoTask -= mAwc.DoTask;
                    DoTask -= mRgv.DoTask;
                    DoTask -= mArf.DoTask;
                    DoTask -= mFrt.DoTask;
                    DoTask -= mPkl.DoTask;
                }

                if (mSocket != null)
                {
                    if (mAwc != null) mSocket.AwcDataRecive -= mAwc.UpdateDevice;
                    if (mRgv != null) mSocket.RgvDataRecive -= mRgv.UpdateDevice;
                    if (mArf != null) mSocket.ArfDataRecive -= mArf.UpdateDevice;
                    if (mFrt != null) mSocket.FrtDataRecive -= mFrt.UpdateDevice;
                    if (mPkl != null) mSocket.PklDataRecive -= mPkl.UpdateDevice;

                    mSocket.Close();
                }
            }

            return true;
        }

        #endregion

        #region [ 入库作业 ]

        /// <summary>
        /// 添加入库临时任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="frt"></param>
        public void AddFrtTempJob(WmsTask task, string frt = null)
        {
            if (string.IsNullOrEmpty(task.givesite))
            {
                return;
            }

            string[] loc = task.givesite.Split('-');
            if (loc.Length < 4)
            {
                return;
            }

            // 更新WCS实际作业坐标
            task.UpdateSite();
            if (!string.IsNullOrEmpty(frt)) task.InsertDB();
            AddWms(task);

            if (mTempInJob.Exists(c => c.frt == frt))
            {
                FrtTempJob job = mTempInJob.Find(c => c.frt == frt);
                job.task2 = task;
            }
            else
            {
                mTempInJob.Add(new FrtTempJob()
                {
                    task1 = task,
                    frt = frt,
                    area = loc[0],
                    createtime = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 检查入库任务 & 生成WCS入库作业
        /// </summary>
        private void CheckFrtTempJob()
        {
            if (mTempInJob == null || mTempInJob.Count == 0) return;

            foreach (FrtTempJob job in mTempInJob)
            {
                if (job.CanStart())
                {
                    AddWcsJob(TaskTypeEnum.入库, job.area, job.task1, job.task2);
                }
            }

            mTempInJob.RemoveAll(c => c.CanStart());
        }

        #endregion

        #region [ 出库作业 ]

        /// <summary>
        /// 添加出库临时任务
        /// </summary>
        /// <param name="task"></param>
        public void AddAwcTempJob(WmsTask task, bool insDB = true)
        {
            if (string.IsNullOrEmpty(task.takesite))
            {
                return;
            }

            string[] loc = task.takesite.Split('-');
            if (loc.Length < 4)
            {
                return;
            }

            // 更新WCS实际作业坐标
            task.UpdateSite();
            if (insDB) task.InsertDB();
            AddWms(task);

            mTempOutJob.Add(new AwcTempTask()
            {
                task = task,
                area = loc[0],
                X = task.site.GetAwcSiteX(),
                Y = task.site.GetAwcSiteY(),
                Z = task.site.GetAwcSiteZ(),
                createtime = DateTime.Now,
            });
        }

        /// <summary>
        /// 出库任务排序
        /// </summary>
        /// <param name="job1"></param>
        /// <param name="job2"></param>
        /// <returns></returns>
        private int SortAwcTempJob(AwcTempTask job1, AwcTempTask job2)
        {
            // -1:job1在前； 0:同序；1:job2在前；
            int res = 0;
            if (job1 == null && job2 == null) return 0;
            else if (job1 != null && job2 == null) return 1;
            else if (job1 == null && job2 != null) return -1;

            // 对比X
            if (job1.X == job2.X)
            {
                // X 相等对比 Y
                if (job1.Y == job2.Y)
                {
                    // Y 相等对比 Z
                    if (job1.Z < job2.Z)
                    {
                        res = 1;
                    }
                    else
                    {
                        res = -1;
                    }
                }
                else if (job1.Y < job2.Y)
                {
                    res = -1;
                }
                else
                {
                    res = 1;
                }
            }
            else if (job1.X < job2.X)
            {
                res = -1;
            }
            else
            {
                res = 1;
            }

            return res;
        }

        /// <summary>
        /// 清除指定WMS出库临时任务
        /// </summary>
        /// <param name="task"></param>
        private void RemoveAwcTempJob(WmsTask task)
        {
            if (mTempOutJob.Exists(c => c.task == task))
            {
                mTempOutJob.RemoveAll(c => c.task == task);
            }
        }

        /// <summary>
        /// 检查出库任务 & 生成WCS出库作业
        /// </summary>
        private void CheckAwcTempJob(string area)
        {
            if (mTempOutJob == null || mTempOutJob.Count == 0) return;
            List<AwcTempTask> Jobs = mTempOutJob.FindAll(c => c.area == area);
            if (Jobs == null || Jobs.Count == 0) return;
            // （离入库口）从近到远排序
            Jobs.Sort(SortAwcTempJob);

            DevFlag[] flags;
            switch (Jobs.Count)
            {
                case 1:
                    flags = mAwc.GetAwcFlag(area, Jobs[0].X);
                    AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, null, flags[0]);
                    RemoveAwcTempJob(Jobs[0].task);
                    break;

                case 2:
                    flags = mAwc.GetAwcFlag(area, Jobs[0].X, Jobs[1].X);
                    if (flags[0] == flags[1])
                    {
                        AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, Jobs[1].task, flags[0]);
                    }
                    else
                    {
                        AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, null, flags[0]);
                        AddWcsJob(TaskTypeEnum.出库, area, Jobs[1].task, null, flags[1]);
                    }
                    RemoveAwcTempJob(Jobs[0].task);
                    RemoveAwcTempJob(Jobs[1].task);
                    break;

                case 3:
                    flags = mAwc.GetAwcFlag(area, Jobs[0].X, Jobs[1].X, Jobs[2].X);
                    switch (flags.Length)
                    {
                        case 2:
                            AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, Jobs[1].task, flags[0]);
                            RemoveAwcTempJob(Jobs[0].task);
                            RemoveAwcTempJob(Jobs[1].task);
                            break;
                        case 3:
                            if (flags[0] == flags[1])
                            {
                                AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, Jobs[1].task, flags[0]);
                                AddWcsJob(TaskTypeEnum.出库, area, Jobs[2].task, null, flags[2]);
                            }
                            else
                            {
                                AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, null, flags[0]);
                                AddWcsJob(TaskTypeEnum.出库, area, Jobs[1].task, Jobs[2].task, flags[2]);
                            }
                            RemoveAwcTempJob(Jobs[0].task);
                            RemoveAwcTempJob(Jobs[1].task);
                            RemoveAwcTempJob(Jobs[2].task);
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    // 获取最近 & 最后的各2笔任务
                    flags = mAwc.GetAwcFlag(area, Jobs[0].X, Jobs[1].X, Jobs[Jobs.Count - 2].X, Jobs[Jobs.Count - 1].X);
                    AddWcsJob(TaskTypeEnum.出库, area, Jobs[0].task, Jobs[1].task, flags[0]);
                    AddWcsJob(TaskTypeEnum.出库, area, Jobs[Jobs.Count - 2].task, Jobs[Jobs.Count - 1].task, flags[1]);
                    RemoveAwcTempJob(Jobs[0].task);
                    RemoveAwcTempJob(Jobs[1].task);
                    RemoveAwcTempJob(Jobs[2].task);
                    RemoveAwcTempJob(Jobs[3].task);
                    break;
            }
        }

        #endregion

        #region [ AGV 搬运 ]

        /// <summary>
        /// 添加搬运临时任务
        /// </summary>
        /// <param name="task"></param>
        public void AddAgvTempTask(WmsTask task, string pkl = null)
        {
            if (string.IsNullOrEmpty(task.takesite) || string.IsNullOrEmpty(task.givesite))
            {
                return;
            }

            if (!string.IsNullOrEmpty(pkl)) task.InsertDB();
            AddWms(task);

            mTempAgvJob.Add(new AgvTempTask()
            {
                task = task,
                areaPK = task.takesite,
                areaWH = task.givesite,
                pkl = pkl,
                createtime = DateTime.Now,
            });
        }

        /// <summary>
        /// 检查AGV任务 & 生成FRT搬运作业
        /// </summary>
        public bool CheckAgvTempTask(string jobid, string pkl)
        {
            if (mTempAgvJob == null || mTempAgvJob.Count == 0) return false;

            if (mTempAgvJob.Exists(c => c.pkl == pkl && c.CanStart()))
            {
                AgvTempTask agv = mTempAgvJob.Find(c => c.pkl == pkl && c.CanStart());
                // 库区辊台任务
                mFrt.AddTask(jobid, agv.areaWH, TaskTypeEnum.AGV搬运, 1, DevType.AGV, DevType.空设备);

                mTempAgvJob.Remove(agv);

                return true;
            }
            return false;

        }

        #endregion

        #region [ 作业执行 ]

        /// <summary>
        /// 添加WCS作业
        /// </summary>
        private void AddWcsJob(TaskTypeEnum type, string area, WmsTask task1, WmsTask task2, DevFlag flag = DevFlag.不参考)
        {
            Job wcs = new Job()
            {
                jobid = DateTime.Now.ToString("yyyyMMddHHmmffff"),
                area = area,
                wmstask1 = task1,
                wmstask2 = task2,
                jobtype = type,
                flag = flag
            };
            switch (type)
            {
                case TaskTypeEnum.AGV搬运:
                    wcs.jobid = "A" + wcs.jobid;
                    mAgvJob.Add(wcs);
                    break;

                case TaskTypeEnum.入库:
                    wcs.jobid = "I" + wcs.jobid;
                    mInJob.Add(wcs);
                    break;

                case TaskTypeEnum.出库:
                    wcs.jobid = "O" + wcs.jobid;
                    mOutJob.Add(wcs);
                    break;
                default:
                    return;
            }
            wcs.InsertDB();
            if (task1 != null) task1.UpdateStatus(WmsTaskStatus.excuting);
            if (task2 != null) task2.UpdateStatus(WmsTaskStatus.excuting);
        }

        /// <summary>
        /// 执行WCS入库作业
        /// </summary>
        private void DoWcsJobIn()
        {
            if (!PublicParam.IsDoJobIn) return;

            // 生成任务
            CheckFrtTempJob();

            // 运行任务
            if (mInJob == null || mInJob.Count == 0) return;
            foreach (Job j in mInJob)
            {
                // 货物数量 = 任务数量
                int goodsNum = j.wmstask2 == null ? 1 : 2;

                switch (j.instatus)
                {
                    case WcsInStatus.init:
                        // 固定辊台任务
                        mFrt.AddTask(j.jobid, j.area, j.jobtype, goodsNum, DevType.空设备, DevType.摆渡车);

                        // 摆渡车任务
                        int arfPfrt = j.wmstask1.site.arfbuttfrt; // 摆渡车对接固定辊台坐标
                        int arfPrgv = mDis.GetArfButtP(j.area);      // 摆渡车对接运输车坐标
                        mArf.AddTask(j.jobid, j.area, j.jobtype, goodsNum, arfPfrt, arfPrgv);

                        // 运输车任务
                        MTask rgvT = new MTask();
                        MPoint rgvP1 = new MPoint();
                        MPoint rgvP2 = new MPoint();
                        if (goodsNum == 1)
                        {
                            rgvT.task1 = j.wmstask1.taskuid;
                            rgvP1.takesite = mDis.GetRgvButtP(j.area);
                            rgvP1.givesite = j.wmstask1.site.rgvsite1;
                        }
                        else
                        {
                            if (j.wmstask1.site.rgvsite1 > j.wmstask2.site.rgvsite2)
                            {
                                rgvT.task1 = j.wmstask2.taskuid;
                                rgvT.task2 = j.wmstask1.taskuid;

                                rgvP1.takesite = rgvP2.takesite = mDis.GetRgvButtP(j.area);

                                rgvP1.givesite = j.wmstask2.site.rgvsite2;
                                rgvP2.givesite = j.wmstask1.site.rgvsite1;
                            }
                            else
                            {
                                rgvT.task1 = j.wmstask1.taskuid;
                                rgvT.task2 = j.wmstask2.taskuid;

                                rgvP1.takesite = rgvP2.takesite = mDis.GetRgvButtP(j.area);

                                rgvP1.givesite = j.wmstask1.site.rgvsite1;
                                rgvP2.givesite = j.wmstask2.site.rgvsite2;
                            }
                        }
                        mRgv.AddTask(j.jobid, rgvT, j.area, j.jobtype, rgvP1, rgvP2, goodsNum, goodsNum);

                        // 行车任务
                        string awcTask1;
                        string awcTake1;
                        string awcGive1;
                        string awcTask2;
                        string awcTake2;
                        string awcGive2;
                        int awcgiveX1;
                        int awcgiveX2;
                        if (goodsNum == 1)
                        {
                            awcTask1 = j.wmstask1.taskuid;
                            awcgiveX1 = j.wmstask1.site.GetAwcButtRgvX();
                            awcTake1 = j.wmstask1.site.awcbuttRgv;
                            awcGive1 = j.wmstask1.site.awcsite;

                            DevFlag[] flags = mAwc.GetAwcFlag(j.area, awcgiveX1);
                            mAwc.AddTask(j.jobid, awcTask1, j.area, j.jobtype, flags[0], awcTake1, awcGive1);
                        }
                        else
                        {
                            if (rgvP1.givesite == j.wmstask1.site.rgvsite1)
                            {
                                awcTask1 = j.wmstask1.taskuid;
                                awcTake1 = j.wmstask1.site.awcbuttRgv;
                                awcGive1 = j.wmstask1.site.awcsite;
                                awcTask2 = j.wmstask2.taskuid;
                                awcTake2 = j.wmstask2.site.awcbuttRgv;
                                awcGive2 = j.wmstask2.site.awcsite;
                            }
                            else
                            {
                                awcTask1 = j.wmstask2.taskuid;
                                awcTake1 = j.wmstask2.site.awcbuttRgv;
                                awcGive1 = j.wmstask2.site.awcsite;
                                awcTask2 = j.wmstask1.taskuid;
                                awcTake2 = j.wmstask1.site.awcbuttRgv;
                                awcGive2 = j.wmstask1.site.awcsite;
                            }

                            awcgiveX1 = int.Parse(j.wmstask1.site.awcsite.Split('-')[0]);
                            awcgiveX2 = int.Parse(j.wmstask2.site.awcsite.Split('-')[0]);
                            if (Math.Abs(awcgiveX1 - awcgiveX2) < mDis.GetAwcSafeDis(j.area))
                            {
                                DevFlag[] flags = mAwc.GetAwcFlag(j.area, awcgiveX1, awcgiveX2);
                                mAwc.AddTask(j.jobid, awcTask1, j.area, j.jobtype, flags[0], awcTake1, awcGive1);
                                mAwc.AddTask(j.jobid, awcTask2, j.area, j.jobtype, flags[1], awcTake2, awcGive2);
                            }
                            else
                            {
                                mAwc.AddTask(j.jobid, awcTask1, j.area, j.jobtype, DevFlag.靠近入库口, awcTake1, awcGive1);
                                mAwc.AddTask(j.jobid, awcTask2, j.area, j.jobtype, DevFlag.远离入库口, awcTake2, awcGive2);
                            }
                        }
                        j.UpdateStatus((int)WcsInStatus.frttoarf);
                        break;

                    case WcsInStatus.frttoarf:
                        if (mFrt.IsTaskConform(j.jobid, j.jobtype, TaskStatus.finish))
                        {
                            j.UpdateStatus((int)WcsInStatus.arftorgv);
                        }
                        break;

                    case WcsInStatus.arftorgv:
                        if (mArf.IsTaskConform(j.jobid, TaskStatus.finish))
                        {
                            j.UpdateStatus((int)WcsInStatus.rgvtoawc);
                        }
                        break;

                    case WcsInStatus.rgvtoawc:
                        bool result = false;
                        switch (goodsNum)
                        {
                            case 1:
                                result = mAwc.IsTaskConform(j.wmstask1.taskuid, TaskStatus.finish);
                                break;
                            case 2:
                                result = (mAwc.IsTaskConform(j.wmstask1.taskuid, TaskStatus.finish) && mAwc.IsTaskConform(j.wmstask2.taskuid, TaskStatus.finish));
                                break;
                            default:
                                break;
                        }
                        if (result) j.UpdateStatus((int)WcsInStatus.finish);
                        break;

                    case WcsInStatus.finish:
                        // ?WMS完成
                        if (j.wmstask1 != null) FinishWms(j.wmstask1.taskuid);
                        if (j.wmstask2 != null) FinishWms(j.wmstask2.taskuid);
                        // ?备份
                        CommonSQL.Backup(j.jobid);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 执行WCS出库作业
        /// </summary>
        private void DoWcsJobOut()
        {
            if (!PublicParam.IsDoJobOut) return;

            // 生成任务
            if (mDis != null)
            {
                foreach (AreaDistance m in mDis.distances)
                {
                    CheckAwcTempJob(m.area);
                }
            }

            // 运行任务
            if (mOutJob == null || mOutJob.Count == 0) return;
            foreach (Job j in mOutJob)
            {
                // 货物数量 = 任务数量
                int goodsNum = j.wmstask2 == null ? 1 : 2;

                switch (j.outstatus)
                {
                    case WcsOutStatus.init:
                        // 固定辊台任务
                        mFrt.AddTask(j.jobid, j.area, j.jobtype, goodsNum, DevType.摆渡车, DevType.空设备);

                        // 摆渡车任务
                        int arfPfrt = j.wmstask1.site.arfbuttfrt; // 摆渡车对接固定辊台坐标
                        int arfPrgv = mDis.GetArfButtP(j.area);      // 摆渡车对接运输车坐标
                        mArf.AddTask(j.jobid, j.area, j.jobtype, goodsNum, arfPrgv, arfPfrt);

                        // 运输车任务 & 行车任务
                        MTask mt = new MTask();
                        switch (goodsNum)
                        {
                            case 1:
                                mt.task1 = j.wmstask1.taskuid;
                                MPoint rgvP = new MPoint() { takesite = j.wmstask1.site.rgvsite1, givesite = mDis.GetRgvButtP(j.area) };
                                mRgv.AddTask(j.jobid, mt, j.area, j.jobtype, rgvP, null, goodsNum, goodsNum, j.flag);
                                mAwc.AddTask(j.jobid, mt.task1, j.area, j.jobtype, j.flag, j.wmstask1.site.awcsite, j.wmstask1.site.awcbuttRgv);
                                break;
                            case 2:
                                mt.task1 = j.wmstask1.taskuid;
                                mt.task2 = j.wmstask2.taskuid;
                                MPoint rgvP1 = new MPoint() { takesite = j.wmstask1.site.rgvsite1, givesite = mDis.GetRgvButtP(j.area) };
                                MPoint rgvP2 = new MPoint() { takesite = j.wmstask2.site.rgvsite2, givesite = mDis.GetRgvButtP(j.area) };
                                mRgv.AddTask(j.jobid, mt, j.area, j.jobtype, rgvP1, rgvP2, goodsNum, goodsNum, j.flag);

                                mAwc.AddTask(j.jobid, mt.task1, j.area, j.jobtype, j.flag, j.wmstask1.site.awcsite, j.wmstask1.site.awcbuttRgv);
                                mAwc.AddTask(j.jobid, mt.task2, j.area, j.jobtype, j.flag, j.wmstask2.site.awcsite, j.wmstask2.site.awcbuttRgv);
                                break;
                            default:
                                break;
                        }
                        j.UpdateStatus((int)WcsOutStatus.awctorgv);
                        break;

                    case WcsOutStatus.awctorgv:
                        bool result = false;
                        switch (goodsNum)
                        {
                            case 1:
                                result = mAwc.IsTaskConform(j.wmstask1.taskuid, TaskStatus.finish);
                                break;
                            case 2:
                                result = (mAwc.IsTaskConform(j.wmstask1.taskuid, TaskStatus.finish) && mAwc.IsTaskConform(j.wmstask1.taskuid, TaskStatus.finish));
                                break;
                            default:
                                break;
                        }
                        if (result) j.UpdateStatus((int)WcsOutStatus.rgvtoarf);
                        break;

                    case WcsOutStatus.rgvtoarf:
                        if ((int)mArf.GetStatusARF(j.jobid) >= (int)TaskStatus.taked)
                        {
                            j.UpdateStatus((int)WcsOutStatus.arftofrt);
                        }
                        break;

                    case WcsOutStatus.arftofrt:
                        if (mFrt.IsTaskConform(j.jobid, j.jobtype, TaskStatus.finish))
                        {
                            j.UpdateStatus((int)WcsOutStatus.finish);
                        }
                        break;

                    case WcsOutStatus.finish:
                        // ?WMS完成
                        if (j.wmstask1 != null) FinishWms(j.wmstask1.taskuid);
                        if (j.wmstask2 != null) FinishWms(j.wmstask2.taskuid);
                        // ?备份
                        CommonSQL.Backup(j.jobid);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 执行AGV搬运作业
        /// </summary>
        private void DoAgvJob()
        {
            if (!PublicParam.IsDoJobAGV) return;

            // 生成任务
            if (mDis != null)
            {
                // 包装线区域皆为 A 开头
                foreach (AreaDistance m in mDis.distances.FindAll(c => c.area.StartsWith("A")))
                {
                    // 同一包装线区域仅存留 1个 搬运作业
                    if (mAgvJob.FindAll(c => c.area == m.area && c.agvstatus == WcsAgvStatus.init).Count >= 1) return;

                    AddWcsJob(TaskTypeEnum.AGV搬运, m.area, null, null);
                }
            }

            // 运行任务
            if (mAgvJob == null || mAgvJob.Count == 0) return;
            foreach (Job j in mAgvJob)
            {
                int aID = Convert.ToInt32(j.jobid.Substring(1));
                string pkl = mPkl.GetPklName(j.jobid);
                if (j.agvstatus != WcsAgvStatus.init && string.IsNullOrEmpty(pkl)) break;

                switch (j.agvstatus)
                {
                    case WcsAgvStatus.init:
                        // 包装线辊台任务
                        mPkl.AddTask(j.jobid, j.area, TaskTypeEnum.AGV搬运, 1, DevType.空设备, DevType.AGV);
                        j.UpdateStatus((int)WcsAgvStatus.pktoagv);
                        break;

                    case WcsAgvStatus.pktoagv:
                        // ? AGV 任务是否存在
                        if (true)
                        {
                            // ? 发送 NDC 生成 AGV 搬运任务  
                            if (!mNDCControl.AddNDCTask(aID, pkl, PublicParam.AgvUnLoadArea, out string result))
                            {
                                // LOG
                                break;
                            }
                        }

                        if (mPkl.IsTaskConform(j.jobid, TaskStatus.ongivesite))
                        {
                            // ? 发送 NDC 请求 AGV 启动辊台装货
                            if (!mNDCControl.DoLoad(aID, 0, out string result))
                            {
                                // LOG
                                break;
                            }
                        }

                        if (mPkl.IsTaskConform(j.jobid, TaskStatus.giving) ||
                            mPkl.IsTaskConform(j.jobid, TaskStatus.finish))
                        {
                            j.UpdateStatus((int)WcsAgvStatus.waitforwms);
                        }
                        break;

                    case WcsAgvStatus.waitforwms:
                        if (CheckAgvTempTask(j.jobid, pkl))
                        {
                            j.UpdateStatus((int)WcsAgvStatus.agvtowh);
                        }
                        break;

                    case WcsAgvStatus.agvtowh:
                        // ? AGV 任务是否需要重定向
                        if (true)
                        {
                            // ? 发送 NDC 更新 AGV搬运卸货点 
                            if (!mNDCControl.DoReDerect(aID, mPkl.GetPklName(j.jobid), out string result))
                            {
                                // LOG
                                break;
                            }
                        }

                        if (mFrt.IsTaskConform(j.jobid, TaskTypeEnum.AGV搬运, TaskStatus.taking))
                        {
                            // ? 发送 NDC 请求 AGV 启动辊台卸货
                            if (!mNDCControl.DoUnLoad(aID, 0, out string result))
                            {
                                // LOG
                                break;
                            }
                        }

                        if (mFrt.IsTaskConform(j.jobid, TaskTypeEnum.AGV搬运, TaskStatus.finish))
                        {
                            j.UpdateStatus((int)WcsAgvStatus.finish);
                        }
                        break;

                    case WcsAgvStatus.finish:
                        // ? 请求WMS生成货位入库任务
                        break;
                    default:
                        break;
                }
            }

        }

        #endregion

        #region [ WMS ]

        public void AddWms(WmsTask wt)
        {
            if (mWmsTask.Exists(c => c.taskuid == wt.taskuid))
            {
                mWmsTask.RemoveAll(c => c.taskuid == wt.taskuid);
            }

            mWmsTask.Add(wt);
        }

        /// <summary>
        /// 添加 WMS 出库任务
        /// </summary>
        public bool AddWmsOutTask(WmsModel wms, out string result)
        {
            try
            {
                if (wms.Task_type != WmsStatus.StockOutTask)
                {
                    result = "仅接收出库任务";
                    return false;
                }
                else
                {
                    AddAwcTempJob(new WmsTask()
                    {
                        taskuid = wms.Task_UID,
                        tasktype = TaskTypeEnum.出库,
                        barcode = wms.Barcode,
                        takesite = wms.W_S_Loc,
                        //givesite = wms.W_D_Loc
                        givesite = wms.W_S_Loc.Split('-')[0]
                    });

                    result = "";
                    return true;
                }
            }
            catch (Exception ex)
            {
                // LOG
                result = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// WMS 入库任务（分配区域）
        /// </summary>
        public bool AddWmsInTask_Area(string area, string pkl, string code)
        {
            try
            {
                if (mWmsTask.Exists(c => c.barcode == code))
                {
                    // 存在Task资讯则略过
                    return false;
                }

                // ? 请求 WMS 入库区域
                WmsModel wms = mHttp.DoBarcodeScanTask(area, code);

                AddAgvTempTask(new WmsTask()
                {
                    taskuid = wms.Task_UID,
                    tasktype = TaskTypeEnum.AGV搬运,
                    barcode = wms.Barcode,
                    takesite = wms.W_S_Loc,
                    givesite = wms.W_D_Loc
                },pkl);

                return true;
            }
            catch (Exception ex)
            {
                // LOG
                return false;
            }
        }

        /// <summary>
        /// WMS 入库任务（分配货位）
        /// </summary>
        public bool AddWmsInTask_Site(string area, string frt, string takeid)
        {
            try
            {
                if (mWmsTask.Exists(c => c.taskuid != takeid))
                {
                    // 不存在Task资讯则略过
                    return false;
                }

                // ? 请求 WMS 入库货位
                WmsModel wms = mHttp.DoReachStockinPosTask(area, takeid);

                AddFrtTempJob(new WmsTask()
                {
                    taskuid = wms.Task_UID,
                    tasktype = TaskTypeEnum.入库,
                    barcode = wms.Barcode,
                    takesite = wms.W_S_Loc,
                    givesite = wms.W_D_Loc
                }, frt);

                return true;
            }
            catch (Exception ex)
            {
                // LOG
                return false;
            }
        }

        /// <summary>
        /// 完成 WMS任务
        /// </summary>
        public static void FinishWms(string wmsid)
        {
            if (mWmsTask.Exists(c => c.taskuid == wmsid))
            {
                WmsTask wms = mWmsTask.Find(c => c.taskuid == wmsid);

                // 更新状态
                wms.UpdateStatus(WmsTaskStatus.finish);

                // ? Call WMS（通知完成）
                switch (wms.tasktype)
                {
                    case TaskTypeEnum.入库:
                        mHttp.DoStockInFinishTask(wms.givesite, wmsid);
                        break;
                    case TaskTypeEnum.出库:
                        mHttp.DoStockOutFinishTask(wms.givesite, wmsid);
                        break;
                    default:
                        return;
                }

                mWmsTask.RemoveAll(c => c.taskuid == wmsid);
            }
        }

        #endregion

        #region [ 其他 ]

        /// <summary>
        /// 设备信息转换
        /// </summary>
        public static DevType GetDevTypeEnum(string dev)
        {
            if (string.IsNullOrEmpty(dev))
            {
                return DevType.空设备;
            }

            switch (dev)
            {
                case DeviceType.AGV:
                    return DevType.AGV;

                case DeviceType.包装线:
                    return DevType.包装线;

                case DeviceType.固定辊台:
                    return DevType.固定辊台;

                case DeviceType.摆渡车:
                    return DevType.摆渡车;

                case DeviceType.运输车:
                    return DevType.运输车;

                case DeviceType.行车:
                    return DevType.行车;

                default:
                    return DevType.空设备;
            }
        }

        /// <summary>
        /// 是否执行入库作业
        /// </summary>
        public void AlterDoJobIn(bool isdo)
        {
            PublicParam.IsDoJobIn = isdo;
        }

        /// <summary>
        /// 是否执行出库作业
        /// </summary>
        public void AlterDoJobOut(bool isdo)
        {
            PublicParam.IsDoJobOut = isdo;
        }

        /// <summary>
        /// 是否执行设备任务
        /// </summary>
        public void AlterDoTask(bool isdo)
        {
            PublicParam.IsDoTask = isdo;
        }

        /// <summary>
        /// TEST
        /// </summary>
        public string GetOutTempInfo()
        {
            string rs = "";
            foreach (Job j in mOutJob)
            {
                rs += j.Tostring() + "\n";
            }
            return rs;
        }

        #endregion

    }

    #region [ 临时 ]

    /// <summary>
    /// 临时入库作业
    /// </summary>
    public class FrtTempJob
    {
        /// <summary>
        /// 入库区域
        /// </summary>
        public string area;

        /// <summary>
        /// WMS 入库任务1
        /// </summary>
        public WmsTask task1;

        /// <summary>
        /// WMS 入库任务2
        /// </summary>
        public WmsTask task2;

        /// <summary>
        /// 入库固定辊台
        /// </summary>
        public string frt;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime;

        /// <summary>
        /// 是否开始执行
        /// </summary>
        /// <returns></returns>
        public bool CanStart()
        {
            if (task1 != null && task2 != null)
            {
                return true;
            }
            if ((DateTime.Now - createtime).TotalMinutes > 30)  //单托等待时间超 30 min
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 临时出库任务
    /// </summary>
    public class AwcTempTask
    {
        /// <summary>
        /// WMS 出库任务
        /// </summary>
        public WmsTask task;

        /// <summary>
        /// 出库区域
        /// </summary>
        public string area;

        /// <summary>
        /// 出库货位 X轴值
        /// </summary>
        public int X;

        /// <summary>
        /// 出库货位 Y轴值
        /// </summary>
        public int Y;

        /// <summary>
        /// 出库货位 Z轴值
        /// </summary>
        public int Z;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime;
    }

    /// <summary>
    /// 临时搬运任务
    /// </summary>
    public class AgvTempTask
    {
        /// <summary>
        /// 包装线区域
        /// </summary>
        public string areaPK;

        /// <summary>
        /// 库存区域
        /// </summary>
        public string areaWH;

        /// <summary>
        /// WMS 入库任务
        /// </summary>
        public WmsTask task;

        /// <summary>
        /// 包装线设备名
        /// </summary>
        public string pkl;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime;

        /// <summary>
        /// 是否开始执行
        /// </summary>
        /// <returns></returns>
        public bool CanStart()
        {
            if (task != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    #endregion

}
