using System;
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
    public struct ACI_MSG_j_extended_A
    {
        public ushort magic;
        public byte spare;
        public byte itemc;
        public ushort itemo;
        public ushort itemn;
        public UInt32 lpflags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_j_extended_B
    {
        public ushort magic;
        public byte spare;
        public byte itemc;
        public ushort itemo;
        public ushort itemn;
        public UInt32 lpflags;
        public ushort ut;
        public ushort lline;
        public ushort lunit;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_j_extended_C
    {
        public ushort magic;
        public byte spare;
        public byte itemc;
        public ushort itemo;
        public ushort itemn;
        public UInt32 lpflags;
        public ushort module;
        public ushort mtype;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_j_extended_D
    {
        public ushort magic;
        public byte spare;
        public byte itemc;
        public ushort itemo;
        public ushort itemn;
        public UInt32 lpflags;
        public ushort itemt;
    }

    public class Message_j_extended : ACIMessageBase
    {
        private enum JType
        {
            A,
            B,
            C,
            D
        }
        private JType m_JType;
        private int m_Magic;
        private byte m_Spare;
        private byte m_ItemCode;
        private int m_ItemOffset;
        private int m_ItemNumber;
        private UInt32 m_LocalParamterFlags;
        private int m_ItemTypeCode;
        private int m_UnitType;
        private int m_LogicalLine;
        private int m_LogicalUnit;
        private int m_Module;
        private int m_Mtype;

        // for serialization
        public Message_j_extended()
            : base("j")
        {
        }
        /// <summary>
        /// Order Status Request Message – ‘j’ (extended)  format a
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="itemc"></param>
        /// <param name="itemo"></param>
        /// <param name="itemn"></param>
        /// <param name="lpflag"></param>
        public Message_j_extended(int magic, byte itemc, int itemo, int itemn, UInt32 lpflag)
            : base("j")
        {
            m_JType = JType.A;
            m_Magic = (ushort)magic;
            m_Spare = 0;
            m_ItemCode = itemc;
            m_ItemOffset = (ushort)itemo;
            m_ItemNumber = (ushort)itemn;
            m_LocalParamterFlags = lpflag;
        }
        /// <summary>
        /// Order Status Request Message – ‘j’ (extended)  format b
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="itemc"></param>
        /// <param name="itemo"></param>
        /// <param name="itemn"></param>
        /// <param name="lpflag"></param>
        /// <param name="ut"></param>
        /// <param name="lline"></param>
        /// <param name="lunit"></param>
        public Message_j_extended(int magic, byte itemc, int itemo, int itemn, UInt32 lpflag, int ut, int lline, int lunit)
            : base("j")
        {
            m_JType = JType.B;
            m_Magic = (ushort)magic;
            m_Spare = 0;
            m_ItemCode = itemc;
            m_ItemOffset = (ushort)itemo;
            m_ItemNumber = (ushort)itemn;
            m_LocalParamterFlags = lpflag;
            m_UnitType = (ushort)ut;
            m_LogicalLine = (ushort)lline;
            m_LogicalUnit = (ushort)lunit;
        }
        /// <summary>
        /// Order Status Request Message – ‘j’ (extended)  format c
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="itemc"></param>
        /// <param name="itemo"></param>
        /// <param name="itemn"></param>
        /// <param name="lpflag"></param>
        /// <param name="module"></param>
        /// <param name="mType"></param>
        public Message_j_extended(int magic, byte itemc, int itemo, int itemn, UInt32 lpflag, int module, int mType)
            : base("j")
        {
            m_JType = JType.C;
            m_Magic = (ushort)magic;
            m_Spare = 0;
            m_ItemCode = (byte)itemc;
            m_ItemOffset = (ushort)itemo;
            m_ItemNumber = (ushort)itemn;
            m_LocalParamterFlags = lpflag;
            m_Module = (ushort)module;
            m_Mtype = 0;
        }
        /// <summary>
        /// Order Status Request Message – ‘j’ (extended)  format d
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="itemc"></param>
        /// <param name="itemo"></param>
        /// <param name="itemn"></param>
        /// <param name="lpflag"></param>
        /// <param name="itemt"></param>
        public Message_j_extended(int magic, byte itemc, int itemo, int itemn, UInt32 lpflag, int itemt)
            : base("j")
        {
            m_JType = JType.D;
            m_Magic = (ushort)magic;
            m_Spare = 0;
            m_ItemCode = itemc;
            m_ItemOffset = (ushort)itemo;
            m_ItemNumber = (ushort)itemn;
            m_LocalParamterFlags = lpflag;
            m_ItemTypeCode = (ushort)itemt;

        }
        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                if(m_JType == JType.A)
                {
                    ACI_MSG_j_extended_A msg = new ACI_MSG_j_extended_A();
                    int size = 0;


                    msg.magic = (ushort)m_Magic;
                    msg.spare = m_Spare;
                    msg.itemc = m_ItemCode;
                    msg.itemo = (ushort)m_ItemOffset;
                    msg.itemn = (ushort)m_ItemNumber;
                    msg.lpflags = m_LocalParamterFlags;

                    size += Marshal.SizeOf(msg.magic) + Marshal.SizeOf(msg.spare) + Marshal.SizeOf(msg.itemc) + Marshal.SizeOf(msg.itemo) + Marshal.SizeOf(msg.itemn) + Marshal.SizeOf(msg.lpflags);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_extended_A>(msg), size);

                }
                else if(m_JType == JType.B)
                {
                    ACI_MSG_j_extended_B msg = new ACI_MSG_j_extended_B();
                    int size = 0;

                    msg.magic = (ushort)m_Magic;
                    msg.spare = m_Spare;
                    msg.itemc = m_ItemCode;
                    msg.itemo = (ushort)m_ItemOffset;
                    msg.itemn = (ushort)m_ItemNumber;
                    msg.lpflags = m_LocalParamterFlags;
                    msg.ut = (ushort)m_UnitType;
                    msg.lline = (ushort)m_LogicalLine;
                    msg.lunit = (ushort)m_LogicalUnit;


                    size += Marshal.SizeOf(msg.magic) + Marshal.SizeOf(msg.spare) + Marshal.SizeOf(msg.itemc) + Marshal.SizeOf(msg.itemo) + Marshal.SizeOf(msg.itemn) + Marshal.SizeOf(msg.lpflags)
                        + Marshal.SizeOf(msg.ut) + Marshal.SizeOf(msg.lline) + Marshal.SizeOf(msg.lunit);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_extended_B>(msg), size);

                }
                else if(m_JType == JType.C)
                {
                    ACI_MSG_j_extended_C msg = new ACI_MSG_j_extended_C();
                    int size = 0;

                    msg.magic = (ushort)m_Magic;
                    msg.spare = m_Spare;
                    msg.itemc = m_ItemCode;
                    msg.itemo = (ushort)m_ItemOffset;
                    msg.itemn = (ushort)m_ItemNumber;
                    msg.lpflags = m_LocalParamterFlags;
                    msg.module = (ushort)m_Module;
                    msg.mtype = (ushort)m_Mtype;


                    size += Marshal.SizeOf(msg.magic) + Marshal.SizeOf(msg.spare) + Marshal.SizeOf(msg.itemc) + Marshal.SizeOf(msg.itemo) + Marshal.SizeOf(msg.itemn) + Marshal.SizeOf(msg.lpflags)
                        + Marshal.SizeOf(msg.module) + Marshal.SizeOf(msg.mtype);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_extended_C>(msg), size);

                }
                else if(m_JType == JType.D)
                {
                    ACI_MSG_j_extended_D msg = new ACI_MSG_j_extended_D();
                    int size = 0;

                    msg.magic = (ushort)m_Magic;
                    msg.spare = m_Spare;
                    msg.itemc = (byte)m_ItemCode;
                    msg.itemo = (ushort)m_ItemOffset;
                    msg.itemn = (ushort)m_ItemNumber;
                    msg.lpflags = m_LocalParamterFlags;
                    msg.itemt = (ushort)m_ItemTypeCode;


                    size += Marshal.SizeOf(msg.magic) + Marshal.SizeOf(msg.spare) + Marshal.SizeOf(msg.itemc) + Marshal.SizeOf(msg.itemo) + Marshal.SizeOf(msg.itemn) + Marshal.SizeOf(msg.lpflags)
                        + Marshal.SizeOf(msg.itemt);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_extended_D>(msg), size);

                }

                else
                {
                    throw new ArgumentException("Unknown type of 'qj_extended' message");
                }
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_j_extended_A)));
            }
        }
    }
}
