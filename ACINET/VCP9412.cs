using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
    /*******************/
    /* Header key      */
    // 0x87CD
    /*******************/
    /* Size of header  */
    // 8 bytes
    /*******************/
    /* Size of message */
    // 0 - 128 (padded even)
    /*******************/
    /* Function code   */
    // 1 = Normal message, 2 = Disconnect, 3 = Reserved, 4 = Heartbeat poll, 5 = Heartbeat ACK
    /*******************/
    /* Message part    */
    // ACI_MSG
    /*******************/

    public class VCP9412 : IDisposable
    {
        private const int CONNECTION_TIMEOUT = 10 * 1000;
        private const int CONNECTION_RETRY_TIMEOUT = 5 * 1000;
        private const int MESSAGE_RESEND_TIMEOUT = 5 * 1000;
        private const int BUFFER_SIZE = 4096;
        private const int HEADER_KEY = 0x87CD;
        private const int HEADER_SIZE = 8;

        private static VCP9412 m_Instance;

        private TcpClient m_Client;
        private string m_IP;
        private int m_Port;
        private bool m_Connected;

        private NetworkStream m_Stream;

        private ManualResetEvent m_StopSignal;
        private AutoResetEvent m_EventSignal;
        private AutoResetEvent m_QueueHandle;

        private Thread m_ReaderThread;
        private Thread m_SenderThread;
        private Timer m_RetryTimer;

        public event ConnectionEventHandler Connected;
        public event ConnectionEventHandler Disconnected;
        internal event NewDataEventHandler NewData;
        public delegate void ConnectionEventHandler(string host, int port);
        internal delegate void NewDataEventHandler(ushort type, byte[] data);

        private Queue<IACIMessage> m_DataQueue;
        private Queue<byte[]> m_ReceiveQueue;

        public delegate void ReciveDataHandler(IACIMessage msg);
        public event ReciveDataHandler ReciveData;

        private VCP9412()
        {
        }

        public static VCP9412 Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    m_Instance = new VCP9412();
                }
                return m_Instance;
            }
        }

        public AutoResetEvent EventHandle
        {
            get
            {
                if(m_EventSignal == null)
                {
                    m_EventSignal = new AutoResetEvent(false);
                }

                return m_EventSignal;
            }
        }

        public bool IsConnected
        {
            get
            {
                return m_Client != null && m_Connected;
            }
        }

        public bool Open(string host, int port)
        {
            try
            {
                if(IsConnected)
                {
                    string logMessage = "VCP9412 client already started";
                    throw new InvalidOperationException(logMessage);
                }

                if(string.IsNullOrEmpty(host))
                {
                    string logMessage = string.Format("VCP9412 server ip not valid: '{0}'", host);
                    throw new ArgumentNullException(logMessage);
                }

                if(port <= 0 || port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                {
                    string logMessage = string.Format("VCP9412 server port not valid: '{0}'", port);
                    throw new ArgumentOutOfRangeException(logMessage);
                }

                m_IP = host;
                m_Port = port;
                m_DataQueue = new Queue<IACIMessage>();
                m_QueueHandle = new AutoResetEvent(false);
                m_ReceiveQueue = new Queue<byte[]>();

                this.NewData += VCP9412_NewData;

                // connect on new thread
                new Thread(() =>
                {
                    Connect(m_IP, m_Port);
                }).Start();
            }
            catch(Exception ex)
            {

                Close();
                throw ex;
            }

            return true;
        }


        public void Close()
        {
            try
            {
                // stop timer
                if(m_RetryTimer != null)
                {
                    m_RetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                Disconnect();

                this.NewData -= VCP9412_NewData;

                if(m_ReceiveQueue != null)
                {
                    m_ReceiveQueue.Clear();
                    m_ReceiveQueue = null;
                }

                if(m_DataQueue != null)
                {
                    m_DataQueue.Clear();
                    m_DataQueue = null;
                }

                if(m_QueueHandle != null)
                {
                    m_QueueHandle = null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void VCP9412_NewData(ushort type, byte[] data)
        {
            ReciveDataHandler tmp = ReciveData;
            if(tmp != null)
            {
                IACIMessage message = MessageParser.Parse(type, data);
                try
                {
                    tmp(message);
                }
                catch(Exception)
                {

                    throw;
                }
            }
        }

        public bool SendMessage(IACIMessage msg)
        {
            if(m_DataQueue == null)
            {
                string logMessage = "Queue not initialized";
                throw new ArgumentNullException(logMessage);
            }

            if(msg == null)
            {
                string logMessage = "Cannot send empty message";
                throw new ArgumentNullException(logMessage);
            }


            lock(m_DataQueue)
            {
                m_DataQueue.Enqueue(msg);
            }

            // signal that new data is available
            m_QueueHandle.Set();

            return true;
        }

        private void Connect(string ip, int port)
        {
            m_Client = new TcpClient();
            m_Client.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), null);
        }

        private void Reconnect()
        {            
            // important to disconnect within a timer (different thread) since the call came from
            // one of the threads being closed in the Disconnect() method
            m_RetryTimer = new Timer(delegate (object state)
            {
                m_RetryTimer = null;
                Disconnect();
                Connect(m_IP, m_Port);
            }, null, CONNECTION_RETRY_TIMEOUT, 0);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                m_Client.EndConnect(ar);

                m_Stream = m_Client.GetStream();

                m_StopSignal = new ManualResetEvent(false);
                m_Connected = true;

                m_ReaderThread = new Thread(new ThreadStart(ReceiverHandler));
                m_ReaderThread.Name = "ClientBaseReceiver";
                m_ReaderThread.Start();

                m_SenderThread = new Thread(new ThreadStart(SenderHandler));
                m_SenderThread.Name = "ClientBaseSender";
                m_SenderThread.Start();

                m_SenderThread = new Thread(new ThreadStart(ReciveEventHandler));
                m_SenderThread.Name = "ClientEventReciver";
                m_SenderThread.Start();

                OnConnected(m_IP, m_Port);
            }
            catch
            {
                m_RetryTimer = new Timer(delegate (object state)
                {
                    m_RetryTimer = null;
                    Connect(m_IP, m_Port);
                }, null, CONNECTION_RETRY_TIMEOUT, 0);
            }
        }

        private void Disconnect()
        {
            if(m_Client != null)
            {
                m_Client.Close();
                m_Client = null;
            }

            if(m_StopSignal != null)
            {
                m_StopSignal.Set();
            }

            if(m_ReaderThread != null)
            {
                m_ReaderThread.Join();
                m_ReaderThread = null;
            }

            if(m_SenderThread != null)
            {
                m_SenderThread.Join();
                m_SenderThread = null;
            }

            if(m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }

            m_Connected = false;

            OnDisconnected(m_IP, m_Port);
        }

        private void ReceiverHandler()
        {
            try
            {
                if(!IsConnected)
                {
                    string logMessage = "Cannot start receiver - client not started";
                    throw new InvalidOperationException(logMessage);
                }

                byte[] bufferData = null;

                byte[] buffer = new byte[BUFFER_SIZE];

                while(!m_StopSignal.WaitOne(0, false))
                {
                    try
                    {

                        int bytesRead = m_Stream.Read(buffer, 0, BUFFER_SIZE);
                        if(bytesRead == 0)
                        {
                            Reconnect();
                            break;
                        }

                        byte[] readData = buffer.Take(bytesRead).ToArray();

                        // if we have remaining data from a previous read operation concatenate with newly read data
                        if(bufferData != null && bufferData.Length > 0)
                        {
                            readData = bufferData.Concat(readData).ToArray();
                        }

                        // make sure we at least have one header
                        while(readData.Count() >= 8)
                        {
                            ushort headerKey = BitConverter.ToUInt16(ShiftBytes(readData, 0, 2), 0);
                            if(headerKey != HEADER_KEY)
                            {
                                throw new IOException("Header key did not match!");
                            }

                            ushort headerSize = BitConverter.ToUInt16(ShiftBytes(readData, 2, 2), 0);
                            if(headerSize != HEADER_SIZE)
                            {
                                throw new IOException("Header size did not match!");
                            }

                            ushort messageSize = BitConverter.ToUInt16(ShiftBytes(readData, 4, 2), 0);
                            ushort functionCode = BitConverter.ToUInt16(ShiftBytes(readData, 6, 2), 0);

                            // normal message
                            if(functionCode == 1)
                            {
                                // do we have a complete message
                                if(readData.Count() >= (headerSize + messageSize))
                                {
                                    // Put in receive queue
                                    m_ReceiveQueue.Enqueue(readData);

                                    // remove from data array
                                    readData = readData.Skip(headerSize + messageSize).ToArray();
                                }
                            }
                            // disconnect, reserved, heart-beat poll or heart-beat ACK
                            else
                            {
                                // remove from data array
                                readData = readData.Skip(headerSize).ToArray();
                            }
                        }

                        // save until next round
                        bufferData = readData;
                    }
                    catch(IOException)
                    {
                        // unclean disconnect from service
                        Reconnect();
                        break;
                    }
                    catch
                    {
                        // don't handle error, just wait for end signal
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void ReciveEventHandler()
        {
            try
            {
                List<WaitHandle> waitHandles = new List<WaitHandle>();
                waitHandles.Add(m_StopSignal);
                waitHandles.Add(m_QueueHandle);

                while(!m_StopSignal.WaitOne(0, false))
                {
                    try
                    {
                        while(m_ReceiveQueue.Count() > 0)
                        {
                            byte[] readData = new byte[4096];
                            lock(m_ReceiveQueue)
                            {
                                readData = m_ReceiveQueue.Peek();
                            }

                            if(readData.Length == 0)
                            {
                                continue;
                            }

                            ushort messageSize = BitConverter.ToUInt16(ShiftBytes(readData, 4, 2), 0);

                            // shift/reverse
                            Array.Reverse(readData, 8, 2);  // message type
                            Array.Reverse(readData, 10, 2); // num par

                            // read num par to get size of telegram
                            int telegramSize = BitConverter.ToUInt16(readData, 10);

                            // get message part and type of data array
                            byte[] messageData = readData.Skip(HEADER_SIZE + 4).Take(telegramSize).ToArray();
                            ushort messageType = BitConverter.ToUInt16(readData, HEADER_SIZE);

                            // send event köa upp?
                            OnNewData(messageType, messageData);

                            lock(m_ReceiveQueue)
                            {
                                m_ReceiveQueue.Dequeue();
                            }
                        }
                    }
                    catch
                    {
                        // don't handle error, just wait for signal
                    }

                    // wait for signal
                    WaitHandle.WaitAny(waitHandles.ToArray(), MESSAGE_RESEND_TIMEOUT);
                }
            }
            catch(Exception ex)
            {
                if(ex.InnerException != null)
                    throw ex;
            }

        }

        private void SenderHandler()
        {
            try
            {
                if(!IsConnected)
                {
                    string logMessage = "Cannot start sender - client not started";
                    //                    Logger.Instance.Log(logMessage, TraceEventType.Error);
                    throw new InvalidOperationException(logMessage);
                }

                List<WaitHandle> waitHandles = new List<WaitHandle>();
                waitHandles.Add(m_StopSignal);
                waitHandles.Add(m_QueueHandle);

                while(!m_StopSignal.WaitOne(0, false))
                {
                    try
                    {
                        while(m_DataQueue.Count() > 0)
                        {
                            IACIMessage msg = null;
                            lock(m_DataQueue)
                            {
                                msg = m_DataQueue.Peek();
                            }

                            MsgBuffer msgBuffer = msg.ToAciMsgBuffer();
                            if(msgBuffer.Buffer == null)
                            {
                                continue;
                            }

                            int headSize = 4;
                            int messageSize = headSize + msgBuffer.Size + (msgBuffer.Size % 2);

                            byte[] messageData = ShiftBytes(
                                new byte[] { Encoding.Default.GetBytes(msg.Type)[0], new byte() }, 0, 2)    // type
                                .Concat(ShiftBytes(BitConverter.GetBytes(msgBuffer.Size), 0, 2))            // num par
                                .Concat(msgBuffer.Buffer)                                                   // data
                                .ToArray();

                            // pad to even bytes
                            if((msgBuffer.Size % 2) == 1)
                            {
                                messageData = messageData.Concat(new byte[1]).ToArray();
                            }

                            byte[] data = ShiftBytes(BitConverter.GetBytes(HEADER_KEY), 0, 2)   // headerKey
                                .Concat(ShiftBytes(BitConverter.GetBytes(HEADER_SIZE), 0, 2))   // headerSize
                                .Concat(ShiftBytes(BitConverter.GetBytes(messageSize), 0, 2))   // message size
                                .Concat(ShiftBytes(BitConverter.GetBytes(1), 0, 2))             // function code
                                .Concat(messageData)                                            // message data
                                .ToArray();

                            if(data != null && data.Count() > 0)
                            {
                                m_Stream.Write(data, 0, data.Count());
                                m_Stream.Flush();
                            }

                            lock(m_DataQueue)
                            {
                                m_DataQueue.Dequeue();
                            }
                        }
                    }
                    catch
                    {
                        // don't handle error, just wait for signal
                    }

                    // wait for signal
                    WaitHandle.WaitAny(waitHandles.ToArray(), MESSAGE_RESEND_TIMEOUT);
                }
            }
            catch(Exception ex)
            {
                if(ex.InnerException != null)
                    throw ex;
            }
        }

        private void OnConnected(string host, int port)
        {
            if(Connected != null)
            {
                try
                {
                    Connected(host, port);
                }
                catch
                {
                    // don't care about error
                }
            }
        }

        private void OnDisconnected(string host, int port)
        {
            if(Disconnected != null)
            {
                try
                {
                    Disconnected(host, port);
                }
                catch
                {
                    // don't care about error
                }
            }
        }

        private void OnNewData(ushort type, byte[] data)
        {
            if(NewData != null)
            {
                try
                {
                    NewData(type, data);
                }
                catch
                {
                    // don't care about error
                }
            }
        }

        private byte[] ShiftBytes(byte[] buffer, int offset, int size)
        {
            return buffer.Skip(offset).Take(size).Reverse().ToArray();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
