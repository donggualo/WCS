namespace Module.DEV
{
    public class DeviceAWC : IBaseModule
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        public ActionEnum ActionStatus;

        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceEnum DeviceStatus;

        /// <summary>
        /// 命令状态
        /// </summary>
        public CommandEnum CommandStatus;

        /// <summary>
        /// 当前任务
        /// </summary>
        public AwcTaskEnum CurrentTask;

        /// <summary>
        /// 当前X轴坐标
        /// </summary>
        public int CurrentSiteX;

        /// <summary>
        /// 当前Y轴坐标
        /// </summary>
        public int CurrentSiteY;

        /// <summary>
        /// 当前Z轴坐标
        /// </summary>
        public int CurrentSiteZ;

        /// <summary>
        /// 完成任务
        /// </summary>
        public AwcTaskEnum FinishTask;

        /// <summary>
        /// 货物状态
        /// </summary>
        public AwcGoodsEnum GoodsStatus;

        /// <summary>
        /// 故障信息
        /// </summary>
        public int ErrorMessage;

    }
}
