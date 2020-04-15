using Module;
using Module.DEV;
using Socket.module;
using System.Runtime.InteropServices;

namespace Socket.message
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PklRecive_S
    {
        public ushort CommandHead;//命令字头【0x82,0x02】
        public byte DeviceNo;//设备号
        public byte ActionStatus;//运行状态
        public byte DeviceStatus;//设备状态
        public byte CommandStatus;//命令状态
        public byte CurrentTask;//当前执行任务
        public byte FinishTask;//完成任务
        public byte GoodsStatus;//货物状态
        public byte ErrorMessage;//故障信息
        public ushort CommandTail;//命令字尾【0xFF,0xFE】
    }

    class PklMessage : IMessageBase
    {
        public PklMessage(byte[] msg)
        {
            PklRecive_S st = BufferToStruct<PklRecive_S>(msg);

            DevicePKL m = new DevicePKL
            {
                ActionStatus = (ActionEnum)st.ActionStatus,
                DeviceStatus = (DeviceEnum)st.DeviceStatus,
                CommandStatus = (CommandEnum)st.CommandStatus,
                CurrentTask = (TaskEnum)st.CurrentTask,
                FinishTask = (TaskEnum)st.FinishTask,
                GoodsStatus = (GoodsEnum)st.GoodsStatus,
                ErrorMessage = st.ErrorMessage,
            };

            Module = m;
        }
    }
}
