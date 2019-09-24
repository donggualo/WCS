using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Threading;
using ToolManager;
using TaskManager.Devices;
using ModuleManager.WCS;
using PubResourceManager;

namespace TaskManager
{
    #region 设备指令任务

    public abstract class TaskInterface
    {
        public abstract void DoWork();
    }
    /// <summary>
    /// 指令任务
    /// </summary>
    public class Task : TaskInterface
    {
        WCS_TASK_ITEM _ITEM = null;
        private string _deviceType = null;
        private byte[] _order = null;
        private bool _isSuc = false;
        private bool _isErr = false;

        public WCS_TASK_ITEM ITEM
        {
            get { return _ITEM; }
        }
        public string Id
        {
            get { return _ITEM.ID.ToString(); }
        }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string Devicetype
        {
            get { return _deviceType; }
        }
        /// <summary>
        /// 设备指令
        /// </summary>
        public byte[] Order
        {
            get { return _order; }
        }
        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsSuc
        {
            get { return _isSuc; }
        }
        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsErr
        {
            get { return _isErr; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="item"></param>
        /// <param name="deviceType"></param>
        /// <param name="order"></param>
        public Task(WCS_TASK_ITEM item, string deviceType, byte[] order)
        {
            _ITEM = item;
            _deviceType = deviceType;
            _order = order;
        }

        /// <summary>
        /// 定位任务等待对接
        /// </summary>
        public void ISetTaskWait()
        {
            if (_ITEM.ID.ToString() != null)
            {
                // 更新数据库资讯
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, _ITEM.ITEM_ID, ItemColumnName.作业状态, ItemStatus.交接中);
                // 任务完成
                _isSuc = true;
            }
        }
        /// <summary>
        /// 任务完成
        /// </summary>
        public void ISetTaskSuc()
        {
            if (_ITEM.ID.ToString() != null)
            {
                // 更新数据库资讯
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, _ITEM.ITEM_ID, ItemColumnName.作业状态, ItemStatus.完成任务);
                // 任务完成
                _isSuc = true;
                DataControl._mSocket.SwithRefresh(_ITEM.DEVICE, true);
            }
        }
        /// <summary>
        /// 任务异常
        /// </summary>
        public void ISetTaskErr()
        {
            if (_ITEM.ID.ToString() != null)
            {
                // 更新数据库资讯
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, _ITEM.ITEM_ID, ItemColumnName.作业状态, ItemStatus.出现异常);
                // 任务完成
                _isErr = true;
            }
        }

        /// <summary>
        /// 业务处理
        /// </summary>
        public override void DoWork()
        {
            // 各设备重写执行逻辑
        }

    }

    /// <summary>
    /// 与 AGV 对接的固定辊台任务
    /// </summary>
    public class AGVFRTTack : Task
    {
        FRT _device;
        Log log;
        public AGVFRTTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new FRT(ITEM.DEVICE);
            log = new Log("AGV_FRT-" + ITEM.DEVICE + "-");
            // 记录生成指令LOG
            log.LOG(String.Format(@"【CreatOrder】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, DataControl._mStools.BytetToString(order)));
        }

        public override void DoWork()
        {
            try
            {
                // 异常
                if (_device.DeviceStatus() == FRT.DeviceError || _device.CommandStatus() == FRT.CommandError)
                {
                    ISetTaskErr();
                    // LOG
                    log.LOG(String.Format(@"【Error】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 异常信息[ {4} ].",
                    ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, "设备故障或命令错误"));
                    return;
                }

                #region 调试
                if (PublicParam.IsIgnoreFRT) //add调试判断
                {
                    if (_device.ActionStatus() == FRT.Stop)
                    {
                        // 发送指令
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        // LOG
                        log.LOG(String.Format(@"【SendOrder】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                    }
                    else
                    {
                        Thread.Sleep(5000);
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(String.Format(@"【Success】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                    }
                    return;
                }
                #endregion

                // 对接设备状态
                if (!string.IsNullOrEmpty(ITEM.LOC_TO.Trim())) // 目标不为空即最终无货
                {
                    // 固定辊台无货物
                    if (_device.GoodsStatus() == FRT.GoodsNoAll && _device.ActionStatus() == FRT.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(String.Format(@"【Success】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                        return;
                    }
                }
                else
                {
                    // 固定辊台上有2#有货 或者 都有货
                    if ((_device.GoodsStatus() == FRT.GoodsYes2 || _device.GoodsStatus() == FRT.GoodsYesAll) && _device.ActionStatus() == FRT.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(String.Format(@"【Success】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                        return;
                    }
                }
                // 发送指令
                if (_device.ActionStatus() == FRT.Stop)
                {
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                    {
                        throw new Exception(result);
                    }
                    // LOG
                    log.LOG(String.Format(@"【SendOrder】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                    ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(String.Format(@"【Error】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 异常信息[ {4} ].",
                ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, ex.ToString()));
            }
        }
    }

    /// <summary>
    /// 与 ARF 对接的固定辊台任务
    /// </summary>
    public class FRTTack : Task
    {
        FRT _device;
        Log log;
        public FRTTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new FRT(ITEM.DEVICE);
            log = new Log("Task_FRT-" + ITEM.DEVICE + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));
        }

        public override void DoWork()
        {
            try
            {
                // 异常
                if (_device.DeviceStatus() == FRT.DeviceError || _device.CommandStatus() == FRT.CommandError)
                {
                    ISetTaskErr();
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, "设备故障或命令错误."));
                    return;
                }

                #region 调试
                if (PublicParam.IsIgnoreFRT) //add调试判断
                {
                    if (_device.ActionStatus() == FRT.Stop)
                    {
                        // 发送指令
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                    else
                    {
                        Thread.Sleep(5000);
                        // 完成任务
                        ISetTaskSuc();
                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                    }
                    return;
                }
                #endregion

                ARF _arf = new ARF(ITEM.LOC_TO);
                // 对接设备状态
                if (!string.IsNullOrEmpty(ITEM.LOC_TO.Trim())) // 目标不为空即最终无货
                {
                    // 摆渡车辊台停止状态
                    if (_arf.CurrentStatus() == ARF.RollerStop)
                    {
                        // 摆渡车辊台上无货物
                        if (_arf.GoodsStatus() == ARF.GoodsNoAll)
                        {
                            return;
                        }
                    }
                    // 固定辊台无货物
                    if (_device.GoodsStatus() == FRT.GoodsNoAll && _device.ActionStatus() == FRT.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                    else if (_device.GoodsStatus() == FRT.GoodsYesAll && _device.ActionStatus() == FRT.Stop &&
                             _arf.GoodsStatus() == ARF.GoodsYesAll && _arf.ActionStatus() == ARF.Stop)
                    {
                        return; // 固定辊台与摆渡车都有货，不启动辊台
                    }
                }
                else
                {
                    // 摆渡车辊台上无货物,固定辊台上有货物
                    if (_arf.GoodsStatus() == ARF.GoodsNoAll && _device.GoodsStatus() != FRT.GoodsNoAll && _device.ActionStatus() == FRT.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                    else if (_device.GoodsStatus() == FRT.GoodsYesAll && _device.ActionStatus() == FRT.Stop &&
                             _arf.GoodsStatus() == ARF.GoodsYesAll && _arf.ActionStatus() == ARF.Stop)
                    {
                        return; // 固定辊台与摆渡车都有货，不启动辊台
                    }
                }
                // 发送指令
                if (_device.ActionStatus() == FRT.Stop)
                {
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                    {
                        throw new Exception(result);
                    }
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.ToString()));
            }
        }
    }

    /// <summary>
    /// 摆渡车任务
    /// </summary>
    public class ARFTack : Task
    {
        ARF _device;
        Log log;
        public ARFTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new ARF(ITEM.DEVICE);
            log = new Log("Task_ARF-" + ITEM.DEVICE + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));
        }

        public override void DoWork()
        {
            try
            {
                // 异常
                if (_device.DeviceStatus() == ARF.DeviceError || _device.CommandStatus() == ARF.CommandError)
                {
                    ISetTaskErr();
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, "设备故障或命令错误."));
                    return;
                }
                // 对接任务
                if (ITEM.ITEM_ID.Substring(0, 2) == "11")
                {
                    #region 调试
                    if (PublicParam.IsIgnoreARF) //add调试判断
                    {
                        if (_device.ActionStatus() == ARF.Stop)
                        {
                            // 发送指令
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                throw new Exception(result);
                            }
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                        }
                        else
                        {
                            Thread.Sleep(5000);
                            // 完成任务
                            ISetTaskSuc();
                            // 解锁设备数据状态
                            DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        }
                        return;
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(ITEM.LOC_TO.Trim())) // 目标不为空即最终无货
                    {
                        // 获取目标设备类型
                        String typeTo = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_TO);
                        // 摆渡车对接 固定辊台/运输车
                        switch (typeTo)
                        {
                            case DeviceType.固定辊台:
                                FRT _frt = new FRT(ITEM.LOC_TO);
                                // 固定辊台停止状态
                                if (_frt.CurrentStatus() == FRT.RollerStop)
                                {
                                    // 固定辊台上无货物
                                    if (_frt.GoodsStatus() == FRT.GoodsNoAll)
                                    {
                                        return;
                                    }
                                }
                                else if (_device.GoodsStatus() == ARF.GoodsYesAll && _device.ActionStatus() == ARF.Stop &&
                                         _frt.GoodsStatus() == FRT.GoodsYesAll && _frt.ActionStatus() == FRT.Stop)
                                {
                                    return; // 摆渡车与摆渡车都有货，不启动辊台
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_TO);
                                // 运输车辊台停止状态
                                if (_rgv.CurrentStatus() == RGV.RollerStop)
                                {
                                    // 运输车辊台上无货物
                                    if (_rgv.GoodsStatus() == RGV.GoodsNoAll)
                                    {
                                        return;
                                    }
                                }
                                else if (_device.GoodsStatus() == ARF.GoodsYesAll && _device.ActionStatus() == ARF.Stop &&
                                         _rgv.GoodsStatus() == RGV.GoodsYesAll && _rgv.ActionStatus() == RGV.Stop)
                                {
                                    return; // 摆渡车与运输车都有货，不启动辊台
                                }
                                break;
                            default:
                                break;
                        }
                        // 摆渡车无货物
                        if (_device.GoodsStatus() == ARF.GoodsNoAll && _device.ActionStatus() == ARF.Stop)
                        {
                            // 完成任务
                            ISetTaskSuc();
                            // 解锁设备数据状态
                            DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                            return;
                        }
                    }
                    else
                    {
                        // 获取目标设备类型
                        String typeFrom = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_FROM);
                        // 摆渡车对接 固定辊台/运输车
                        switch (typeFrom)
                        {
                            case DeviceType.固定辊台:
                                FRT _frt = new FRT(ITEM.LOC_FROM);
                                // 固定辊台上无货物,摆渡车辊台上有货物
                                if (_frt.GoodsStatus() == FRT.GoodsNoAll && _device.GoodsStatus() != ARF.GoodsNoAll && _device.ActionStatus() == ARF.Stop)
                                {
                                    // 完成任务
                                    ISetTaskSuc();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                else if (_device.GoodsStatus() == ARF.GoodsYesAll && _device.ActionStatus() == ARF.Stop &&
                                         _frt.GoodsStatus() == FRT.GoodsYesAll && _frt.ActionStatus() == FRT.Stop)
                                {
                                    return; // 摆渡车与摆渡车都有货，不启动辊台
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_FROM);
                                // 运输车辊台上无货物,摆渡车辊台上有货物
                                if (_rgv.GoodsStatus() == FRT.GoodsNoAll && _device.GoodsStatus() != ARF.GoodsNoAll)
                                {
                                    // 完成任务
                                    ISetTaskSuc();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                else if (_device.GoodsStatus() == ARF.GoodsYesAll && _device.ActionStatus() == ARF.Stop &&
                                         _rgv.GoodsStatus() == RGV.GoodsYesAll && _rgv.ActionStatus() == RGV.Stop)
                                {
                                    return; // 摆渡车与运输车都有货，不启动辊台
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    // 发送指令
                    if (_device.ActionStatus() == ARF.Stop)
                    {
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
                // 定位任务
                else
                {
                    // 发送指令
                    if (_device.ActionStatus() == ARF.Stop)
                    {
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                    // 当前位置与目的位置一致 视为任务完成
                    if (_device.CurrentSite() == Convert.ToInt32(ITEM.LOC_TO) && _device.ActionStatus() == ARF.Stop)
                    {
                        // 等待对接
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.ToString()));
            }
        }
    }

    /// <summary>
    /// 运输车任务
    /// </summary>
    public class RGVTack : Task
    {
        RGV _device;
        Log log;
        public RGVTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new RGV(ITEM.DEVICE);
            log = new Log("Task_RGV-" + ITEM.DEVICE + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));
        }

        public override void DoWork()
        {
            try
            {
                // 异常
                if (_device.DeviceStatus() == RGV.DeviceError || _device.CommandStatus() == RGV.CommandError)
                {
                    ISetTaskErr();
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, "设备故障或命令错误."));
                    return;
                }
                // 对接任务
                if (ITEM.ITEM_ID.Substring(0, 2) == "11")
                {
                    #region 调试
                    if (PublicParam.IsIgnoreRGV) //add调试判断
                    {
                        if (_device.ActionStatus() == RGV.Stop)
                        {
                            // 发送指令
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                throw new Exception(result);
                            }
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                        }
                        else
                        {
                            Thread.Sleep(5000);
                            // 完成任务
                            ISetTaskSuc();
                            // 解锁设备数据状态
                            DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        }
                        return;
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(ITEM.LOC_TO.Trim())) // 目标不为空即最终无货
                    {
                        // 获取目标设备类型
                        String typeTo = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_TO);
                        // 运输车对接 摆渡车/运输车
                        switch (typeTo)
                        {
                            case DeviceType.摆渡车:
                                ARF _arf = new ARF(ITEM.LOC_TO);
                                // 摆渡车辊台停止状态
                                if (_arf.CurrentStatus() == ARF.RollerStop)
                                {
                                    // 摆渡车辊台上无货物
                                    if (_arf.GoodsStatus() == ARF.GoodsNoAll)
                                    {
                                        return;
                                    }
                                }
                                else if (_device.GoodsStatus() == RGV.GoodsYesAll && _device.ActionStatus() == RGV.Stop &&
                                         _arf.GoodsStatus() == ARF.GoodsYesAll && _arf.ActionStatus() == ARF.Stop)
                                {
                                    return; // 摆渡车与运输车都有货，不启动辊台
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_TO);
                                // 目的运输车辊台停止状态
                                if (_rgv.CurrentStatus() == RGV.RollerStop)
                                {
                                    // 目的运输车辊台上无货物
                                    if (_rgv.GoodsStatus() == RGV.GoodsNoAll)
                                    {
                                        return;
                                    }
                                }
                                else if (_device.GoodsStatus() == RGV.GoodsYesAll && _device.ActionStatus() == RGV.Stop &&
                                         _rgv.GoodsStatus() == RGV.GoodsYesAll && _rgv.ActionStatus() == RGV.Stop)
                                {
                                    return; // 运输车与运输车都有货，不启动辊台
                                }
                                break;
                            default:
                                break;
                        }
                        // 运输车无货物
                        if (_device.GoodsStatus() == RGV.GoodsNoAll)
                        {
                            // 完成任务
                            ISetTaskSuc();
                            // 解锁设备数据状态
                            DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                            return;
                        }
                    }
                    else
                    {
                        // 获取目标设备类型
                        String typeFrom = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_FROM);
                        // 运输车对接 摆渡车/运输车
                        switch (typeFrom)
                        {
                            case DeviceType.摆渡车:
                                ARF _arf = new ARF(ITEM.LOC_FROM);
                                // 摆渡车辊台上无货物,运输车辊台上有货物
                                if (_arf.GoodsStatus() == ARF.GoodsNoAll && _device.GoodsStatus() != RGV.GoodsNoAll && _device.ActionStatus() == RGV.Stop)
                                {
                                    // 完成任务
                                    ISetTaskSuc();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                else if (_device.GoodsStatus() == RGV.GoodsYesAll && _device.ActionStatus() == RGV.Stop &&
                                         _arf.GoodsStatus() == ARF.GoodsYesAll && _arf.ActionStatus() == ARF.Stop)
                                {
                                    return; // 摆渡车与运输车都有货，不启动辊台
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_FROM);
                                // 来源运输车辊台上无货物,运输车辊台上有货物
                                if (_rgv.GoodsStatus() == FRT.GoodsNoAll && _device.GoodsStatus() != ARF.GoodsNoAll && _device.ActionStatus() == RGV.Stop)
                                {
                                    // 完成任务
                                    ISetTaskSuc();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                else if (_device.GoodsStatus() == RGV.GoodsYesAll && _device.ActionStatus() == RGV.Stop &&
                                         _rgv.GoodsStatus() == RGV.GoodsYesAll && _rgv.ActionStatus() == RGV.Stop)
                                {
                                    return; // 运输车与运输车都有货，不启动辊台
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    // 发送指令
                    if (_device.ActionStatus() == RGV.Stop)
                    {
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
                // 定位任务
                else
                {
                    // 发送指令
                    if (_device.ActionStatus() == RGV.Stop)
                    {
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                    // 当前位置与目的位置一致 视为任务完成
                    if (_device.GetCurrentSite() == Convert.ToInt32(ITEM.LOC_TO) && _device.ActionStatus() == RGV.Stop)
                    {
                        // 等待对接
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.ToString()));
            }
        }
    }

    /// <summary>
    /// 行车任务
    /// </summary>
    public class ABCTack : Task
    {
        ABC _device;
        Log log;
        public ABCTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new ABC(ITEM.DEVICE);
            log = new Log("Task_ABC-" + ITEM.DEVICE + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));
        }

        public override void DoWork()
        {
            try
            {
                // 异常
                if (_device.DeviceStatus() == ABC.DeviceError || _device.CommandStatus() == ABC.CommandError)
                {
                    ISetTaskErr();
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, "设备故障或命令错误."));
                    return;
                }

                #region 调试
                if (PublicParam.IsIgnoreABC) //add调试判断
                {
                    if (_device.ActionStatus() == ABC.Stop)
                    {
                        // 发送指令
                        if (_device.ActionStatus() == ABC.Stop)
                        {
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                throw new Exception(result);
                            }
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                        }
                    }
                    else
                    {
                        Thread.Sleep(5000);
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                    }
                    return;
                }
                #endregion

                // 取放货任务
                if (ITEM.ITEM_ID == ItemId.行车取货)
                {
                    // 有货则任务完成
                    if (_device.GoodsStatus() == ABC.GoodsYes && _device.ActionStatus() == ABC.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                }
                else if (ITEM.ITEM_ID == ItemId.行车放货)
                {
                    // 无货则任务完成
                    if (_device.GoodsStatus() == ABC.GoodsNo && _device.ActionStatus() == ABC.Stop)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                }
                // 定位任务
                else
                {
                    // 当前位置与目的位置一致 视为任务完成
                    if (_device.GetCurrentSite().Equals(ITEM.LOC_TO))
                    {
                        // 等待对接
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                        return;
                    }
                }
                // 发送指令
                if (_device.ActionStatus() == ABC.Stop)
                {
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                    {
                        throw new Exception(result);
                    }
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.ToString()));
            }
        }
    }

    #endregion

    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskControler
    {
        // 线程
        Thread _thread;
        // 线程开关
        public bool PowerSwitch = true; 
        private readonly object _ans = new object();
        // 执行对象
        List<Task> _taskList = new List<Task>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskControler()
        {
            _thread = new Thread(ThreadFunc)
            {
                Name = "任务指令处理线程",
                IsBackground = true
            };

            _thread.Start();
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        public void Close()
        {
            PowerSwitch = false;
        }

        /// <summary>
        /// 事务线程
        /// </summary>
        private void ThreadFunc()
        {
            List<Task> taskList = new List<Task>();
            while (PowerSwitch)
            {
                Thread.Sleep(500);
                if (!PublicParam.IsRunTaskOrder)
                {
                    continue;
                }
                try
                {
                    // 同步任务
                    lock (_ans)
                    {
                        taskList.Clear();
                        taskList.AddRange(_taskList);
                    }

                    foreach (var item in taskList)
                    {
                        item.DoWork();
                        // 任务完成
                        if (item.IsSuc || item.IsErr)
                        {
                            IDeletTask(item.Id);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 开始一个新任务
        /// </summary>
        /// <param name="task"></param>
        public void StartTask(Task task)
        {
            lock (_ans)
            {
                Task exit = _taskList.Find(c => { return c.Id == task.Id; });

                if (exit == null)
                {
                    // 新增任务
                    _taskList.Add(task);
                }
            }
        }

        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="Id"></param>
        public void IDeletTask(string Id)
        {
            lock (_ans)
            {
                Task exit = _taskList.Find(c => { return c.Id == Id; });

                if (exit != null && _taskList.Contains(exit))
                {
                    // 删除任务
                    _taskList.Remove(exit);
                }
            }
        }

        /// <summary>
        /// 指令是否在任务链表中
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsOrderInTask(string id)
        {
            lock (_ans)
            {
                return _taskList.Find(c => { return c.Id == id; }) != null;
            }
        }

        /// <summary>
        /// 停止事务线程
        /// </summary>
        public void ThreadStop()
        {
            if (_thread != null) _thread.Abort();
        }
    }
}
