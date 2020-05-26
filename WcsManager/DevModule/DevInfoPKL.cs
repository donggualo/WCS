﻿using Module;
using Module.DEV;
using PubResourceManager;

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
        /// 锁定号
        /// </summary>
        public string lockID;

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
            isLock = islock;
            lockID = lockid;

            CommonSQL.UpdateDevInfo(devName, lockid, islock);
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
            ADS.mSocket.SendOrder(devName, order, true);
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
