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
    public struct ACI_MSG_b
    {
        public ushort index;        // order index
        public byte trpsno;         // transport structure #
        public byte status;         // message status
        public byte parno;          // LP offset, -1 if not used
        public byte sp;             // spare = 0
        public ushort ikey;         // for order ack, optional !
    }

    public class Message_b : ACIMessageBase
    {

        public Message_b(byte[] data)
            : base("b")                      //(ACI_MSG msg) : base(msg)
        {
            ACI_MSG_b msg_b = BufferToStruct<ACI_MSG_b>(data);

            Index = shiftBytes(msg_b.index);

            TransportStructure = msg_b.trpsno;
            
            Status = msg_b.status;

            ParNo = msg_b.parno;

            IKEY = shiftBytes(msg_b.ikey);
        }

        public int Index
        {
            get; private set;
        }
  
        public int TransportStructure
        {
            get; private set;
        }

        public int Status
        {
            get; private set;
        }

        public int ParNo
        {
            get; private set;
        }

        public int IKEY
        {
            get; private set;
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', Index '{1}', TS '{2}', Status '{3}', ParNo '{4}', IKEY '{5}' - '{6}'", Type, Index, TransportStructure, Status, ParNo, IKEY, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
