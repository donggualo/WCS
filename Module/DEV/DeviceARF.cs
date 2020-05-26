namespace Module.DEV
{
    /// <summary>
    /// 自动有轨摆渡车 Automatic Railway Ferry
    /// </summary>
    public class DeviceARF : IBaseModule
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
        public TaskEnum CurrentTask;

        /// <summary>
        /// 当前坐标
        /// </summary>
        public int CurrentSite;

        /// <summary>
        /// 辊台状态
        /// </summary>
        public RollerStatusEnum RollerStatus;

        /// <summary>
        /// 辊台方向
        /// </summary>
        public RollerDiretionEnum RollerDiretion;

        /// <summary>
        /// 完成任务
        /// </summary>
        public TaskEnum FinishTask;

        /// <summary>
        /// 货物状态
        /// </summary>
        public GoodsEnum GoodsStatus;

        /// <summary>
        /// 故障信息
        /// </summary>
        public int ErrorMessage;

    }
}