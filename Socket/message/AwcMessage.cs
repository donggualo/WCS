using Module;
using Module.DEV;
using Socket.module;
using System;
using System.Runtime.InteropServices;

namespace Socket.message
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AwcRecive_S
    {
        public ushort CommandHead;//命令字头【0x91,0x02】
        public byte DeviceNo;//设备号
        public byte ActionStatus;//运行状态
        public byte DeviceStatus;//设备状态
        public byte CommandStatus;//命令状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] AimSiteX;//目标 X 坐标
        public ushort AimSiteY;//目标 Y 坐标
        public ushort AimSiteZ;//目标 Z 坐标
        public byte CurrentTask;//当前执行任务
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] CurrentSiteX;//当前 X 坐标
        public ushort CurrentSiteY;//当前 Y 坐标
        public ushort CurrentSiteZ;//当前 Z 坐标
        public byte FinishTask;//完成任务
        public byte GoodsStatus;//货物状态
        public byte ErrorMessage;//故障信息
        public byte Reserve;//预留
        public ushort CommandTail;//命令字尾【0xFF,0xFE】
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AwcSend_S
    {
        public ushort Head;//命令字头【0x90,0x02】
        public byte Device;//设备号
        public byte Control;//控制码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] SiteX;//X轴坐标
        public ushort SiteY;//Y轴坐标
        public ushort SiteZ;//Z轴坐标
        public ushort Tail;//结束符【0xFF, 0xFE】
    }

    public class AwcMessage : IMessageBase
    {
        public AwcMessage(byte[] msg)
        {
            AwcRecive_S st = BufferToStruct<AwcRecive_S>(msg);

            byte[] x = new byte[4];
            x[0] = 0;
            x[1] = st.CurrentSiteX[0];
            x[2] = st.CurrentSiteX[1];
            x[3] = st.CurrentSiteX[2];
            DeviceAWC m = new DeviceAWC
            {
                ActionStatus = (ActionEnum)st.ActionStatus,
                DeviceStatus = (DeviceEnum)st.DeviceStatus,
                CommandStatus = (CommandEnum)st.CommandStatus,
                CurrentTask = (AwcTaskEnum)st.CurrentTask,
                CurrentSiteX = BitConverter.ToInt32(ShiftBytes(x),0),//BitConverter.ToInt32(ShiftBytes(st.CurrentSiteX), 0),
                CurrentSiteY = ShiftBytes(st.CurrentSiteY),
                CurrentSiteZ = ShiftBytes(st.CurrentSiteZ),
                FinishTask = (AwcTaskEnum)st.FinishTask,
                GoodsStatus = (AwcGoodsEnum)st.GoodsStatus,
                ErrorMessage = st.ErrorMessage
            };

            Module = m;
        }
    }
}
