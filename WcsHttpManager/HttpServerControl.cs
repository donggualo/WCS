using MHttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Thread thread;

        public delegate bool WmsModelHandler(WmsModel model, out string result);
        public event WmsModelHandler WmsModelAdd;

        public HttpServerControl()
        {
            StartServer();
        }

        public void StartServer()
        {
            try
            {
                if (!isStart)
                {
                    Routes routes = new Routes();
                    routes.WmsModelAdd += Routes_WmsModelAdd;
                    
                    HttpServer httpServer = new HttpServer(listenerPort, routes.GET);

                    thread = new Thread(new ThreadStart(httpServer.Listen));
                    thread.Start();
                    isStart = true;
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool Routes_WmsModelAdd(WmsModel model, out string result)
        {
            return WmsModelAdd(model, out result);
        }

        public void StopServer()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
        }
    }
}
