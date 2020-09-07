
namespace Module
{
    #region 任务信息

    /// <summary>
    /// 任务类型
    /// </summary>
    public enum TaskTypeEnum
    {
        无,
        入库,
        出库
    }

    /// <summary>
    /// 设备任务状态
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        init,
        /// <summary>
        /// 前往接货点
        /// </summary>
        totakesite,
        /// <summary>
        /// 抵达接货点
        /// </summary>
        ontakesite,
        /// <summary>
        /// 接货中
        /// </summary>
        taking,
        /// <summary>
        /// 接货完成
        /// </summary>
        taked,
        /// <summary>
        /// 前往送货点
        /// </summary>
        togivesite,
        /// <summary>
        /// 抵达送货点
        /// </summary>
        ongivesite,
        /// <summary>
        /// 送货中
        /// </summary>
        giving,
        /// <summary>
        /// 送货完成
        /// </summary>
        gived,
        /// <summary>
        /// 完成任务
        /// </summary>
        finish
    }

    #endregion

    #region 设备信息

    #region [ 实际反馈 ]

    /// <summary>
    /// 运行状态
    /// </summary>
    public enum ActionEnum
    {
        运行中 = 1,
        停止 = 0
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceEnum
    {
        设备故障 = 1,
        设备正常 = 0
    }

    /// <summary>
    /// 命令状态
    /// </summary>
    public enum CommandEnum
    {
        命令错误 = 1,
        命令正常 = 0
    }

    /// <summary>
    /// 任务
    /// </summary>
    public enum TaskEnum
    {
        定位任务 = 1,
        辊台任务 = 2,
        停止辊台任务 = 3
    }

    /// <summary>
    /// 行车任务
    /// </summary>
    public enum AwcTaskEnum
    {
        定位任务 = 1,
        取货任务 = 2,
        放货任务 = 3,
        复位任务 = 4
    }

    /// <summary>
    /// 辊台状态
    /// </summary>
    public enum RollerStatusEnum
    {
        辊台停止 = 0,
        辊台1启动 = 1,
        辊台2启动 = 2,
        辊台全启动 = 3
    }

    /// <summary>
    /// 辊台方向
    /// </summary>
    public enum RollerDiretionEnum
    {
        正向 = 1,
        反向 = 2
    }

    /// <summary>
    /// 辊台接送类型
    /// </summary>
    public enum RollerTypeEnum
    {
        接货 = 1,
        送货 = 2
    }

    /// <summary>
    /// 货物状态
    /// </summary>
    public enum GoodsEnum
    {
        辊台无货 = 0,
        辊台1有货 = 1,
        辊台2有货 = 2,
        辊台满货 = 3,
        辊台中间有货 = 4
    }

    /// <summary>
    /// 行车货物状态
    /// </summary>
    public enum AwcGoodsEnum
    {
        无货 = 0,
        有货 = 1
    }

    /// <summary>
    /// 故障信息
    /// </summary>
    public enum ErrorMessage
    {
        行车大车1故障 = 1,
        行车大车2故障 = 2,
        行车小车故障 = 3,
        行车起升故障 = 4,
        行车大车定位异常 = 5,
        行车小车定位异常 = 6,
        行车起升定位异常 = 7,
        行车电缸推出异常 = 8,
        行车电缸收回异常 = 9,
        空货异常 = 10,       // 取货到位后无货物存在
        货位异常 = 11,       // 放货坐标处有货物存在
        行车松绳异常 = 12,
    }

    #endregion

    #region [ 虚拟使用 ]

    /// <summary>
    /// 设备参考信息
    /// </summary>
    public enum DevFlag
    {
        不参考,
        靠近入库口,
        远离入库口
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public enum DevType
    {
        空设备,
        AGV,
        包装线辊台 = 33282, //0x82,0x02
        行车 = 37122,       //0x91,0x02
        固定辊台 = 37634,   //0x93,0x02
        摆渡车 = 37890,     //0x94,0x02
        运输车 = 38402      //0x96,0x02
    }

    #endregion

    #endregion

    public class BaseMaster
    {

    }
}
