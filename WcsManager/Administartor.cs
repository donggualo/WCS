using System;
using System.Collections.Generic;
using System.Threading;
using Module;
using ModuleManager.WCS;
using PubResourceManager;
using WcsHttpManager;
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




        /// <summary>
        /// 固定辊台对接摆渡车点位
        /// </summary>
        /// <returns></returns>
        public static int FRTbuttARF(string frt)
        {
            try
            {
                return CommonSQL.GetArfByFrt(frt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                            Site = Enum.GetName(typeof(GoodsEnum), pkl._.GoodsStatus),
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
                            Site = Enum.GetName(typeof(GoodsEnum), frt._.GoodsStatus),
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
                            Site = string.Format(@"[ {0} ], {1}", arf._.CurrentSite.ToString(), Enum.GetName(typeof(GoodsEnum), arf._.GoodsStatus)),
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
                            Site = string.Format(@"[ {0} ], {1}", rgv._.CurrentSite.ToString(), Enum.GetName(typeof(GoodsEnum), rgv._.GoodsStatus)),
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
                            Site = string.Format(@"[ {0}-{1}-{2} ], {3}",
                                awc._.CurrentSiteX, awc._.CurrentSiteY, awc._.CurrentSiteZ, Enum.GetName(typeof(AwcGoodsEnum), awc._.GoodsStatus)),
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



        #region [ WMS & NEW ]

        public static void StopRoll()
        {
            try
            {
                mRgv.StopALLRoll();
                mArf.StopALLRoll();
                mFrt.StopALLRoll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最新货物编码
        /// </summary>
        /// <returns></returns>
        public static string GetGoodsCode(string day)
        {
            try
            {
                //day = "202008";
                string sn = CommonSQL.GetSN(day, out string box);
                // @流水号|A产品名称|B型号|C批号|D颜色|E等级|F箱号|G规格
                //return string.Format(@"@{0}|Afptc|B1200800|C{1}|Dw|Ea|F{2}|G1200800",
                //    sn, day, box);
                return string.Format(@"@20200906{0}|Afpz|B12*18|Dw|Ea2|F001|G2440*0600", sn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最新初始化入库任务目的区域
        /// </summary>
        /// <returns></returns>
        public static string GetInAreaWMS(string from, string code, out string taskid)
        {
            try
            {
                string area = CommonSQL.GetToArea(code, out taskid);
                if (string.IsNullOrEmpty(area))
                {
                    // 请求WMS任务
                    WmsModel wms = mHttp.DoBarcodeScanTask(from, code);
                    if (wms != null && !string.IsNullOrEmpty(wms.Task_UID))
                    {
                        // 写入数据库
                        CommonSQL.InsertTaskInfo(wms.Task_UID, (int)wms.Task_type, wms.Barcode, wms.W_S_Loc, wms.W_D_Loc, "");

                        area = wms.W_D_Loc;
                    }
                }
                return area;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最早待执行入库任务
        /// </summary>
        /// <returns></returns>
        public static bool GetInTaskWMS(string from, int num, out string[] taskid)
        {
            try
            {
                bool res = false;
                taskid = new string[num];
                List<WCS_WMS_TASK> task = CommonSQL.GetInTaskInfo(from);
                switch (num)
                {
                    case 1:
                        if (task != null || task.Count != 0)
                        {
                            taskid[0] = task[0].TASK_ID;
                            res = true;
                        }
                        break;
                    case 2:
                        if (task != null || task.Count == 2)
                        {
                            taskid[0] = task[0].TASK_ID;
                            taskid[1] = task[1].TASK_ID;
                            res = true;
                        }
                        break;
                    default:
                        break;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最早待执行出库任务
        /// </summary>
        /// <returns></returns>
        public static int GetOutTaskWMS(string to, out string[] taskid)
        {
            try
            {
                // 等待WMS请求秒数  3min
                int WMSs = 180;

                int res = 0;
                List<WCS_WMS_TASK> task = CommonSQL.GetOutTaskInfo(to);
                taskid = new string[task.Count];

                int s = 0;
                if (task != null && task.Count > 0)
                {
                    TimeSpan ts = DateTime.Now.Subtract(task[0].CREATION_TIME);
                    s = Convert.ToInt32(ts.TotalSeconds);
                    task.Sort(SortOutTaskWMS);
                }

                switch (task.Count)
                {
                    case 1:
                        if (s < WMSs) break;

                        taskid[0] = task[0].TASK_ID;
                        res = 1;
                        break;
                    case 2:
                        if (s < WMSs) break;

                        taskid[0] = task[0].TASK_ID;
                        taskid[1] = task[1].TASK_ID;
                        res = 2;
                        break;
                    case 3:
                        if (s < WMSs) break;

                        taskid[0] = task[0].TASK_ID;
                        taskid[1] = task[1].TASK_ID;
                        taskid[2] = task[2].TASK_ID;
                        res = 3;
                        break;
                    case 4:
                        taskid[0] = task[0].TASK_ID;
                        taskid[1] = task[1].TASK_ID;
                        taskid[2] = task[2].TASK_ID;
                        taskid[3] = task[3].TASK_ID;
                        res = 4;
                        break;
                    default:
                        break;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 出库任务排序
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        private static int SortOutTaskWMS(WCS_WMS_TASK t1, WCS_WMS_TASK t2)
        {
            // -1:t1在前； 0:同序； 1:t2在前；
            int res = 0;
            if (t1 == null && t2 == null) return 0;
            else if (t1 != null && t2 == null) return 1;
            else if (t1 == null && t2 != null) return -1;

            string[] t1_loc = t1.WMS_LOC_FROM.Split('-'); 
            string[] t2_loc = t2.WMS_LOC_FROM.Split('-'); 

            // 对比X
            if (int.Parse(t1_loc[1]) == int.Parse(t2_loc[1]))
            {
                // X 相等对比 Y
                if (int.Parse(t1_loc[2]) == int.Parse(t2_loc[2]))
                {
                    // Y 相等对比 Z
                    if (int.Parse(t1_loc[3]) >= int.Parse(t2_loc[3]))
                    {
                        res = -1;
                    }
                    else
                    {
                        res = 1;
                    }
                }
                else if (int.Parse(t1_loc[2]) > int.Parse(t2_loc[2]))
                {
                    res = -1;
                }
                else
                {
                    res = 1;
                }
            }
            else if (int.Parse(t1_loc[1]) > int.Parse(t2_loc[1]))
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
        /// WMS分配库区
        /// </summary>
        /// <returns></returns>
        public static bool AssignInArea(string area, string code)
        {
            try
            {
                bool res = false;
                WmsModel wms = null;
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(code))
                { 
                    // 请求WMS任务
                    wms = mHttp.DoBarcodeScanTask(area, code);
                    if (wms != null && !string.IsNullOrEmpty(wms.Task_UID))
                    {
                        // 写入数据库
                        CommonSQL.InsertTaskInfo(wms.Task_UID, (int)wms.Task_type, wms.Barcode, wms.W_S_Loc, wms.W_D_Loc, "");

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
        /// WMS分配库位
        /// </summary>
        /// <returns></returns>
        public static bool AssignInSite(string area, string tid)
        {
            try
            {
                bool res = false;
                WmsModel wms = null;
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(tid))
                {
                    // 请求WMS任务
                    wms = mHttp.DoReachStockinPosTask(area, tid);
                    if (wms != null && !string.IsNullOrEmpty(wms.Task_UID))
                    {
                        CommonSQL.UpdateWms(wms.Task_UID, (int)WmsTaskStatus.待执行, wms.W_S_Loc, wms.W_D_Loc);
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
