using ModuleManager.PUB;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ToolManager;

namespace WcsManager
{
    public class MasterCode
    {
        /// <summary>
        /// 运行
        /// </summary>
        private bool IsTurnOn = true;
        private readonly object _obj = new object();

        /// <summary>
        /// 等待
        /// </summary>
        internal const int WAIT_TIMEOUT = 10;

        private Socket serverSocket;
        private List<ClinetBase> clients = new List<ClinetBase>();
        private Log log;
        private List<CodeBase> Codes = new List<CodeBase>();

        public delegate void SendCode(string type, string dev, string msg);
        public event SendCode sendCode;

        public MasterCode()
        {
            log = new Log("ScanCode");
            AddCode();
            Start();
        }

        /// <summary>
        /// 加载扫码器信息
        /// </summary>
        private void AddCode()
        {
            if (CommonSQL.GetWcsParam("WCS_SCAN_CODE", out List<WCS_PARAM> info))
            {
                foreach (WCS_PARAM item in info)
                {
                    Codes.Add(new CodeBase()
                    {
                        _ip = item.VALUE1,
                        _name = item.VALUE2,
                        _forType = item.VALUE4,
                        _forDev = item.VALUE5
                    });
                }
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.8.233"), 63333));
            serverSocket.Listen(100);
            log.LOG(string.Format("已启动服务器,启动监听{0}成功", serverSocket.LocalEndPoint.ToString()));
            new Thread(ListenClientConnect).Start();
            new Thread(ReceiveMessage).Start();
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            while (IsTurnOn)
            {
                if (!IsTurnOn)
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                    serverSocket.Close();
                    log.LOG("CLOSE...");
                }
                else
                {
                    Socket clientSocket = serverSocket.Accept();
                    lock (clients)
                    {
                        if (!clients.Exists(c => c.clientSocket == clientSocket))
                        {
                            clients.Add(new ClinetBase()
                            {
                                clientSocket = clientSocket,
                                buffer = new byte[1024]  //设置一个缓冲区，用来保存数
                            });
                        }
                    }

                    //Socket clientSocket = serverSocket.Accept();
                    //Thread receiveThread = new Thread(ReceiveMessage);
                    //receiveThread.Start(clientSocket);
                }
            }
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="clientSocket"></param>
        //private void ReceiveMessage(object clientSocket)
        //{
        //    Socket myClientSocket = (Socket)clientSocket;
        //    while (!myClientSocket.Poll(10, SelectMode.SelectRead))
        //    {
        //        try
        //        {
        //            var buffer = new byte[1024];//设置一个缓冲区，用来保存数
        //            myClientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
        //            {
        //                try
        //                {
        //                    int length = myClientSocket.EndReceive(ar);
        //                    //读取出来消息内容
        //                    string message = Encoding.ASCII.GetString(buffer, 0, length);
        //                    if (!string.IsNullOrEmpty(message))
        //                    {
        //                        string clientIP = myClientSocket.RemoteEndPoint.ToString().Split(':')[0];
        //                        if (Codes.Exists(c => c._ip == clientIP))
        //                        {
        //                            CodeBase cb = Codes.Find(c => c._ip == clientIP);
        //                            sendCode?.Invoke(cb._forType, cb._forDev, message.Trim());
        //                        }
        //                        log.LOG(string.Format("接收客户端{0}消息:{1}", clientIP, message));
        //                    }
        //                }
        //                catch
        //                {
        //                }
        //            }), null);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            myClientSocket.Shutdown(SocketShutdown.Both);
        //            myClientSocket.Close();
        //            break;
        //        }
        //    }
        //}

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="clientSocket"></param>
        private void ReceiveMessage()
        {
            while (IsTurnOn)
            {
                if (clients == null || clients.Count == 0) continue;

                lock (clients)
                {
                    try
                    {
                        foreach (ClinetBase cb in clients)
                        {
                            if (!cb.clientSocket.Poll(10, SelectMode.SelectRead))
                            {
                                try
                                {
                                    cb.clientSocket.BeginReceive(cb.buffer, 0, cb.buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                                    {
                                        try
                                        {
                                            int length = cb.clientSocket.EndReceive(ar);
                                            //读取出来消息内容
                                            string message = Encoding.ASCII.GetString(cb.buffer, 0, length);
                                            if (!string.IsNullOrEmpty(message))
                                            {
                                                string clientIP = cb.clientSocket.RemoteEndPoint.ToString().Split(':')[0];
                                                if (Codes.Exists(c => c._ip == clientIP))
                                                {
                                                    CodeBase codeB = Codes.Find(c => c._ip == clientIP);
                                                    sendCode?.Invoke(codeB._forType, codeB._forDev, message.Trim());
                                                }
                                                log.LOG(string.Format("接收客户端{0}消息:{1}", clientIP, message));
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }), null);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    cb.clientSocket.Shutdown(SocketShutdown.Both);
                                    cb.clientSocket.Close();
                                    clients.Remove(cb);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 关闭所有
        /// </summary>
        public void CloseAll()
        {
            if (IsTurnOn)
            {
                IsTurnOn = false;

                lock (serverSocket)
                {
                    //serverSocket.Close();
                    log.LOG("CLOSE...");
                }
            }
        }

    }

    public class CodeBase
    {
        internal string _ip;
        internal string _name;
        internal string _forType;
        internal string _forDev;
    }

    public class ClinetBase
    {
        internal Socket clientSocket;
        internal byte[] buffer;
    }

}
