using System;
using System.Diagnostics;
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
    public static class MessageParser
    {
        public static IACIMessage Parse(ushort type, byte[] msg)
        {
            try
            {
                switch((char)type)
                {
                    case 's':
                        return new Message_s(msg);

                    case 'b':
                        return new Message_b(msg);

                    case 'E':
                        return new Message_E(msg);

                    case 'o':
                        return new Message_o(msg);

                    case 'w':
                        return new Message_w(msg);

                    case 'r':
                        return new Message_r(msg);

                    case '<':
                        return new Message_vpil(msg);

                    case 'p':
                        return new Message_p(msg);

                    default:
                        return new Message_unknown((char)type, msg);
                }
            }
            catch(Exception ex)
            {
                // throw new Exception(string.Format("Error while creating message buffer for '{0}'", tring()));
                Trace.TraceWarning(string.Format("Failed to parse message {0}: {1}", (char)type, ex.Message));
                return null;
            }
        }
    }
}
