using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
//
// This software is offered as is, with no warranties, and the user assumes all liability for its use.
// Kollmorgen Automation AB assume no responsibility or liability for errors or omissions or any actions
// resulting from the use of this software or the information contained herein.
// In no event shall Kollmorgen Automation AB be liable for any damages whatsoever,
// real or imagined, resulting from the loss of use, profits,
// or data whether or not we have been advised of the possibility of such damage. 
//
// In other words, we are letting you using the software, at your own risk,
// and try it for free so that you may determine if it suites your needs.
// By so doing, you are agreeing to the stated terms and conditions and accepting full
// responsibility for your own actions. 
//
namespace NDC8.ACINET.ACI
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_InsertLocalParameter
    {
        public ushort index;
        public byte func;
        public byte parno;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_DeleteLocalParameter
    {
        public ushort index;
        public byte func;
        public byte parno;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_ReadLocalParameter
    {
        public ushort index;
        public byte func;
        public byte parno;
        public byte p0no;
        public byte p1no;
        public byte p2no;
        public byte p3no;
        public byte p4no;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_ChangeOrderPriority
    {
        public ushort index;
        public byte func;
        public byte prio;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_ConnectAllocatedVehicle
    {
        public ushort index;
        public byte func;
        public byte agvid;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_m_PVAL
    {
        public ushort pval;
    }

    public class Message_m : ACIMessageBase
    {
        private enum MType
        {
            InsertLocalParameter,
            DeleteLocalParameter,
            ReadLocalParameter,
            ChangeOrderPriority,
            ConnectAllocatedVehicle
        }
        private MType m_MType;
        private int m_Index;
        private byte m_Func;
        private byte m_ParNo;
        private List<int> m_Values;
        private byte m_P0no;
        private byte m_P1no;
        private byte m_P2no;
        private byte m_P3no;
        private byte m_P4no;
        private byte m_Prio;
        private byte m_AgvId;

        public Message_m()
            : base("m")
        {
        }

        /// <summary>
        /// Local Parameter Message – m 
        /// Insert Local Parameter 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="func"></param>
        /// <param name="parno"></param>
        /// <param name="values"></param>
        public Message_m(int index, byte func, byte parno, List<int> values)
            : base("m")
        {
            if(func == 0 | func == 1)
            {


                m_MType = MType.InsertLocalParameter;
                m_Index = index;
                m_Func = func;
                m_ParNo = parno;
                m_Values = values;
            }
            else
            {
                throw new ArgumentException("Illegal function code");
            }
        }
        /// <summary>
        /// Local Parameter Message – m 
        /// Delete Local Parameter (code 2) , Change Order Priority (code 4) or Connect Allocated Vehicle (code 5(
        /// </summary>
        /// <param name="index"></param>
        /// <param name="func"></param>
        /// <param name="value"></param>
        public Message_m(int index, byte func, byte value)
            : base("m")
        {
            m_Func = func;
            switch(func)
            {
                case 2:
                    m_MType = MType.DeleteLocalParameter;
                    m_Index = index;
                    m_ParNo = value;
                    break;

                case 4:
                    m_MType = MType.ChangeOrderPriority;
                    m_Index = index;
                    m_Prio = value;
                    break;

                case 5:
                    m_MType = MType.ConnectAllocatedVehicle;
                    m_Index = index;
                    m_AgvId = value;
                    break;

                default:
                    throw new ArgumentException("Illegal function code");
            }


        }
        /// <summary>
        /// Local Parameter Message – m 
        /// Read Local Parameter
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p0no"></param>
        /// <param name="p1no"></param>
        /// <param name="p2no"></param>
        /// <param name="p3no"></param>
        /// <param name="p4no"></param>
        public Message_m(int index, byte p0no, byte p1no, byte p2no, byte p3no, byte p4no)
            : base("m")
        {
            m_MType = MType.ReadLocalParameter;
            m_Index = index;
            m_Func = 3;
            m_P0no = p0no;
            m_P1no = p1no;
            m_P2no = p2no;
            m_P3no = p3no;
            m_P4no = p4no;
        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {

                if(m_MType == MType.InsertLocalParameter)
                {
                    ACI_MSG_m_InsertLocalParameter msg = new ACI_MSG_m_InsertLocalParameter();

                    msg.index = shiftBytes((ushort)m_Index);
                    msg.func = m_Func;
                    msg.parno = m_ParNo;

                    byte[] data = structToBuffer<ACI_MSG_m_InsertLocalParameter>(msg);

                    if(m_Values != null)
                    {
                        data = data
                            .Concat(m_Values
                                    .SelectMany(par => BitConverter.GetBytes(shiftBytes((ushort)par)))
                                    .ToArray())
                            .ToArray();
                    }

                    return new MsgBuffer(data);

                }

                else if(m_MType == MType.DeleteLocalParameter)
                {
                    ACI_MSG_m_DeleteLocalParameter msg = new ACI_MSG_m_DeleteLocalParameter();
                    int size = 0;

                    msg.index = shiftBytes((ushort)m_Index);
                    msg.func = m_Func;
                    msg.parno = m_ParNo;
                    size += Marshal.SizeOf(msg.index) + Marshal.SizeOf(msg.func) + Marshal.SizeOf(msg.parno);

                    return new MsgBuffer(StructToBuffer<ACI_MSG_m_DeleteLocalParameter>(msg, size));
                }
                else if(m_MType == MType.ReadLocalParameter)
                {
                    ACI_MSG_m_ReadLocalParameter msg = new ACI_MSG_m_ReadLocalParameter();
                    int size = 0;

                    msg.index = shiftBytes((ushort)m_Index);
                    msg.func = m_Func;
                    msg.p0no = m_P0no;
                    msg.p1no = m_P1no;
                    msg.p2no = m_P2no;
                    msg.p3no = m_P3no;
                    msg.p4no = m_P4no;

                    size += Marshal.SizeOf(msg.index) + Marshal.SizeOf(msg.func) + Marshal.SizeOf(msg.parno)
                        + Marshal.SizeOf(msg.p0no) + Marshal.SizeOf(msg.p1no) + Marshal.SizeOf(msg.p2no) + Marshal.SizeOf(msg.p3no) + Marshal.SizeOf(msg.p4no);

                    return new MsgBuffer(StructToBuffer<ACI_MSG_m_ReadLocalParameter>(msg, size));
                }
                else if(m_MType == MType.ChangeOrderPriority)
                {
                    ACI_MSG_m_ChangeOrderPriority msg = new ACI_MSG_m_ChangeOrderPriority();
                    int size = 0;

                    msg.index = shiftBytes((ushort)m_Index);
                    msg.func = m_Func;
                    msg.prio = m_Prio;
                    size += Marshal.SizeOf(msg.index) + Marshal.SizeOf(msg.func) + Marshal.SizeOf(msg.prio);

                    return new MsgBuffer(StructToBuffer<ACI_MSG_m_ChangeOrderPriority>(msg, size));
                }
                else if(m_MType == MType.ConnectAllocatedVehicle)
                {
                    ACI_MSG_m_ConnectAllocatedVehicle msg = new ACI_MSG_m_ConnectAllocatedVehicle();
                    int size = 0;

                    msg.index = shiftBytes((ushort)m_Index);
                    msg.func = m_Func;
                    msg.agvid = m_AgvId;
                    size += Marshal.SizeOf(msg.index) + Marshal.SizeOf(msg.func) + Marshal.SizeOf(msg.agvid);

                    return new MsgBuffer(StructToBuffer<ACI_MSG_m_ConnectAllocatedVehicle>(msg, size));
                }
                else
                {
                    throw new ArgumentException("Unknown type of 'm' message");
                }
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type 'm' "));
            }
        }
    }
}
