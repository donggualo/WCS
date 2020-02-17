using Module;
using Module.DEV;
using Socket.module;
using System;
using System.Runtime.InteropServices;

namespace Socket.message
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RgvRecive_S
    {
        public ushort CommandHead;//命令字头【0x97,0x02】
        public byte DeviceNo;//设备号
        public byte ActionStatus;//运行状态
        public byte DeviceStatus;//设备状态
        public byte CommandStatus;//命令状态
        public byte AimSite1;//目标值 1
        public byte AimSite2;//目标值 2
        public byte AimSite3;//目标值 3
        public byte AimSite4;//目标值 4
        public byte CurrentTask;//当前执行任务
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CurrentSite;//当前坐标值 
        public byte RollerStatus;//当前辊台状态
        public byte RollerDirection;//辊台运行方向
        public byte FinishTask;//完成任务
        public byte GoodsStatus;//货物状态
        public byte ErrorMessage;//故障信息
        public byte Reserve;//预留
        public ushort CommandTail;//命令字尾【0xFF,0xFE】
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RgvSend_S
    {
        public ushort Head;//命令字头【0x96,0x02】
        public byte Device;//设备号
        public byte Control;//控制码
        public byte Site1;//值1
        public byte Site2;//值2
        public byte Site3;//值3
        public byte Site4;//值4
        public ushort Tail;//结束符【0xFF, 0xFE】
    }

    class RgvMessage : IMessageBase
    {
        public RgvMessage(byte[] msg)
        {
            RgvRecive_S st = BufferToStruct<RgvRecive_S>(msg);

            DeviceRGV m = new DeviceRGV
            {
                ActionStatus = (ActionEnum)st.ActionStatus,
                DeviceStatus = (DeviceEnum)st.DeviceStatus,
                CommandStatus = (CommandEnum)st.CommandStatus,
                CurrentTask = (TaskEnum)st.CurrentTask,
                RollerStatus = (RollerStatusEnum)st.RollerStatus,
                RollerDiretion = (RollerDiretionEnum)st.RollerDirection,
                CurrentSite = BitConverter.ToInt32(ShiftBytes(st.CurrentSite), 0),
                FinishTask = (TaskEnum)st.FinishTask,
                GoodsStatus = (GoodsEnum)st.GoodsStatus,
                ErrorMessage = st.ErrorMessage,
            };

            Module = m;
        }
    }
}
