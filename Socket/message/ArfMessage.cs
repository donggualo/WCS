using Module;
using Module.DEV;
using Socket.module;
using System.Runtime.InteropServices;

namespace Socket.message
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArfRecive_S
    {
        public ushort CommandHead;//命令字头【0x95,0x02】
        public byte DeviceNo;//设备号
        public byte ActionStatus;//运行状态
        public byte DeviceStatus;//设备状态
        public byte CommandStatus;//命令状态
        public byte AimSite1;//目标值 1
        public byte AimSite2;//目标值 2
        public byte AimSite3;//目标值 3
        public byte AimSite4;//目标值 4
        public byte CurrentTask;//当前执行任务
        public byte CurrentSite;//当前坐标值
        public byte RollerStatus;//当前辊台状态
        public byte RollerDirection;//辊台运行方向
        public byte FinishTask;//完成任务
        public byte GoodsStatus;//货物状态
        public byte ErrorMessage;//故障信息
        public byte Reserve;//预留
        public ushort CommandTail;//命令字尾【0xFF,0xFE】
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArfSend_S
    {
        public ushort Head;//命令字头【0x94,0x02】
        public byte Device;//设备号
        public byte Control;//控制码
        public byte Site1;//值1
        public byte Site2;//值2
        public byte Site3;//值3
        public byte Site4;//值4
        public ushort Tail;//结束符【0xFF, 0xFE】
    }

    public class ArfMessage : IMessageBase
    {
        public ArfMessage(byte[] msg)
        {
            ArfRecive_S st = BufferToStruct<ArfRecive_S>(msg);

            DeviceARF m = new DeviceARF
            {
                ActionStatus = (ActionEnum)st.ActionStatus,
                DeviceStatus = (DeviceEnum)st.DeviceStatus,
                CommandStatus = (CommandEnum)st.CommandStatus,
                CurrentTask = (TaskEnum)st.CurrentTask,
                CurrentSite = st.CurrentSite,
                RollerStatus = (RollerStatusEnum)st.RollerStatus,
                RollerDiretion = (RollerDiretionEnum)st.RollerDirection,
                FinishTask = (TaskEnum)st.FinishTask,
                GoodsStatus = (GoodsEnum)st.GoodsStatus,
                ErrorMessage = st.ErrorMessage,
            };

            Module = m;
        }
    }
}
