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
    public struct ACI_MSG_j_limited_A
    {
        public ushort index;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_j_limited_B
    {
        public ushort index;
        public byte carno;
    }

    public class Message_j_limited : ACIMessageBase
    {
        private enum JType
        {
            A,
            B
        }
        private JType m_JType;
        private int m_Index;
        private int m_CarNo;

        // for serialization
        public Message_j_limited()
            : base("j")
        {
        }
        /// <summary>
        /// Order Status Request Message – j  (limited)  format a
        /// </summary>
        /// <param name="index"></param>
        public Message_j_limited(int index)
            : base("j")
        {
            m_JType = JType.A;
            m_Index = index;

        }
        /// <summary>
        /// Order Status Request Message – j  (limited)  format b
        /// </summary>
        /// <param name="carno"></param>
        public Message_j_limited(ushort carno)
            : base("j")
        {
            m_JType = JType.B;
            m_Index = 0; // Don't care about index just ask for carrier number
            m_CarNo = carno;

        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                if(m_JType == JType.A)
                {
                    ACI_MSG_j_limited_A msg = new ACI_MSG_j_limited_A();
                    int size = 0;

                    msg.index = (ushort)m_Index;
                    size += Marshal.SizeOf(msg.index);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_limited_A>(msg), size);

                }
                else if(m_JType == JType.B)
                {
                    ACI_MSG_j_limited_B msg = new ACI_MSG_j_limited_B();
                    int size = 0;

                    msg.index = (ushort)m_Index;
                    msg.carno = (byte)m_CarNo;

                    size += Marshal.SizeOf(msg.index) + Marshal.SizeOf(msg.carno);

                    return new MsgBuffer(structToBuffer<ACI_MSG_j_limited_B>(msg), size);
                }
                else
                {
                    throw new ArgumentException("Unknown type of 'j_limited' message");
                }
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_j_limited_A)));
            }
        }
    }
}
