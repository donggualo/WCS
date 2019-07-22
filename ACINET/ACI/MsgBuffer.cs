using System;
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
    public struct MsgBuffer
    {
        public byte[] Buffer;
        public UInt16 Size;

        public MsgBuffer(byte[] buffer)
        {
            Buffer = buffer;
            Size = Convert.ToUInt16(buffer.Length);
        }

        public MsgBuffer(byte[] buffer, int size)
        {
            Buffer = buffer;
            Size = Convert.ToUInt16(size);
        }
    }
}
