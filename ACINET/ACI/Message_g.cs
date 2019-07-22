using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public struct ACI_MSG_g_WriteGlobalParameter
    {
        public ushort magic;
        public byte code;
        public byte par_num;
        public ushort par_ix;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_g_ReadGlobalParameter
    {
        public ushort magic;
        public byte code;
        public byte par_num;
        public ushort par_ix;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_g_PVAL
    {
        public ushort pval;
    }

    public class Message_g : ACIMessageBase
    {
        private enum GType
        {
            WriteGlobalParameter,
            ReadGlobalParameter
        }
        private GType m_GType;
        private List<int> m_Values;

        public Message_g()
            : base("g")
        {
        }

        /// <summary>
        /// Global Parameter Command  –  g 
        /// Write
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="code"></param>
        /// <param name="par_num"></param>
        /// <param name="par_ix"></param>
        /// <param name="values"></param>
        public Message_g(int magic, byte code, byte par_num, int par_ix, List<int> values)
            : base("g")
        {
            m_GType = GType.WriteGlobalParameter;
            Magic = magic;
            Code = code;
            NumberOfParameters = par_num;
            GlobalParameterOffset = par_ix;
            m_Values = values;

        }

        /// <summary>
        /// Global Parameter Command  –  g
        /// Read 
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="code"></param>
        /// <param name="par_num"></param>
        /// <param name="par_ix"></param>
        public Message_g(int magic, byte code, byte par_num, int par_ix)
            : base("g")
        {
            m_GType = GType.ReadGlobalParameter;
            Magic = magic;
            Code = code;
            NumberOfParameters = par_num;
            GlobalParameterOffset = par_ix;
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



        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {

                if(m_GType == GType.ReadGlobalParameter)
                {
                    ACI_MSG_g_ReadGlobalParameter msg = new ACI_MSG_g_ReadGlobalParameter();
                    int size = 0;

                    msg.magic = shiftBytes((ushort)Magic);
                    msg.code = Code;
                    msg.par_num = NumberOfParameters;
                    msg.par_ix = shiftBytes((ushort)GlobalParameterOffset);
                    size += Marshal.SizeOf(msg.magic) + Marshal.SizeOf(msg.code) + Marshal.SizeOf(msg.par_num) + Marshal.SizeOf(msg.par_ix);

                    return new MsgBuffer(StructToBuffer<ACI_MSG_g_ReadGlobalParameter>(msg, size));

                }

                else if(m_GType == GType.WriteGlobalParameter)
                {
                    ACI_MSG_g_WriteGlobalParameter msg = new ACI_MSG_g_WriteGlobalParameter();
                    msg.magic = shiftBytes((ushort)Magic);
                    msg.code = Code;
                    msg.par_num = NumberOfParameters;
                    msg.par_ix = shiftBytes((ushort)GlobalParameterOffset);

                    byte[] data = structToBuffer<ACI_MSG_g_WriteGlobalParameter>(msg);

                    if(m_Values != null)
                    {
                        data = data
                            .Concat(m_Values
                                    .SelectMany(par => BitConverter.GetBytes(shiftBytes((ushort)par)))
                                    .ToArray())
                            .ToArray();
                    }

                    return new MsgBuffer(data, data.Count<byte>());

                }

                else
                {
                    throw new ArgumentException("Unknown type of 'g' message");
                }
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type 'g' "));
            }
        }
    }
}
