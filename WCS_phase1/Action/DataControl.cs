using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Http;
using WCS_phase1.NDC;
using WCS_phase1.Socket;

namespace WCS_phase1.Action
{
    class DataControl
    {
        /// <summary>
        /// 获取设备通信信息
        /// </summary>
        internal static SocketControl _mSocket;

        /// <summary>
        /// 控制提供给WMS的服务
        /// </summary>
        internal static HttpServerControl _mHttpServer;

        /// <summary>
        /// 请求WMS
        /// </summary>
        internal static HttpControl _mHttp;


        /// <summary>
        /// 控制激光小车任务和调度
        /// </summary>
        internal static NDCControl _mNDCControl;

        private static bool init = false;//是否已经初始化

        public static void Init()
        {
            if (!init)
            {
                _mSocket = new SocketControl();

                _mHttpServer = new HttpServerControl();

                _mHttp = new HttpControl();

                _mNDCControl = new NDCControl();

                init = true;
            }
        }

        public static bool BeforeClose()
        {
            if (init)
            {
                if(_mSocket!=null)_mSocket.CloseClient();

                if (_mHttpServer != null) _mHttpServer.StopServer();

                if (_mNDCControl != null) _mNDCControl.DoConnectNDC();
            }
            return true;
        }
    }
}
