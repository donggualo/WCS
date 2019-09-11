using MHttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WcsHttpManager
{
    /// <summary>
    /// 提供WMS访问的服务类
    /// </summary>
    public class HttpServerControl
    {
        /// <summary>
        /// 服务监听端口
        /// </summary>
        private int listenerPort = 8080;
        private bool isStart = false;
        HttpServer httpServer;

        /// <summary>
        /// WMS任务通知句柄
        /// </summary>
        /// <param name="model"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public delegate bool WmsModelHandler(WmsModel model, out string result);
        /// <summary>
        /// WMS任务请求
        /// </summary>
        public event WmsModelHandler WmsModelAdd;

        /// <summary>
        /// WCS HTTP服务启动
        /// </summary>
        public HttpServerControl()
        {
            StartServer();
        }

        /// <summary>
        /// 启动服务并监听端口
        /// </summary>
        public void StartServer()
        {
            try
            {
                if (!isStart)
                {
                    Routes routes = new Routes();
                    routes.WmsModelAdd += Routes_WmsModelAdd;
                    
                    httpServer = new HttpServer(listenerPort, routes.GET);

                    new Thread(httpServer.Listen)
                    {
                        IsBackground = true
                    }.Start();
                    
                    isStart = true;
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// WMS任务请求 ->  NDC-HTTP服务接收 -> 通知WCS
        /// </summary>
        /// <param name="model"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool Routes_WmsModelAdd(WmsModel model, out string result)
        {
            return WmsModelAdd(model, out result);
        }


        /// <summary>
        /// 关闭服务
        /// </summary>
        public void Close()
        {
            if (httpServer != null)
            {
                //1.退出循环
                httpServer.Close();

                //2.退出Accept阻塞
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect("127.0.0.1", listenerPort);

                NetworkStream ns = tcpClient.GetStream();
                if (ns.CanWrite)
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Hello");
                    ns.Write(sendBytes, 0, sendBytes.Length);
                }
                ns.Close();
                tcpClient.Close();
            }
        }
    }
}
