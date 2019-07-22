using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_phase1.NDC
{
    /// <summary>
    /// NDC任务状态
    /// </summary>
    public enum NDCStatus
    {
        /// <summary>
        /// 任务确认
        /// </summary>
        OrderAcknowledge = 1,

        /// <summary>
        /// 任务完成
        /// </summary>
        OrderFinished = 3,

        /// <summary>
        /// 取消任务
        /// </summary>
        CancelAcknowledge = 25,

        /// <summary>
        /// 参数接受
        /// </summary>
        ParameterAccepted = 19,

        /// <summary>
        /// 参数拒绝
        /// </summary>
        ParameterNotAccepted = 20,

        /// <summary>
        /// 致命错误
        /// </summary>
        FatalError = 17,

        /// <summary>
        /// 未激活序列号
        /// </summary>
        NoActiveIndex = 14,

        /// <summary>
        /// 优先级错误
        /// </summary>
        PriorityError = 9,

        /// <summary>
        /// 无效结构
        /// </summary>
        InvalidStructure = 10,

        /// <summary>
        /// 小车ID错误
        /// </summary>
        CarrierNumberError = 22,

        /// <summary>
        /// 参数确认
        /// </summary>
        ParameterAcknowledge = 7,

        /// <summary>
        /// 参数数字太大
        /// </summary>
        ParameterNumberTooHighForThis = 15,

        /// <summary>
        /// 优先级修改确定
        /// </summary>
        ChangeOrderInstancePriorityAcknowledge = 35,

        /// <summary>
        /// 小车已分配
        /// </summary>
        CarrierAllocated = 2,

        /// <summary>
        /// 小车已连接
        /// </summary>
        CarrierConnected = 37,

        /// <summary>
        /// 任务取消
        /// </summary>
        OrderCancelled = 4,

        /// <summary>
        /// 参数删除
        /// </summary>
        ParameterDeleted = 18,

        /// <summary>
        /// 无效任务号
        /// </summary>
        InvalidIndexNumber = 24,

        /// <summary>
        /// Ikey已被占用
        /// </summary>
        IKEYInUse = 27,
    }
}
