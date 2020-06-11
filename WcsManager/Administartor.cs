using System;
using System.Collections.Generic;
using Module;
using PubResourceManager;
using WcsManager.Base;
using WcsManager.DevModule;

namespace WcsManager
{
    public class Administartor : AdminBase
    {
        public Administartor() : base()
        {
        }

        #region [ 行车 ]

        /// <summary>
        /// 行车作业判断执行接货？
        /// </summary>
        public static bool JobPartAWC_Take(string taskid)
        {
            return mRgv.IsTakeButtRGV(taskid, DevType.行车);
        }

        /// <summary>
        /// 行车作业判断执行送货？
        /// </summary>
        public static bool JobPartAWC_Give(string taskid)
        {
            return mRgv.IsGiveButtRGV(taskid, DevType.行车);
        }

        /// <summary>
        /// 行车作业判断入库完成？
        /// </summary>
        public static void JobPartAWC_FinishIn(string taskid)
        {
            FinishWms(taskid);
        }

        #endregion


        #region [ 运输车 ]

        /// <summary>
        /// 运输车作业判断执行接货(摆渡车)？
        /// </summary>
        public static bool JobPartRGV_Take(string jobid)
        {
            return mArf.IsTaskConform(jobid, TaskStatus.ongivesite);
        }

        /// <summary>
        /// 运输车作业判断执行送货(摆渡车)？
        /// </summary>
        public static bool JobPartRGV_Give(string jobid)
        {
            return mArf.IsTaskConform(jobid, TaskStatus.taking);
        }

        /// <summary>
        /// 运输车作业判断接货后提前离开(行车放货)？
        /// </summary>
        public static bool JobPartRGV_TakeLeave(string area, string taskid)
        {
            return mAwc.IsGivedOrSafeDis(taskid, mDis.GetAwcGiveRgvDis(area));
        }

        /// <summary>
        /// 运输车作业判断送货后提前离开(行车取货)？
        /// </summary>
        public static bool JobPartRGV_GiveLeave(string area, string taskid)
        {
            return mAwc.IsTakedOrSafeDis(taskid, mDis.GetAwcTakeRgvDis(area));
        }

        #endregion


        #region [ 摆渡车 ]

        /// <summary>
        /// 摆渡车作业对接固定辊台坐标？
        /// </summary>
        public static int JobPartARF_Site(string jobid)
        {
            string frt = mFrt.GetFrtName(jobid);
            if (string.IsNullOrEmpty(frt))
            {
                return 0;
            }
            else
            {
                return CommonSQL.GetArfByFrt(frt);
            }
        }

        /// <summary>
        /// 摆渡车作业判断执行接货？
        /// </summary>
        public static bool JobPartARF_Take(string jobid, TaskTypeEnum tt)
        {
            bool result = false;
            switch (tt)
            {
                case TaskTypeEnum.入库:
                    // 入库接货--固定辊台是否到位
                    result = mFrt.IsTaskConform(jobid, tt, TaskStatus.ongivesite);
                    break;
                case TaskTypeEnum.出库:
                    // 出库接货--运输车是否到位
                    result = mRgv.IsTakeButtRGV(jobid, DevType.摆渡车);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 摆渡车作业判断执行送货？
        /// </summary>
        public static bool JobPartARF_Give(string jobid, TaskTypeEnum tt)
        {
            bool result = false;
            switch (tt)
            {
                case TaskTypeEnum.入库:
                    // 入库送货--运输车是否到位
                    result = mRgv.IsGiveButtRGV(jobid, DevType.摆渡车);
                    break;
                case TaskTypeEnum.出库:
                    // 出库送货--固定辊台是否到位
                    result = mFrt.IsTaskConform(jobid, tt, TaskStatus.taking);
                    break;
                default:
                    break;
            }
            return result;
        }

        #endregion


        #region [ 固定辊台 ]

        /// <summary>
        /// 固定辊台作业判断执行接货？
        /// </summary>
        public static bool JobPartFRT_Take(string jobid, DevType dev)
        {
            bool result = false;
            switch (dev)
            {
                case DevType.AGV:
                    // ? NDC 确认AGV是否到达卸货点
                    result = mNDCControl.IsUnLoadReady(Convert.ToInt32(jobid.Substring(1)));
                    break;
                case DevType.摆渡车:
                    result = mArf.IsTaskConform(jobid, TaskStatus.ongivesite);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 固定辊台作业判断执行送货？
        /// </summary>
        public static bool JobPartFRT_Give(string jobid)
        {
            // 当前方案仅入库作业存在送货任务(摆渡车)
            return mArf.IsTaskConform(jobid, TaskStatus.taking);
        }

        #endregion


        #region [ 包装线 ]

        /// <summary>
        /// 包装线作业判断执行接货？
        /// </summary>
        //public static bool JobPartPKL_Take(string jobid){}

        /// <summary>
        /// 包装线作业判断执行送货？
        /// </summary>
        public static bool JobPartPKL_Give(string jobid)
        {
            // 当前方案仅搬运作业存在送货任务(AGV)
            // ? NDC 确认AGV是否装货中
            return mNDCControl.IsLoading(Convert.ToInt32(jobid.Substring(1)));
        }

        #endregion


        #region [ 当前设备信息]

        public static List<DevItem> GetDevInfo()
        {
            try
            {
                List<DevItem> msg = new List<DevItem>();
                if (mPkl.devices != null || mPkl.devices.Count != 0)
                {
                    foreach (DevInfoPKL pkl in mPkl.devices)
                    {
                        msg.Add(new DevItem()
                        {
                            Connected = mSocket.IsConnected(pkl.devName) ? "在线" : "离线",
                            DevName = pkl.devName,
                            Site = pkl.area,
                            TaskStatus = Enum.GetName(typeof(TaskEnum), pkl._.CurrentTask)
                        });
                    }
                }
                if (mFrt.devices != null || mFrt.devices.Count != 0)
                {
                    foreach (DevInfoFRT frt in mFrt.devices)
                    {
                        msg.Add(new DevItem()
                        {
                            Connected = mSocket.IsConnected(frt.devName) ? "在线" : "离线",
                            DevName = frt.devName,
                            Site = frt.area,
                            TaskStatus = Enum.GetName(typeof(TaskEnum), frt._.CurrentTask)
                        });
                    }
                }
                if (mArf.devices != null || mArf.devices.Count != 0)
                {
                    foreach (DevInfoARF arf in mArf.devices)
                    {
                        msg.Add(new DevItem()
                        {
                            Connected = mSocket.IsConnected(arf.devName) ? "在线" : "离线",
                            DevName = arf.devName,
                            Site = arf._.CurrentSite.ToString(),
                            TaskStatus = Enum.GetName(typeof(TaskEnum), arf._.CurrentTask)
                        });
                    }
                }
                if (mRgv.devices != null || mRgv.devices.Count != 0)
                {
                    foreach (DevInfoRGV rgv in mRgv.devices)
                    {
                        msg.Add(new DevItem()
                        {
                            Connected = mSocket.IsConnected(rgv.devName) ? "在线" : "离线",
                            DevName = rgv.devName,
                            Site = rgv._.CurrentSite.ToString(),
                            TaskStatus = Enum.GetName(typeof(TaskEnum), rgv._.CurrentTask)
                        });
                    }
                }
                if (mAwc.devices != null || mAwc.devices.Count != 0)
                {
                    foreach (DevInfoAWC awc in mAwc.devices)
                    {
                        msg.Add(new DevItem()
                        {
                            Connected = mSocket.IsConnected(awc.devName) ? "在线" : "离线",
                            DevName = awc.devName,
                            Site = string.Format(@"{0}-{1}-{2}",
                                awc._.CurrentSiteX, awc._.CurrentSiteY, awc._.CurrentSiteZ),
                            TaskStatus = Enum.GetName(typeof(TaskEnum), awc._.CurrentTask)
                        });
                    }
                }
                return msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<DevError> GetDevError()
        {
            try
            {
                List<DevError> msg = new List<DevError>();
                return msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}

public class DevItem
{
    public string Connected { set; get; }
    public string DevName { set; get; }
    public string Site { set; get; }
    public string TaskStatus { set; get; }
}

public class DevError
{
    public string DevName { set; get; }
    public string Error { set; get; }
    public string Method { set; get; }
}
