using System;
using System.Collections.Generic;
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
        /// 任务等待对接
        /// </summary>
        public void ISetTaskWait()
        {
            if (_ITEM.ID.ToString() != null)
            {
                // 更新数据库资讯
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, ItemColumnName.作业状态, ItemStatus.交接中);
                // 任务完成
                _isSuc = true;
                DataControl._mSocket.SwithRefresh(_ITEM.DEVICE, true);
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
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, ItemColumnName.作业状态, ItemStatus.完成任务);
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
                DataControl._mTaskTools.UpdateItem(_ITEM.ID, _ITEM.WCS_NO, ItemColumnName.作业状态, ItemStatus.出现异常);
                // 任务完成
                _isErr = true;
                DataControl._mSocket.SwithRefresh(_ITEM.DEVICE, true);
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
            log = new Log("AGV_FRT-" + ITEM.ID + "-");
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
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                            throw new Exception(result);
                        }
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
                if (!string.IsNullOrEmpty(ITEM.LOC_TO)) // 目标不为空即最终无货 --送货
                {
                    // 固定辊台无货物
                    if (_device.GoodsStatus() == FRT.GoodsNoAll && _device.ActionStatus() == FRT.Stop &&
                        _device.FinishTask() == FRT.TaskRelease)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(String.Format(@"【Success】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                        return;
                    }
                }
                else // 接货
                {
                    bool isOK = false;
                    switch (ITEM.LOC_FROM.Substring(ITEM.LOC_FROM.Length - 2))
                    {
                        case "-1":
                            // 固定辊台上有1#有货
                            if (_device.GoodsStatus() == FRT.GoodsYes1 && _device.ActionStatus() == FRT.Stop && _device.FinishTask() == FRT.TaskRelease)
                            {
                                isOK = true;
                            }
                            break;
                        case "-2":
                            // 固定辊台上有都有货
                            if (_device.GoodsStatus() == FRT.GoodsYesAll && _device.ActionStatus() == FRT.Stop && _device.FinishTask() == FRT.TaskRelease)
                            {
                                isOK = true;
                            }
                            break;
                        default:
                            break;
                    }

                    // 固定辊台上有1#有货 或者 都有货
                    if (isOK)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // LOG
                        log.LOG(String.Format(@"【Success】{0}：WMS任务ID[ {1} ]，AGV任务ID[ {2} ]，设备号[ {3} ], 指令[ {4} ].",
                        ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, DataControl._mStools.BytetToString(Order)));
                        return;
                    }
                }
                // 当完成状态-辊台任务，立即发送 停止辊台任务 指令
                if (_device.FinishTask() == FRT.TaskTake)
                {
                    byte[] order = FRT._StopRoller(_device.FRTNum());
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, order, out string res))
                    {
                        throw new Exception(res);
                    }
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, order));
                }
                // 发送指令
                else if (_device.CurrentTask() != _device.FinishTask() || _device.ActionStatus() == FRT.Stop)
                {
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
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
                ITEM.ITEM_ID, ITEM.WCS_NO, ITEM.ID, ITEM.DEVICE, ex.Message));
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
        byte[] rollOutOrder;
        public FRTTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new FRT(ITEM.DEVICE);
            log = new Log("Task_FRT-" + ITEM.ID + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));

            // 分段指令
            rollOutOrder = null;
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
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
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

                // 对接设备状态
                if (!string.IsNullOrEmpty(ITEM.LOC_TO)) // 目标不为空即最终无货 --送货
                {
                    // 固定辊台无货物
                    if (_device.GoodsStatus() == FRT.GoodsNoAll && _device.ActionStatus() == FRT.Stop &&
                        _device.FinishTask() == FRT.TaskRelease)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // 解锁设备数据状态
                        DataControl._mTaskTools.UnLockByDevAndWcsNo(ITEM.DEVICE, ITEM.WCS_NO);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                    ARF _arf = new ARF(ITEM.LOC_TO);
                    // 摆渡车辊台停止状态 || 方向相邻的辊台有货
                    if (_arf.CurrentStatus() == ARF.RollerStop || _arf.GoodsStatus() == ARF.GoodsYes2)
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        return;
                    }
                    // 固定辊台与摆渡车都有货，不启动辊台
                    if (_device.ActionStatus() == FRT.Stop && (
                        (_device.GoodsStatus() != FRT.GoodsNoAll && _arf.GoodsStatus() == ARF.GoodsYesAll) ||
                        (_device.GoodsStatus() == FRT.GoodsYesAll && _arf.GoodsStatus() != ARF.GoodsNoAll)))
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        return;
                    }
                    // 分段送货
                    if (_device.GoodsStatus() == FRT.GoodsYesAll)
                    {
                        // 固定辊台送摆渡车只有 正向
                        rollOutOrder = FRT._RollerControl(_device.FRTNum(), FRT.RollerRun1, FRT.RunFront, FRT.GoodsDeliver, FRT.GoodsQty1);
                    }
                    else
                    {
                        rollOutOrder = null;
                    }
                }
                else // 接货
                {
                    ARF _arf = new ARF(ITEM.LOC_FROM);
                    // 摆渡车辊台上无货物,固定辊台上有货物
                    if (_arf.GoodsStatus() == ARF.GoodsNoAll && _device.GoodsStatus() != FRT.GoodsNoAll &&
                        _device.ActionStatus() == FRT.Stop && _device.FinishTask() == FRT.TaskRelease)
                    {
                        // 完成任务
                        ISetTaskSuc();
                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(ITEM.DEVICE);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                    // 固定辊台与摆渡车都有货，不启动辊台
                    if (_device.ActionStatus() == FRT.Stop && (_device.GoodsStatus() == FRT.GoodsYesAll ||
                        (_device.GoodsStatus() != FRT.GoodsNoAll && _arf.GoodsStatus() == ARF.GoodsYesAll)))
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        return;
                    }
                }
                // 当完成状态-辊台任务，立即发送 停止辊台任务 指令
                if (_device.FinishTask() == FRT.TaskTake)
                {
                    byte[] order = FRT._StopRoller(_device.FRTNum());
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, order, out string res))
                    {
                        throw new Exception(res);
                    }
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, order));
                }
                // 发送指令
                else if (_device.CurrentTask() != _device.FinishTask() || _device.ActionStatus() == FRT.Stop)
                {
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, rollOutOrder ?? Order, out string result))
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        throw new Exception(result);
                    }
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.Message));
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
        byte[] rollOutOrder;
        public ARFTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new ARF(ITEM.DEVICE);
            log = new Log("Task_ARF-" + ITEM.ID + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));

            // 分段指令
            rollOutOrder = null;
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
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
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

                    if (!string.IsNullOrEmpty(ITEM.LOC_TO)) // 目标不为空即最终无货 --送货
                    {
                        // 摆渡车无货物
                        if (_device.GoodsStatus() == ARF.GoodsNoAll && _device.ActionStatus() == ARF.Stop &&
                            _device.FinishTask() == ARF.TaskRelease)
                        {
                            // 完成任务
                            ISetTaskWait();
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                            return;
                        }
                        // 获取目标设备类型
                        String typeTo = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_TO);
                        // 摆渡车对接 固定辊台/运输车
                        switch (typeTo)
                        {
                            case DeviceType.固定辊台:
                                FRT _frt = new FRT(ITEM.LOC_TO);
                                // 固定辊台停止状态 || 方向相邻的辊台有货
                                if (_frt.CurrentStatus() == FRT.RollerStop || _frt.GoodsStatus() == FRT.GoodsYes1)
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 摆渡车与固定辊台都有货，不启动辊台
                                if (_device.ActionStatus() == ARF.Stop && (
                                    (_device.GoodsStatus() != ARF.GoodsNoAll && _frt.GoodsStatus() == FRT.GoodsYesAll) ||
                                    (_device.GoodsStatus() == ARF.GoodsYesAll && _frt.GoodsStatus() != FRT.GoodsNoAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 分段送货
                                if (_device.GoodsStatus() == ARF.GoodsYesAll)
                                {
                                    // 摆渡车送固定辊台只有 反向
                                    rollOutOrder = ARF._RollerControl(_device.ARFNum(), ARF.RollerRun2, ARF.RunObverse, ARF.GoodsDeliver, ARF.GoodsQty1);
                                }
                                else
                                {
                                    rollOutOrder = null;
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_TO);
                                // 运输车辊台停止状态 || 方向相邻的辊台有货
                                if (_rgv.CurrentStatus() == RGV.RollerStop || _rgv.GoodsStatus() == RGV.GoodsYes2)
                                {
                                    return;
                                }
                                // 摆渡车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == ARF.Stop && (
                                    (_device.GoodsStatus() != ARF.GoodsNoAll && _rgv.GoodsStatus() == RGV.GoodsYesAll) ||
                                    (_device.GoodsStatus() == ARF.GoodsYesAll && _rgv.GoodsStatus() != RGV.GoodsNoAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 分段送货
                                if (_device.GoodsStatus() == ARF.GoodsYesAll)
                                {
                                    // 摆渡车送运输车只有 正向
                                    rollOutOrder = ARF._RollerControl(_device.ARFNum(), ARF.RollerRun1, ARF.RunFront, ARF.GoodsDeliver, ARF.GoodsQty1);
                                }
                                else
                                {
                                    rollOutOrder = null;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else // 接货
                    {
                        // 获取目标设备类型
                        String typeFrom = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_FROM);
                        // 摆渡车对接 固定辊台/运输车
                        switch (typeFrom)
                        {
                            case DeviceType.固定辊台:
                                FRT _frt = new FRT(ITEM.LOC_FROM);
                                // 固定辊台上无货物,摆渡车辊台上有货物
                                if (_frt.GoodsStatus() == FRT.GoodsNoAll && _device.GoodsStatus() != ARF.GoodsNoAll
                                    && _device.ActionStatus() == ARF.Stop && _device.FinishTask() == ARF.TaskRelease)
                                {
                                    // 完成任务
                                    ISetTaskWait();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                // 固定辊台与摆渡车都有货，不启动辊台
                                if (_device.ActionStatus() == ARF.Stop && (_device.GoodsStatus() == ARF.GoodsYesAll ||
                                    (_device.GoodsStatus() != ARF.GoodsNoAll && _frt.GoodsStatus() == FRT.GoodsYesAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }

                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_FROM);
                                // 运输车辊台上无货物,摆渡车辊台上有货物
                                if (_rgv.GoodsStatus() == RGV.GoodsNoAll && _device.GoodsStatus() != ARF.GoodsNoAll
                                    && _device.ActionStatus() == ARF.Stop && _device.FinishTask() == ARF.TaskRelease)
                                {
                                    // 完成任务
                                    ISetTaskWait();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                // 摆渡车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == ARF.Stop && (_device.GoodsStatus() == ARF.GoodsYesAll ||
                                    (_device.GoodsStatus() != ARF.GoodsNoAll && _rgv.GoodsStatus() == RGV.GoodsYesAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }

                                break;
                            default:
                                break;
                        }
                    }
                    // 当完成状态-辊台任务，立即发送 停止辊台任务 指令
                    if (_device.FinishTask() == ARF.TaskTake)
                    {
                        byte[] order = ARF._StopRoller(_device.ARFNum());
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, order, out string res))
                        {
                            throw new Exception(res);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, order));
                    }
                    // 发送指令
                    else if (_device.CurrentTask() != _device.FinishTask() || _device.ActionStatus() == ARF.Stop)
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, rollOutOrder ?? Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
                // 定位任务
                else
                {
                    // 当前位置与目的位置一致 视为任务完成
                    if (_device.CurrentSite() == Convert.ToInt32(ITEM.LOC_TO) && _device.ActionStatus() == ARF.Stop)
                    {
                        // 等待对接
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                        return;
                    }
                    // 发送指令
                    if (_device.ActionStatus() == ARF.Stop)
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                            throw new Exception(result);
                        }
                        // 发完定位后立即获取
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.Message));
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
        byte[] rollOutOrder;
        public RGVTack(WCS_TASK_ITEM item, string deviceType, byte[] order) : base(item, deviceType, order)
        {
            _device = new RGV(ITEM.DEVICE);
            log = new Log("Task_RGV-" + ITEM.ID + "-");
            // 记录生成指令LOG
            log.LOG(DataControl._mTaskTools.GetLogMessC(item, order));

            // 分段指令
            rollOutOrder = null;
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
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
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

                    if (!string.IsNullOrEmpty(ITEM.LOC_TO)) // 目标不为空即最终无货 --送货
                    {
                        // 运输车无货物
                        if (_device.GoodsStatus() == RGV.GoodsNoAll && _device.ActionStatus() == RGV.Stop &&
                            _device.FinishTask() == RGV.TaskRelease)
                        {
                            // 完成任务
                            ISetTaskWait();
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                            return;
                        }
                        // 获取目标设备类型
                        String typeTo = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_TO);
                        // 运输车对接 摆渡车/运输车
                        switch (typeTo)
                        {
                            case DeviceType.摆渡车:
                                ARF _arf = new ARF(ITEM.LOC_TO);
                                // 摆渡车辊台停止状态 || 方向相邻的辊台有货
                                if (_arf.CurrentStatus() == ARF.RollerStop || _arf.GoodsStatus() == ARF.GoodsYes1)
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 摆渡车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == RGV.Stop && (
                                    (_device.GoodsStatus() != RGV.GoodsNoAll && _arf.GoodsStatus() == ARF.GoodsYesAll) ||
                                    (_device.GoodsStatus() == RGV.GoodsYesAll && _arf.GoodsStatus() != ARF.GoodsNoAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 分段送货
                                if (_device.GoodsStatus() == RGV.GoodsYesAll)
                                {
                                    // 运输车送摆渡车只有 反向
                                    rollOutOrder = RGV._RollerControl(_device.RGVNum(), RGV.RollerRun2, RGV.RunObverse, RGV.GoodsDeliver, RGV.GoodsQty1);
                                }
                                else
                                {
                                    rollOutOrder = null;
                                }

                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_TO);
                                // 目的运输车辊台停止状态 || 方向相邻的辊台有货
                                if (_rgv.CurrentStatus() == RGV.RollerStop)
                                {
                                    return;
                                }
                                else
                                {
                                    if (_device.GetCurrentSite() > _rgv.GetCurrentSite())
                                    {
                                        // 反向
                                        if (_rgv.GoodsStatus() == RGV.GoodsYes1) return;
                                    }
                                    else
                                    {
                                        // 正向
                                        if (_rgv.GoodsStatus() == RGV.GoodsYes2) return;
                                    }
                                }
                                // 运输车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == RGV.Stop && (
                                    (_device.GoodsStatus() != RGV.GoodsNoAll && _rgv.GoodsStatus() == RGV.GoodsYesAll) ||
                                    (_device.GoodsStatus() == RGV.GoodsYesAll && _rgv.GoodsStatus() != RGV.GoodsNoAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                // 分段送货
                                if (_device.GoodsStatus() == RGV.GoodsYesAll)
                                {
                                    if (_device.GetCurrentSite() > _rgv.GetCurrentSite())
                                    {
                                        // 靠内的运输车送靠外的运输车只有 反向
                                        rollOutOrder = RGV._RollerControl(_device.RGVNum(), RGV.RollerRun2, RGV.RunObverse, RGV.GoodsDeliver, RGV.GoodsQty1);
                                    }
                                    else
                                    {
                                        // 靠外的运输车送靠内的运输车只有 正向
                                        rollOutOrder = RGV._RollerControl(_device.RGVNum(), RGV.RollerRun1, RGV.RunFront, RGV.GoodsDeliver, RGV.GoodsQty1);
                                    }
                                }
                                else
                                {
                                    rollOutOrder = null;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else //接货
                    {
                        // 获取目标设备类型
                        String typeFrom = DataControl._mTaskTools.GetDeviceType(ITEM.LOC_FROM);
                        // 运输车对接 摆渡车/运输车
                        switch (typeFrom)
                        {
                            case DeviceType.摆渡车:
                                ARF _arf = new ARF(ITEM.LOC_FROM);
                                // 摆渡车辊台上无货物,运输车辊台上有货物
                                if (_arf.GoodsStatus() == ARF.GoodsNoAll && _device.GoodsStatus() != RGV.GoodsNoAll &&
                                    _device.ActionStatus() == RGV.Stop && _device.FinishTask() == RGV.TaskRelease)
                                {
                                    // 完成任务
                                    ISetTaskWait();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                // 摆渡车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == RGV.Stop && (_device.GoodsStatus() == RGV.GoodsYesAll ||
                                    (_device.GoodsStatus() != RGV.GoodsNoAll && _arf.GoodsStatus() == ARF.GoodsYesAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                break;
                            case DeviceType.运输车:
                                RGV _rgv = new RGV(ITEM.LOC_FROM);
                                // 来源运输车辊台上无货物,目的运输车辊台上有货物
                                if (_rgv.GoodsStatus() == RGV.GoodsNoAll && _device.GoodsStatus() != RGV.GoodsNoAll &&
                                    _device.ActionStatus() == RGV.Stop && _device.FinishTask() == RGV.TaskRelease)
                                {
                                    // 完成任务
                                    ISetTaskWait();
                                    // LOG
                                    log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                                    return;
                                }
                                // 运输车与运输车都有货，不启动辊台
                                if (_device.ActionStatus() == RGV.Stop && (_device.GoodsStatus() == RGV.GoodsYesAll ||
                                    (_device.GoodsStatus() != RGV.GoodsNoAll && _rgv.GoodsStatus() == RGV.GoodsYesAll)))
                                {
                                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                                    return;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    // 当完成状态-辊台任务，立即发送 停止辊台任务 指令
                    if (_device.FinishTask() == RGV.TaskTake)
                    {
                        byte[] order = RGV._StopRoller(_device.RGVNum());
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, order, out string res))
                        {
                            throw new Exception(res);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, order));
                    }
                    // 发送指令
                    else if (_device.CurrentTask() != _device.FinishTask() || _device.ActionStatus() == RGV.Stop)
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, rollOutOrder ?? Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                            throw new Exception(result);
                        }
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
                // 定位任务
                else
                {
                    /* 设备坐标偏差 */
                    if (!DataControl._mTaskTools.GetLocByAddGap(ITEM.DEVICE, ITEM.LOC_TO, out string resultRGV))
                    {
                        // 记录LOG
                        DataControl._mTaskTools.RecordTaskErrLog("RGVTack.DoWork()", "RGV指令任务[设备号,坐标]", ITEM.DEVICE, ITEM.LOC_TO, resultRGV);
                        return;
                    }
                    // 精度范围值
                    int limit = Convert.ToInt32(DataControl._mStools.GetValueByKey("RGVLimit"));
                    // 当前位置与目的位置一致 视为任务完成
                    if (Convert.ToInt32(resultRGV) >= (_device.GetCurrentSite() - limit) &&
                        Convert.ToInt32(resultRGV) <= (_device.GetCurrentSite() + limit) && _device.ActionStatus() == RGV.Stop)
                    {
                        // 等待对接
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                        return;
                    }
                    // 发送指令
                    if (_device.ActionStatus() == RGV.Stop)
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                        if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                        {
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                            throw new Exception(result);
                        }
                        // 发完定位后立即获取
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                    }
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.Message));
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
            log = new Log("Task_ABC-" + ITEM.ID + "-");
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
                    // 异常处理任务
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
                            DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                            if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                            {
                                DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
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
                    if (_device.GoodsStatus() == ABC.GoodsYes && _device.ActionStatus() == ABC.Stop && _device.FinishTask() == ABC.TaskTake)
                    {
                        // 完成任务
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                }
                else if (ITEM.ITEM_ID == ItemId.行车放货)
                {
                    // 无货则任务完成
                    if (_device.GoodsStatus() == ABC.GoodsNo && _device.ActionStatus() == ABC.Stop && _device.FinishTask() == ABC.TaskRelease)
                    {
                        // 完成任务
                        ISetTaskWait();
                        // LOG
                        log.LOG(DataControl._mTaskTools.GetLogMessS(ITEM, Order));
                        return;
                    }
                }
                // 定位任务
                else
                {
                    // 当前位置与目的位置一致 视为任务完成
                    if (_device.ActionStatus() == ABC.Stop)
                    {
                        // 获取当前坐标X轴值
                        int X = DataControl._mStools.BytesToInt(_device.CurrentXsite(), 0);
                        int Y = DataControl._mStools.BytesToInt(_device.CurrentYsite(), 0);

                        /* 设备坐标偏差 */
                        if (!DataControl._mTaskTools.GetLocByAddGap(ITEM.DEVICE, ITEM.LOC_TO, out string resultABC))
                        {
                            // 记录LOG
                            DataControl._mTaskTools.RecordTaskErrLog("ABCTack.DoWork()", "ABC指令任务[设备号,坐标]", ITEM.DEVICE, ITEM.LOC_TO, resultABC);
                            return;
                        }
                        // 目标位置 X/Y 轴值
                        int locToX = Convert.ToInt32(resultABC.Split('-')[0]);
                        int locToY = Convert.ToInt32(resultABC.Split('-')[1]);

                        // 精度范围值
                        int limit = Convert.ToInt32(DataControl._mStools.GetValueByKey("ABCLimit"));

                        if ((locToX >= (X - limit) && locToX <= (X + limit)) &&
                            (locToY >= (Y - limit) && locToY <= (Y + limit)))
                        {
                            // 等待对接
                            ISetTaskWait();
                            // LOG
                            log.LOG(DataControl._mTaskTools.GetLogMessW(ITEM, Order));
                            return;
                        }
                    }
                }
                // 发送指令
                if (_device.ActionStatus() == ABC.Stop)
                {
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, false);
                    if (!DataControl._mSocket.SendToClient(ITEM.DEVICE, Order, out string result))
                    {
                        DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                        throw new Exception(result);
                    }
                    // 发后立即获取
                    DataControl._mSocket.SwithRefresh(ITEM.DEVICE, true);
                    // LOG
                    log.LOG(DataControl._mTaskTools.GetLogMess(ITEM, Order));
                }
            }
            catch (Exception ex)
            {
                // LOG
                log.LOG(DataControl._mTaskTools.GetLogMessE(ITEM, Order, ex.Message));
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
                Thread.Sleep(1000);
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
