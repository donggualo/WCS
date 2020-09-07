using Module;
using Module.DEV;
using ModuleManager.WCS;
using PubResourceManager;
using System;
using ADS = WcsManager.Administartor;

namespace WcsManager.DevModule
{
    public class DevInfoAWC
    {
        #region 虚拟定义

        /// <summary>
        /// 设备号
        /// </summary>
        public string devName;

        /// <summary>
        /// 所属区域
        /// </summary>
        public string area;

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool isLock;

        /// <summary>
        /// 锁定号
        /// </summary>
        public string lockID;

        /// <summary>
        /// 锁定目标
        /// </summary>
        public string lockLocWMS;

        /// <summary>
        /// 锁定运输车坐标1
        /// </summary>
        public int lockLocRGV1;

        /// <summary>
        /// 锁定运输车坐标2
        /// </summary>
        public int lockLocRGV2;

        /// <summary>
        /// 设备参考
        /// </summary>
        public DevFlag flag;

        /// <summary>
        /// 任务类型
        /// </summary>
        public TaskTypeEnum taskType;

        /// <summary>
        /// 当前取货点X
        /// </summary>
        public int TakeSiteX;
        /// <summary>
        /// 当前取货点Y
        /// </summary>
        public int TakeSiteY;
        /// <summary>
        /// 当前取货点Z
        /// </summary>
        public int TakeSiteZ;

        /// <summary>
        /// 当前放货点X
        /// </summary>
        public int GiveSiteX;
        /// <summary>
        /// 当前放货点Y
        /// </summary>
        public int GiveSiteY;
        /// <summary>
        /// 当前放货点Z
        /// </summary>
        public int GiveSiteZ;

        /// <summary>
        /// X轴差距
        /// </summary>
        public int gapX;

        /// <summary>
        /// Y轴差距
        /// </summary>
        public int gapY;

        /// <summary>
        /// Z轴差距
        /// </summary>
        public int gapZ;

        /// <summary>
        /// X轴误差范围
        /// </summary>
        public int limitX;

        /// <summary>
        /// Y轴误差范围
        /// </summary>
        public int limitY;

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUseful;

        #endregion

        /// <summary>
        /// 实际数据
        /// </summary>
        public DeviceAWC _;

        /// <summary>
        /// 更新锁定状态
        /// </summary>
        public void IsLockUnlock(bool islock, string lockid = "")
        {
            try
            {
                CommonSQL.UpdateDevInfo(devName, lockid, islock);
                isLock = islock;
                lockID = lockid;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新锁定状态
        /// </summary>
        public void IsLockUnlockNew(TaskTypeEnum tt, bool islock, string lockid = "")
        {
            try
            {
                CommonSQL.UpdateDevInfo((int)tt, devName, lockid, "", islock);
                taskType = tt;
                isLock = islock;
                lockID = lockid;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新使用状态
        /// </summary>
        public void UpdateUseufl(bool isuseful)
        {
            try
            {
                CommonSQL.UpdateDevInfo(devName, isuseful);
                ADS.mSocket.UpdateUserful(devName, isuseful);
                isUseful = isuseful;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 坐标是否处理
        /// </summary>
        /// <returns></returns>
        public bool IsOkLoc(bool isDel)
        {
            try
            {
                bool res = false;
                if (isDel)
                {
                    // 清坐标
                    lockLocWMS = "";

                    lockLocRGV1 = 0;
                    lockLocRGV2 = 0;

                    TakeSiteX = 0;
                    TakeSiteY = 0;
                    TakeSiteZ = 0;

                    GiveSiteX = 0;
                    GiveSiteY = 0;
                    GiveSiteZ = 0;

                    res = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(lockID))
                    {
                        // 清坐标
                        lockLocWMS = "";

                        lockLocRGV1 = 0;
                        lockLocRGV2 = 0;

                        TakeSiteX = 0;
                        TakeSiteY = 0;
                        TakeSiteZ = 0;

                        GiveSiteX = 0;
                        GiveSiteY = 0;
                        GiveSiteZ = 0;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(lockLocWMS) ||
                            lockLocRGV1 == 0 || lockLocRGV2 == 0 ||
                            TakeSiteX == 0 || TakeSiteY == 0 || TakeSiteZ == 0 ||
                            GiveSiteX == 0 || GiveSiteY == 0 || GiveSiteZ == 0)
                        {
                            WCS_CONFIG_LOC loc = CommonSQL.GetWcsLocByTask(lockID);
                            if (loc == null || string.IsNullOrEmpty(loc.WMS_LOC) ||
                                string.IsNullOrEmpty(loc.RGV_LOC_1) || string.IsNullOrEmpty(loc.RGV_LOC_2) ||
                                string.IsNullOrEmpty(loc.AWC_LOC_TRACK) || string.IsNullOrEmpty(loc.AWC_LOC_STOCK))
                            {
                                throw new Exception("无对应作业【" + lockID + "】坐标！");
                            }

                            lockLocWMS = loc.WMS_LOC;
                            lockLocRGV1 = int.Parse(loc.RGV_LOC_1);
                            lockLocRGV2 = int.Parse(loc.RGV_LOC_2);
                            string t = "";
                            string g = "";
                            switch (taskType)
                            {
                                case TaskTypeEnum.入库:
                                    t = loc.AWC_LOC_TRACK;
                                    g = loc.AWC_LOC_STOCK;
                                    break;
                                case TaskTypeEnum.出库:
                                    t = loc.AWC_LOC_STOCK;
                                    g = loc.AWC_LOC_TRACK;
                                    break;
                                default:
                                    break;
                            }
                            if (!string.IsNullOrEmpty(t))
                            {
                                string[] ts = t.Split('-');
                                TakeSiteX = int.Parse(ts[0]) + gapX;
                                TakeSiteY = int.Parse(ts[1]) + gapY;
                                TakeSiteZ = int.Parse(ts[2]) + gapZ;
                            }
                            if (!string.IsNullOrEmpty(g))
                            {
                                string[] gs = g.Split('-');
                                GiveSiteX = int.Parse(gs[0]) + gapX;
                                GiveSiteY = int.Parse(gs[1]) + gapY;
                                GiveSiteZ = int.Parse(gs[2]) + gapZ;
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

        #region 任务

        /// <summary>
        /// 行车—获取当前数据
        /// </summary>
        /// <returns></returns>
        public static byte[] GetDataOrder()
        {
            //                     字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            return new byte[] { 0x90, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
        }

        /// <summary>
        /// 定位任务
        /// </summary>
        public void ToSite(int siteX, int siteY)
        {
            byte[] x = BitConverter.GetBytes(siteX);
            byte[] y = BitConverter.GetBytes(siteY);
            //                             字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            byte[] order = new byte[] { 0x90, 0x02, 0x01, 0x01, x[2], x[1], x[0], y[1], y[0], 0x00, 0x00, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 取货任务
        /// </summary>
        public void StartTake(int siteZ)
        {
            byte[] z = BitConverter.GetBytes(siteZ);
            //                             字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            byte[] order = new byte[] { 0x90, 0x02, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, z[1], z[0], 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 放货任务
        /// </summary>
        public void StartGive(int siteZ)
        {
            byte[] z = BitConverter.GetBytes(siteZ);
            //                             字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            byte[] order = new byte[] { 0x90, 0x02, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, z[1], z[0], 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 复位任务
        /// </summary>
        /// <param name="ABCNum">行车号</param>
        /// <returns></returns>
        public void ResetTask()
        {
            //                             字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            byte[] order = new byte[] { 0x90, 0x02, 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        public void StopTask()
        {
            //                             字头   设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            byte[] order = new byte[] { 0x90, 0x02, 0x01, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        #endregion
    }

}
