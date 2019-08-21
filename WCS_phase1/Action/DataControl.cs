using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Http;
using WCS_phase1.NDC;
using WCS_phase1.Socket;
using WCS_phase1.Models;
using WCS_phase1.Functions;

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

        /// <summary>
        /// 数据库sql执行
        /// </summary>
        internal static MySQL _mMySql;

        /// <summary>
        /// 简易功能
        /// </summary>
        internal static SimpleTools _mStools;

        /// <summary>
        /// 任务服务
        /// </summary>
        internal static TaskTools _mTaskTools;

        /// <summary>
        /// WCS 任务指令管理
        /// </summary>
        internal static TaskControler _mTaskControler;

        /// <summary>
        /// WCS 任务逻辑执行
        /// </summary>
        internal static RunTask _mRunTask;

        /// <summary>
        /// AGV 任务
        /// </summary>
        internal static ForAGVControl _mForAGVControl;

        #region 设定

        /// <summary>
        /// 是否运行生成任务逻辑
        /// </summary>
        public static bool IsRunTaskLogic = false;

        /// <summary>
        /// 是否运行任务指令发送
        /// </summary>
        public static bool IsRunTaskOrder = false;

        /// <summary>
        /// 是否运行AGV派送
        /// </summary>
        public static bool IsRunSendAGV = false;


        /// <summary>
        /// 是否无视AGV货物状态
        /// </summary>
        public static bool IsIgnoreAGV = false;

        /// <summary>
        /// 是否无视固定辊台货物状态
        /// </summary>
        public static bool IsIgnoreFRT = false;

        /// <summary>
        /// 是否无视摆渡车货物状态
        /// </summary>
        public static bool IsIgnoreARF = false;

        /// <summary>
        /// 是否无视运输车货物状态
        /// </summary>
        public static bool IsIgnoreRGV = false;

        /// <summary>
        /// 是否无视行车货物状态
        /// </summary>
        public static bool IsIgnoreABC = false;

        #endregion

        private static bool init = false;//是否已经初始化

        public static void Init()
        {
            if (!init)
            {
                //_mSocket = new SocketControl();

                _mMySql = new MySQL();

                _mStools = new SimpleTools();

                _mTaskTools = new TaskTools();
                //_mTaskTools.InitializeClient();

                //_mHttpServer = new HttpServerControl();

                //_mHttp = new HttpControl();

                //_mNDCControl = new NDCControl();

                //_mTaskControler = new TaskControler();

                //_mRunTask = new RunTask();

                //_mForAGVControl = new ForAGVControl();

                init = true;
            }
        }

        public static bool BeforeClose()
        {
            if (init)
            {
                if(_mSocket!=null)_mSocket.CloseClient();

                if (_mHttpServer != null) _mHttpServer.StopServer();

                if (_mNDCControl != null) _mNDCControl.DoDisConnectNDC();
            }
            return true;
        }
    }
}
