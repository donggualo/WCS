using System;
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
    public abstract class ACIMessageBase : IACIMessage
    {
        private const int BUFFER_SIZE = 4096;
        protected byte[] m_Msg;
        protected readonly string m_Type;
        protected readonly DateTime m_CreatedUTC;

        public ACIMessageBase(string type)
        {
            m_Type = type;
            m_Msg = new byte[BUFFER_SIZE];
            m_CreatedUTC = DateTime.Now;
        }

        public string Type
        {
            get
            {
                return m_Type;
            }
        }

        public DateTime CreatedUTC
        {
            get
            {
                return m_CreatedUTC;
            }
        }


        public override string ToString()
        {
            return string.Format("Message of type '{0}' - '{1}'", Type, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }

        public virtual MsgBuffer ToAciMsgBuffer()
        {
            throw new NotImplementedException();
        }

        protected T BufferToStruct<T>(byte[] buffer)
        {
            GCHandle packet = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T msg = (T)Marshal.PtrToStructure(packet.AddrOfPinnedObject(), typeof(T));
            packet.Free();

            return msg;
        }

        protected byte[] structToBuffer<T>(T msg)
        {
            int size = Marshal.SizeOf(msg);

            return StructToBuffer<T>(msg, size);
        }

        protected byte[] StructToBuffer<T>(T msg, int size)
        {
            byte[] data = new byte[size];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(msg, handle.AddrOfPinnedObject(), false);
            }
            catch(Exception)
            {
                // don't handle error
            }
            finally
            {
                handle.Free();
            }
            return data;
        }

        protected UInt16 shiftBytes(ushort value)
        {
            byte[] b = BitConverter.GetBytes(value);

            return BitConverter.ToUInt16(new byte[] { b[1], b[0] }, 0);
        }

        protected Int32 shiftBytes(Int32 value)
        {
            byte[] b = BitConverter.GetBytes(value);

            return BitConverter.ToInt32(new byte[] { b[3], b[2], b[1], b[0] }, 0);
        }

        protected UInt32 shiftBytes(UInt32 value)
        {
            byte[] b = BitConverter.GetBytes(value);
            return BitConverter.ToUInt32(new byte[] { b[3], b[2], b[1], b[0] }, 0);
            
        }

    }
}
