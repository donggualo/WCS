using Module;
using Socket.module;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using ToolManager;

namespace Socket
{
    /// <summary>
    /// Socket客户端
    /// </summary>
    public class SocketClient : IClientBase
    {
        #region[定义]
        public delegate void ConnectionEventHandler(string host, int port);
        public delegate void ReciveDataHandler(string devName, DevType head, IBaseModule module);

        // 通知
        public event ConnectionEventHandler Connected;
        public event ConnectionEventHandler Disconnected;
        public event ReciveDataHandler ReceiveData;

        // 指令
        internal byte[] ReOrder;
        internal byte[] ExOrder;
        private bool IsClearAfterOrder;

        #endregion

        #region[构造方法]

        public SocketClient(string dev, string ip, int port, byte[] order) : base()
        {
            m_DevName = dev;
            m_IP = ip;
            m_Port = port;

            ReOrder = order;

            log = new Log(m_DevName);

            Connect(m_IP, m_Port);
        }

        public bool SendMessage(byte[] msg, bool isCycling)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (msg == null)
            {
                string logMessage = "Cannot send empty message";
                throw new ArgumentNullException(logMessage);
            }

            ExOrder = msg;
            IsClearAfterOrder = !isCycling;
            return true;
        }

        #endregion

        #region[通知方法]

        /// <summary>
        /// 通知成功
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        internal override void NoticesConnect(string host, int port)
        {
            if (Connected != null)
            {
                try
                {
                    Connected(host, port);
                }
                catch (Exception ex)
                {
                    // don't care about error
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 通知信息接收
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        internal override void NoticeDataReceive(DevType head, byte[] data)
        {
            ReciveDataHandler tmp = ReceiveData;
            if (tmp != null)
            {
                try
                {
                    IBaseModule message = MessageParser.Parse(head, data);

                    tmp(m_DevName, head, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 通知断开连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        internal override void NoticesDisConnect(string host, int port)
        {
            if (Disconnected != null)
            {
                try
                {
                    Disconnected(host, port);
                }
                catch (Exception ex)
                {
                    // don't care about error
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            }
        }

        #endregion

        #region[连接成功和数据处理]

        /// <summary>
        /// 成功连接
        ///     1.接收数据
        ///     2.处理数据
        ///     3.发送数据
        /// </summary>
        /// <param name="ar"></param>
        internal override void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                m_Client.EndConnect(ar);

                m_Stream = m_Client.GetStream();

                m_Connected = true;

                // 连接成功，开始异步读取数据
                byte[] buffer = new byte[ISocketConst.BUFFER_SIZE];
                m_Client.GetStream().BeginRead(buffer, 0, buffer.Length, ReceiverHandler, buffer);

                NoticesConnect(m_IP, m_Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                m_RetryTimer = new Timer(delegate (object state)
                {
                    m_RetryTimer = null;
                    Connect(m_IP, m_Port);
                }, null, CONNECTION_RETRY_TIMEOUT, 0);
            }
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        private void ReceiverHandler(IAsyncResult ar)
        {
            try
            {
                if (!IsConnected)
                {
                    string logMessage = "Cannot start receiver - client not started";
                    throw new InvalidOperationException(logMessage);
                }

                byte[] bufferData = null;
                byte[] buffer = new byte[ISocketConst.BUFFER_SIZE];

                while (IsConnected)
                {
                    int bytesRead = m_Stream.Read(buffer, 0, ISocketConst.BUFFER_SIZE);
                    if (bytesRead == 0)
                    {
                        throw new IOException("No Data!");
                    }
                    byte[] readData = buffer.Take(bytesRead).ToArray();
                    if (bufferData != null && bufferData.Length > 0)
                    {
                        readData = bufferData.Concat(readData).ToArray();
                    }

                    // make sure we at least have one header
                    while (readData.Count() > ISocketConst.HEADTAIL_SIZE)
                    {
                        log.LOG("Read: " + BitConverter.ToString(readData));
                        DevType head = (DevType)BitConverter.ToUInt16(ShiftBytes(readData, 0, 2), 0);
                        int size;
                        switch (head)
                        {
                            case DevType.固定辊台:
                                size = ISocketConst.FRT_SIZE;
                                break;
                            case DevType.摆渡车:
                                size = ISocketConst.ARF_SIZE;
                                break;
                            case DevType.运输车:
                                size = ISocketConst.RGV_SIZE;
                                break;
                            case DevType.行车:
                                size = ISocketConst.AWC_SIZE;
                                break;
                            case DevType.包装线辊台:
                                size = ISocketConst.PKL_SIZE;
                                break;
                            default:
                                size = 0;
                                break;
                        }

                        if (size == 0)
                        {
                            throw new IOException("Header key did not match!");
                        }

                        if (readData.Count() < size)
                        {
                            throw new IOException("messagesize is small than" + size + " （" + head.ToString() + "） !");
                        }

                        ushort tailKey = BitConverter.ToUInt16(ShiftBytes(readData, size - 2, 2), 0);
                        if (tailKey == ISocketConst.TAIL_KEY)
                        {
                            byte[] data = new byte[size];
                            Array.Copy(readData, 0, data, 0, size);
                            NoticeDataReceive(head, data);
                        }

                        // remove from data array
                        readData = readData.Skip(size).ToArray();
                    }
                    // save until next round
                    bufferData = readData;
                }
            }
            catch (IOException e)
            {
                // unclean disconnect from service
                Reconnect();
                Console.WriteLine(e.Message + e.StackTrace);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void SenderHandler()
        {
            try
            {
                if (!IsConnected)
                {
                    //string logMessage = "Cannot start sender - client not started";
                    //throw new InvalidOperationException(logMessage);
                    return;
                }

                byte[] data = ExOrder ?? ReOrder;
                if (IsClearAfterOrder && ExOrder != null) ExOrder = null;

                if (data != null && data.Count() > 0)
                {
                    m_Client.GetStream().BeginWrite(data, 0, data.Length, HandleDatagramWritten, m_Client);
                    log.LOG("Send: " + BitConverter.ToString(data));
                }
            }
            catch (Exception ex)
            {
                log.LOG(ex);
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 发送数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
        }

        #endregion
    }
}
