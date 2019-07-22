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
    public enum HPIL_CODE : byte
    {
        HPIL_CMDRD = 1,                 // code: Read WORD
        HPIL_CMDWR = 2,                 // code: Write WORD
        HPIL_CMDRDMU = 5,               // code: Read multi byte, 18 byte reply msg
        HPIL_CMDWRMU = 6,               // code: Wite multi byte
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_HPIL_HEAD
    {
        public ushort carid;           // carrier #
        public ushort magic;           // message magic
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_HPIL_WORD
    {
        public ACI_MSG_HPIL_HEAD head;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ACI_MSG_HPIL_WORD_STRUCT[] par;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_HPIL_WORD_STRUCT
    {
        public byte code;              // parameter code
        public byte spare;             // parameter spare
        public byte omlp;              // OM LP address
        public byte plclp;             // PLC byte address
        public ushort val;             // parameter value
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_HPIL_MULTI
    {
        public ACI_MSG_HPIL_HEAD head;
        public byte code;              // parameter code
        public byte save;              // same in reply
        public ushort plclp;           // PLC address
        public byte num;               // number of bytes
        public byte spare;             // spare 0
    }

    public abstract class Message_hpil : ACIMessageBase
    {
        public int m_CarId;
        public int m_Magic;

        public Message_hpil(int carid, int magic)
            : base(">")
        {
            m_CarId = carid;
            m_Magic = magic;
        }
    }

 public class Message_hpil_word : Message_hpil
    {
        private bool m_IsPair = false;
        private byte m_Code1;
        private byte m_Code2;
        private int m_Par1Number;
        private int m_Par1Value;
        private int m_Par2Number;
        private int m_Par2Value;

        /// <summary>
        ///  OM-PLC ">"
        ///  One parameter
        /// </summary>
        /// <param name="carid"></param>
        /// <param name="magic"></param>
        /// <param name="code"></param>
        /// <param name="parNumber"></param>
        /// <param name="parValue"></param>
        public Message_hpil_word(int carid, int magic, byte code, int parNumber, int parValue)
            : base(carid, magic)
        {
            m_Code1 = code;
            m_Par1Number = parNumber;
            m_Par1Value = parValue;
        }
        /// <summary>
        /// OM-PLC ">"
        /// Two parameters
        /// </summary>
        /// <param name="carid"></param>
        /// <param name="magic"></param>
        /// <param name="code1"></param>
        /// <param name="par1Number"></param>
        /// <param name="par1Value"></param>
        /// <param name="code2"></param>
        /// <param name="par2Number"></param>
        /// <param name="par2Value"></param>
        public Message_hpil_word(int carid, int magic, byte code1, int par1Number, int par1Value, byte code2 ,int par2Number, int par2Value)
            : base(carid, magic)
        {
            m_Code1 = code1;
            m_Par1Number = par1Number;
            m_Par1Value = par1Value;
            m_Code2 = code2;
            m_Par2Number = par2Number;
            m_Par2Value = par2Value;
            m_IsPair = true;
        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                ACI_MSG_HPIL_WORD msg = new ACI_MSG_HPIL_WORD();
                int size = 0;

                msg.head.carid = shiftBytes((ushort)m_CarId);
                msg.head.magic = shiftBytes((ushort)m_Magic);
                size += Marshal.SizeOf(msg.head.carid) + Marshal.SizeOf(msg.head.magic);

                msg.par = new ACI_MSG_HPIL_WORD_STRUCT[2];

                msg.par[0].code = m_Code1;
                msg.par[0].spare = 0;
                msg.par[0].omlp = 0;
                msg.par[0].plclp = (byte)m_Par1Number;
                msg.par[0].val = shiftBytes((ushort)m_Par1Value);
                size += Marshal.SizeOf(msg.par[0]);

                if(m_IsPair)
                {
                    msg.par[1].code = m_Code2;
                    msg.par[1].spare = 0;
                    msg.par[1].omlp = 0;
                    msg.par[1].plclp = (byte)m_Par2Number;
                    msg.par[1].val = shiftBytes((ushort)m_Par2Value);
                    size += Marshal.SizeOf(msg.par[1]);
                }

                return new MsgBuffer(structToBuffer<ACI_MSG_HPIL_WORD>(msg), size);
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_HPIL_WORD)));
            }
        }
    }

    public class Message_hpil_multi : Message_hpil
    {
        private byte m_Code;
        private int m_Plclp;
        private byte m_Save;
        private byte m_Num;
        private List<byte> m_Data;

        /// <summary>
        /// Multi byte OM-PLC ">"
        /// </summary>
        /// <param name="carid"></param>
        /// <param name="magic"></param>
        /// <param name="code"></param>
        /// <param name="plclp"></param>
        /// <param name="data" when read this is null></param>
        public Message_hpil_multi(int carid, int magic, byte code, byte s3, int plclp, byte num, List<byte> data)
            : base(carid, magic)
        {
            m_Code = code;
            m_Plclp = plclp;
            m_Data = data;
            m_Save = s3;
            m_Num = num;
        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                ACI_MSG_HPIL_MULTI msg = new ACI_MSG_HPIL_MULTI();

                msg.head.carid = shiftBytes((ushort)m_CarId);
                msg.head.magic = shiftBytes((ushort)m_Magic);
                msg.code = m_Code;
                msg.save = m_Save;
                msg.plclp = shiftBytes((ushort)m_Plclp);
                msg.num = m_Num;
                msg.spare = 0;

                byte[] data = structToBuffer<ACI_MSG_HPIL_MULTI>(msg);

                if(m_Data != null)
                {
                    data = data.Concat(m_Data).ToArray();
                }

                return new MsgBuffer(data, data.Count<byte>());

            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer of type '{0}'", typeof(ACI_MSG_HPIL_MULTI)));
            }
        }
    }
}
