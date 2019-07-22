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
    public struct ACI_MSG_q_A
    {
        public byte trpsno;
        public byte pri;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ACI_MSG_q_B
    {
        public byte trpsno;
        public byte pri;
        public ushort code;
        public ushort ikey;
    }

    public class Message_q : ACIMessageBase
    {
        private enum QType
        {
            A,
            B
        }

        private QType m_QType;
        private int m_TS;
        private int m_Pri;
        private int m_Code;
        private int m_IKEY;
        private List<int> m_Par;

        // for serialization
        public Message_q()
            : base("q")
        {
        }
        /// <summary>
        /// Order Initiate Message – q format a
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="pri"></param>
        /// <param name="par"></param>
        public Message_q(int ts, int pri, List<int> par) : base("q")
        {
            m_QType = QType.A;
            m_TS = ts;
            m_Pri = pri;
            m_Par = par;
        }
        /// <summary>
        /// Order Initiate Message – q format b
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="pri"></param>
        /// <param name="code"></param>
        /// <param name="ikey"></param>
        /// <param name="par"></param>
        public Message_q(int ts, int pri, int code, int ikey, List<int> par)
            : base("q")
        {
            m_QType = QType.B;
            m_TS = ts;
            m_Pri = pri;
            m_Code = code;
            m_IKEY = ikey;
            m_Par = par;
        }
        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                if(m_QType == QType.A)
                {
                    ACI_MSG_q_A msg = new ACI_MSG_q_A();

                    msg.trpsno = (byte)m_TS;
                    msg.pri = (byte)m_Pri;
                    
                    byte[] data = structToBuffer<ACI_MSG_q_A>(msg);

                    if(m_Par != null)
                    {
                        data = data
                            .Concat(m_Par
                                    .SelectMany(par => BitConverter.GetBytes(shiftBytes((ushort)par)))
                                    .ToArray())
                            .ToArray();
                    }

                    return new MsgBuffer(data, data.Count<byte>());

                }
                else if(m_QType == QType.B)
                {
                    ACI_MSG_q_B msg = new ACI_MSG_q_B();

                    msg.trpsno = (byte)m_TS;
                    msg.pri = (byte)m_Pri;
                    msg.code = shiftBytes((ushort)m_Code);
                    msg.ikey = shiftBytes((ushort)m_IKEY);

                    byte[] data = structToBuffer<ACI_MSG_q_B>(msg);

                    if(m_Par != null)
                    {
                        data = data
                            .Concat(m_Par
                                    .SelectMany(par => BitConverter.GetBytes(shiftBytes((ushort)par)))
                                    .ToArray())
                            .ToArray();
                    }

                    return new MsgBuffer(data, data.Count<byte>());
                }
                else
                {
                    throw new ArgumentException("Unknown type of 'q' message");
                }
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_q_A)));
            }
        }
    }
}
