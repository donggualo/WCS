using Module;
using Module.DEV;
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
        /// 设备参考
        /// </summary>
        public DevFlag flag;

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
                isLock = islock;
                lockID = lockid;

                CommonSQL.UpdateDevInfo(devName, lockid, islock);
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
                isUseful = isuseful;

                CommonSQL.UpdateDevInfo(devName, isUseful);
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
