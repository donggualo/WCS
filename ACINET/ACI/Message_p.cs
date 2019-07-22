using System.Collections.Generic;
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
    public struct ACI_MSG_p
    {
        public ushort magic;
        public byte code;
        public byte par_num;
        public ushort par_ix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public ushort[] GpValues;
    }

    public class Message_p : ACIMessageBase
    {
        private List<int> m_Values;

        public Message_p(byte[] msg) : base("p")
        {
            ACI_MSG_p msg_p = BufferToStruct<ACI_MSG_p>(msg);

            Magic = shiftBytes(msg_p.magic);

            Code = msg_p.code;

            NumberOfParameters = msg_p.par_num;

            GlobalParameterOffset = shiftBytes(msg_p.par_ix);

            if(Code == 1 | Code == 3)
            {
                m_Values = new List<int>();
                for(int i = 0; i < NumberOfParameters; i++)
                {
                    m_Values.Add(shiftBytes(msg_p.GpValues[i]));
                }
            }
        }

        public int Magic
        {
            get; private set;
        }

        public byte Code
        {
            get; private set;
        }

        public byte NumberOfParameters
        {
            get; private set;
        }

        public int GlobalParameterOffset
        {
            get; private set;
        }

        public List<int> Values
        {
            get
            {
                return m_Values ?? (m_Values = new List<int>());
            }
        }


        public override string ToString()
        {
            return string.Format("Message of type '{0}', Magic '{1}', Code '{2}', Number of parameters '{3}', Global parameter offset '{4}' GlobalParameters '{5}', Time :{6} "
                , Type, Magic, Code, NumberOfParameters, GlobalParameterOffset, string.Join(",", Values.ToArray()), CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
