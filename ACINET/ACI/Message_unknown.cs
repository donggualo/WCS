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
    public class Message_unknown : ACIMessageBase
    {
        private const int BUFFER_SIZE = 4096;
        private byte[] m_Data;


        public Message_unknown(char type, byte[] data)
           : base(type.ToString())
        {
            //make sure data is kept below maximum
            if(data.Length > BUFFER_SIZE)
            {
                m_Data = new byte[BUFFER_SIZE];

                Buffer.BlockCopy(data, 0, m_Data, 0, data.Length);
            }
            else
            {
                m_Data = data;
            }
        }
        public byte[] Data
        {
            get
            {
                return m_Data;
            }
        }

        public override MsgBuffer ToAciMsgBuffer()
        {
            try
            {
                return new MsgBuffer(m_Data);
            }
            catch
            {
                throw new Exception(string.Format("Error while creating message buffer for '{0}'", m_Data.ToString()));
            }
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}' - '{1}'", Type, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
