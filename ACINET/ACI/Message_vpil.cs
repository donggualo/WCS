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
    public enum VPIL_TYPE
    {
        Word,
        Multi
    }

    public enum VPIL_CODE : byte
    {
        VPIL_ACKRD = 1,
        VPIL_ACKWR = 2,
        VPIL_NAKRD = 3,
        VPIL_NAKWR = 4,
        VPIL_ACKRDMU = 5,
        VPIL_ACKWRMU = 6,
        VPIL_NAKRDMU = 7,
        VPIL_NAKWRMU = 8,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_VPIL_HEAD
    {
        public ushort carid;           // carrier #
        public ushort magic;           // message magic
        public byte code;              // parameter code
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_VPIL_WORD
    {
        public ushort carid;           // carrier #
        public ushort magic;           // message magic
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ACI_MSG_VPIL_WORD_DETAILS[] par;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_VPIL_WORD_DETAILS
    {
        public byte code;               //code 
        public byte spare;             // parameter spare
        public byte omlp;              // OM LP address
        public byte plclp;             // PLC address
        public ushort val;             // parameter value
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_VPIL_MULTI
    {
        public ushort carid;           // carrier #
        public ushort magic;           // message magic
        public byte code;              // parameter code
        public byte save;              // same in reply
        public ushort plclp;           // PLC address
        public byte seq;               // sequense #, 0...
        public byte last;              // last sequense #, 0...
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        public byte[] data;           // bytes (read multi)
    }

    public class Message_vpil : ACIMessageBase
    {

        public Message_vpil(byte[] msg)
            : base("<")
        {
            ACI_MSG_VPIL_HEAD msg_vpil = BufferToStruct<ACI_MSG_VPIL_HEAD>(msg);

            Code = msg_vpil.code;

            if(Code <= (byte)VPIL_CODE.VPIL_NAKWR)
            {
                ACI_MSG_VPIL_WORD msg_vpil_word = BufferToStruct<ACI_MSG_VPIL_WORD>(msg);

                Type = VPIL_TYPE.Word;

                CarId = shiftBytes(msg_vpil_word.carid);

                Magic = shiftBytes(msg_vpil_word.magic);

                Code1 = msg_vpil_word.par[0].code;

                OmLp1 = msg_vpil_word.par[0].omlp;

                PlcLp1 = msg_vpil_word.par[0].plclp;

                Value1 = shiftBytes(msg_vpil_word.par[0].val);

                Code2 = msg_vpil_word.par[1].code;

                OmLp2 = msg_vpil_word.par[1].omlp;

                PlcLp2 = msg_vpil_word.par[1].plclp;

                Value2 = shiftBytes(msg_vpil_word.par[1].val);

            }
            else
            {
                ACI_MSG_VPIL_MULTI msg_vpil_multi = BufferToStruct<ACI_MSG_VPIL_MULTI>(msg);

                Type = VPIL_TYPE.Multi;

                CarId = shiftBytes(msg_vpil_multi.carid);

                Magic = shiftBytes(msg_vpil_multi.magic);

                Save = msg_vpil_multi.save; // same in reply

                PlcLp = shiftBytes(msg_vpil_multi.plclp);           // PLC address

                Sequence = msg_vpil_multi.seq;               // sequense #, 0...

                Last = msg_vpil_multi.last;              // last sequense #, 0...

                MultiByte = msg_vpil_multi.data;           // bytes (read multi)

            }
        }

        public new VPIL_TYPE Type
        {
            get; private set;
        }

        public int CarId
        {
            get; private set;
        }

        public int Magic
        {
            get; private set;
        }

        public byte Code
        {
            get; private set;
        }
        public byte Code1
        {
            get; private set;
        }

        public byte OmLp1
        {
            get; private set;
        }

        public byte PlcLp1
        {
            get; private set;
        }

        public int Value1
        {
            get; private set;
        }
        public byte Code2
        {
            get; private set;
        }
        public byte OmLp2
        {
            get; private set;
        }

        public byte PlcLp2
        {
            get; private set;
        }

        public int Value2
        {
            get; private set;
        }
        public byte Save
        {
            get; private set;
        }

        public int PlcLp
        {
            get; private set;
        }

        public byte Sequence
        {
            get; private set;
        }
        public byte Last
        {
            get; private set;
        }
        public byte[] MultiByte
        {
            get; private set;
        }
    }

}
