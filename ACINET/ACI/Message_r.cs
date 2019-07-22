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
    public struct ACI_MSG_r
    {
        public ushort index;        // order index
        public byte parno;          // LP offset, -1 if not used
    }

    public class Message_r : ACIMessageBase
    {

        public Message_r(byte[] data) : base("r")                      //(ACI_MSG msg) : base(msg)
        {
            ACI_MSG_r msg_r = BufferToStruct<ACI_MSG_r>(data);

            Index = shiftBytes(msg_r.index);

            ParNo = msg_r.parno;
        }

        public int Index
        {
            get; private set;
        }

        public int ParNo
        {
            get; private set;
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', Index '{1}', ParNo '{4}' - '{6}'", Type, Index, ParNo, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
