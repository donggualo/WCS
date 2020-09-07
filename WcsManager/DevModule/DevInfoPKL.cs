using Module;
using Module.DEV;
using PubResourceManager;
using System;
using ADS = WcsManager.Administartor;

namespace WcsManager.DevModule
{
    public class DevInfoPKL
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
        /// 锁定号(AGV)
        /// </summary>
        public string lockID1;

        /// <summary>
        /// 二维码
        /// </summary>
        public string lockID2;

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUseful;

        #endregion

        /// <summary>
        /// 实际数据
        /// </summary>
        public DevicePKL _;

        /// <summary>
        /// 更新锁定状态
        /// </summary>
        public void IsLockUnlock(bool islock, string lockid = "")
        {
            try
            {
                CommonSQL.UpdateDevInfo(devName, lockid, islock);
                isLock = islock;
                lockID1 = lockid;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新锁定状态
        /// </summary>
        public void IsLockUnlockNew(bool islock, string lockid1 = "", string lockid2 = "")
        {
            try
            {
                CommonSQL.UpdateDevInfo(0, devName, lockid1, lockid2, islock);
                isLock = islock;
                lockID1 = lockid1;
                lockID2 = lockid2;

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


        #region 任务

        /// <summary>
        /// 包装线—获取当前数据
        /// </summary>
        /// <returns></returns>
        public static byte[] GetDataOrder()
        {
            //                     字头   设备号 控制码  值1    结束符
            return new byte[] { 0x82, 0x01, 0x01, 0x00, 0x00, 0xFF, 0xFE };
        }

        /// <summary>
        /// 送货任务
        /// </summary>
        public void StartGiveRoll()
        {
            //                             字头    设备号 控制码  值1    结束符
            byte[] order = new byte[] { 0x82, 0x01, 0x01, 0x02, 0x01, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void StopTask()
        {
            //                             字头    设备号 控制码  值1   结束符
            byte[] order = new byte[] { 0x82, 0x01, 0x01, 0x03, 0x01, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        #endregion
    }
}
