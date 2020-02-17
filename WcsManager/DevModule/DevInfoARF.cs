using Module;
using Module.DEV;
using PubResourceManager;
using ADS = WcsManager.Administartor;

namespace WcsManager.DevModule
{
    public class DevInfoARF
    {
        #region 虚拟定义

        /// <summary>
        /// 设备名
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
        /// 任务类型
        /// </summary>
        public TaskTypeEnum taskType;

        #endregion

        /// <summary>
        /// 实际数据
        /// </summary>
        public DeviceARF _;

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
        /// 摆渡车—获取当前数据
        /// </summary>
        /// <returns></returns>
        public static byte[] GetDataOrder()
        {
            //                     字头   设备号 控制码  值1   值2   值3   值4    结束符
            return new byte[] { 0x94, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
        }

        /// <summary>
        /// 定位任务
        /// </summary>
        /// <param name="site"></param>
        public void ToSite(int site)
        {
            //                             字头    设备号 控制码     值1    值2   值3   值4    结束符
            byte[] order = new byte[] { 0x94, 0x02, 0x01, 0x01, (byte)site, 0x00, 0x00, 0x00, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName,order,false);
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void StopTask()
        {
            //                             字头    设备号 控制码  值1    值2   值3   值4    结束符
            byte[] order = new byte[] { 0x94, 0x02, 0x01, 0x7F, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, false);
        }

        /// <summary>
        /// 接货任务
        /// </summary>
        /// <param name="tasktype"></param>
        /// <param name="goodsnum"></param>
        public void StartTakeRoll(TaskTypeEnum tasktype, int goodsnum)
        {
            byte roller = (byte)RollerStatusEnum.辊台全启动;
            byte direction;
            byte take = (byte)RollerTypeEnum.接货;
            byte goods = (byte)goodsnum;

            switch (tasktype)
            {
                case TaskTypeEnum.入库:
                    direction = (byte)RollerDiretionEnum.正向;
                    break;
                case TaskTypeEnum.出库:
                    direction = (byte)RollerDiretionEnum.反向;
                    break;
                default:
                    return;
            }
            //                             字头    设备号 控制码  值1      值2     值3   值4    结束符
            byte[] order = new byte[] { 0x94, 0x02, 0x01, 0x02, roller, direction, take, goods, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, true);
        }

        /// <summary>
        /// 送货任务
        /// </summary>
        /// <param name="tasktype"></param>
        /// <param name="goodsnum"></param>
        public void StartGiveRoll(TaskTypeEnum tasktype)
        {
            byte roller = (byte)RollerStatusEnum.辊台全启动;
            byte direction;
            byte take = (byte)RollerTypeEnum.送货;
            byte goodsnum = 0x02;

            switch (tasktype)
            {
                case TaskTypeEnum.入库:
                    direction = (byte)RollerDiretionEnum.正向;
                    if (_.GoodsStatus == GoodsEnum.辊台满货)
                    {
                        roller = (byte)RollerStatusEnum.辊台1启动;
                        goodsnum = 0x01;
                    }
                    break;

                case TaskTypeEnum.出库:
                    direction = (byte)RollerDiretionEnum.反向;
                    if (_.GoodsStatus == GoodsEnum.辊台满货)
                    {
                        roller = (byte)RollerStatusEnum.辊台2启动;
                        goodsnum = 0x01;
                    }
                    break;

                default:
                    return;
            }
            //                             字头    设备号 控制码  值1      值2      值3     值4      结束符
            byte[] order = new byte[] { 0x94, 0x02, 0x01, 0x02, roller, direction, take, goodsnum, 0xFF, 0xFE };
            ADS.mSocket.SendOrder(devName, order, true);
        }

        #endregion
    }

}
