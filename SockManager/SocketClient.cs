using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ToolManager;
namespace Socket
{
    /// <summary>
    /// 设备
    /// </summary>
    class SocketClient
    {
        internal byte[] Bdata;//字节数据
        internal byte[] RefreshByte;//用于查询的数据
        internal string Sdata;//字符串数据
        internal DateTime UpDateTime;
        internal bool IsAlive;
        internal bool DoRefresh = true;//

        private string IP;//服务端IP
        private int Port;//服务端端口

        public string Name;//设备名称
        private AsyncTcpClient client;//TCP连接
        private bool doCloseSocket = false;//是否是主动关闭连接/还是意外断开
        private Log log;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        public SocketClient(string ip, int port, string name,byte[] refreshB)
        {
            IP = ip;
            Port = port;
            Name = name;
            RefreshByte = refreshB;

            ConnectToService();

            log = new Log(name);
        }

        /// <summary>
        /// 发送设备查询信息
        /// </summary>
        public void Refresh()
        {
            if (!IsConnect()) return;
            if (!DoRefresh) return;
            Send(RefreshByte);
        }

        /// <summary>
        /// 是否连接服务
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            if (client == null || !client.Connected)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (client != null)
            {
                doCloseSocket = true;
                client.Close();
            }
        }

        /// <summary>
        /// 连接到服务端
        /// </summary>
        public void ConnectToService()
        {
            try
            {
                client = new AsyncTcpClient(IPAddress.Parse(IP), Port);
                client.ServerConnected += Client_ServerConnected;//连接成功
                client.PlaintextReceived += Client_PlaintextReceived; //文本接收
                client.DatagramReceived += Client_DatagramReceived; //字节接收
                client.ServerDisconnected += Client_ServerDisconnected;//断开连接
                client.ServerExceptionOccurred += Client_ServerExceptionOccurred;//
                client.Connect();
            }catch(Exception e)
            {
                Console.WriteLine("[连接错误]IP:" + IP + ":" + Port + e.Message);
            }

        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="text"></param>
        internal void Send(string text)
        {
            client.Send(text);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="text"></param>
        internal void Send(byte[] msg)
        {
            client.Send(msg);
            log.LOG("S:" + CRCMethod.AllByteToString(msg));
        }

        /// <summary>
        /// 连接异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ServerExceptionOccurred(object sender, TcpServerExceptionOccurredEventArgs e)
        {
            Console.WriteLine("Client_ServerExceptionOccurred");
        }

        /// <summary>
        /// 服务断开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            //Console.WriteLine(Name + "：服务器断开");
            IsAlive = false;
            if (!doCloseSocket) ConnectToService();
            
        }

        /// <summary>
        /// 接收字节数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            //_master.UpdateDevceBData(Name, e.Datagram);
            if (CRCMethod.CheckCRC(e.Datagram))
            {
                Bdata = e.Datagram;
                UpDateTime = DateTime.Now;
                log.LOG("R:" + CRCMethod.AllByteToString(e.Datagram));
            }
            else
            {
                log.LOG("RN:" + CRCMethod.AllByteToString(e.Datagram));
            }

        }

        /// <summary>
        /// 接收文本数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            //_master.UpdateDevceSData(Name, e.Datagram);
            Sdata = e.Datagram;
            UpDateTime = DateTime.Now;
        }

        /// <summary>
        /// 服务连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            Console.WriteLine(Name + "：服务器已连接");
            IsAlive = true;
        }


    }
}
