using Module;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ToolManager;

namespace SocketManager.module
{
    public abstract class IClientBase : IDisposable
    {

        #region[参数定义]

        internal const int CONNECTION_TIMEOUT = 10 * 1000;
        internal const int CONNECTION_RETRY_TIMEOUT = 5 * 1000;
        internal const int MESSAGE_RESEND_TIMEOUT = 5 * 1000;

        /// <summary>
        /// 客户端自己的标识
        /// </summary>
        internal TcpClient m_Client;
        internal string m_DevName;
        internal string m_IP;
        internal int m_Port;
        internal bool m_Connected;
        internal bool m_isScan; // 扫码器

        /// <summary>
        /// 是否激活
        /// </summary>
        internal bool m_Userful = true;

        internal NetworkStream m_Stream;

        internal Timer m_RetryTimer;

        internal Log log;

        #endregion

        #region[构造方法]

        public IClientBase() : base()
        {
        }

        #endregion

        #region[连接断开]

        public bool IsConnected
        {
            get
            {
                return m_Client != null && m_Connected;
            }
        }

        internal void Connect(string ip, int port)
        {
            try
            {
                if (!m_Userful) return;

                m_Client = new TcpClient();
                m_Client.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception ex)
            {
                log.LOG(ex);
                //Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        internal void Reconnect()
        {
            m_RetryTimer = new Timer(delegate (object state)
            {
                m_RetryTimer = null;
                Disconnect();
                Connect(m_IP, m_Port);
            }, null, CONNECTION_RETRY_TIMEOUT, 0);
        }

        private void Disconnect()
        {
            if (m_Client != null)
            {
                m_Client.Close();
                m_Client = null;
            }

            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }

            m_Connected = false;

            NoticesDisConnect(m_IP, m_Port);
        }

        public void Close()
        {
            try
            {
                // stop timer
                if (m_RetryTimer != null)
                {
                    m_RetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                Disconnect();
                log.LOG("Close: 关闭连接！");
            }
            catch (Exception ex)
            {
                log.LOG(ex);
                throw ex;
            }
        }

        #endregion

        internal byte[] ShiftBytes(byte[] buffer, int offset, int size)
        {
            return buffer.Skip(offset).Take(size).Reverse().ToArray();
        }

        public void Dispose()
        {
            Close();
        }

        #region[抽象方法]

        internal abstract void NoticesConnect(string host, int port);

        internal abstract void NoticesDisConnect(string host, int port);

        internal abstract void NoticeDataReceive(DevType head, byte[] data);

        internal abstract void ConnectCallback(IAsyncResult ar);

        #endregion
    }
}
