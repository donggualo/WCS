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
    public struct ACI_MSG_n
    {
        public ushort index;
        public byte carno;
    }

    public class Message_n : ACIMessageBase
    {
        private ushort m_Index;
        private byte m_CarNo;
        /// <summary>
        /// Delete Order Message – n format a
        /// </summary>
        /// <param name="index"></param>
        public Message_n(ushort index)
            : base("n")
        {
            m_Index = index;
        }
        /// <summary>
        /// Delete Order Message – n format b
        /// </summary>
        /// <param name="carno"></param>
        public Message_n(byte carno)
            : base("n")
        {
            m_Index = 0;
            m_CarNo = carno;
        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                ACI_MSG_n msg = new ACI_MSG_n();
                int size = 0;

                msg.index = shiftBytes((ushort)m_Index);
                size += Marshal.SizeOf(msg.index);

                if(m_CarNo > 0)
                {
                    msg.carno = m_CarNo;
                    size += Marshal.SizeOf(msg.carno);
                }

                return new MsgBuffer(structToBuffer<ACI_MSG_n>(msg), size);
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_n)));
            }
        }
    }
}
