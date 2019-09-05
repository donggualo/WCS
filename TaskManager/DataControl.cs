using SockManager;
using WcsHttpManager;
using NdcManager;
using TaskManager.Functions;
using PubResourceManager;

namespace TaskManager
{
    public class DataControl
    {
        /// <summary>
        /// 获取设备通信信息
        /// </summary>
        public static SocketControl _mSocket;

        /// <summary>
        /// 控制提供给WMS的服务
        /// </summary>
        public static HttpServerControl _mHttpServer;

        /// <summary>
        /// 请求WMS
        /// </summary>
        public static HttpControl _mHttp;

        /// <summary>
        /// 控制激光小车任务和调度
        /// </summary>
        public static NDCControl _mNDCControl;

        /// <summary>
        /// 数据库sql执行
        /// </summary>
        public static MySQL _mMySql;

        ///// <summary>
        ///// 简易功能
        ///// </summary>
        public static SimpleTools _mStools;

        ///// <summary>
        ///// 任务服务
        ///// </summary>
        public static TaskTools _mTaskTools;

        ///// <summary>
        ///// WCS 任务指令管理
        ///// </summary>
        public static TaskControler _mTaskControler;

        ///// <summary>
        ///// WCS 任务逻辑执行
        ///// </summary>
        public static RunTask _mRunTask;

        ///// <summary>
        ///// AGV 任务
        ///// </summary>
        public static ForAGVControl _mForAGVControl;


        public static ForWMSControl _mForWmsControl;

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
                _mSocket = new SocketControl();

                _mMySql = new MySQL();

                _mStools = new SimpleTools();

               // _mTaskTools = new TaskTools();

                _mHttpServer = new HttpServerControl();

                _mHttp = new HttpControl();

                _mNDCControl = new NDCControl();

                _mTaskControler = new TaskControler();

                _mRunTask = new RunTask();

                _mForAGVControl = new ForAGVControl();

                _mNDCControl.AGVMagicUpdate += _mForAGVControl.SubmitAgvMagic;
                _mNDCControl.AGVDataUpdate += _mForAGVControl.SubmitAgvLoading;

                _mForWmsControl = new ForWMSControl();

                _mHttpServer.WmsModelAdd += _mForWmsControl.WriteTaskToWCS;

                init = true;
            }
        }

        public static bool BeforeClose()
        {
            if (init)
            {
                if(_mHttpServer != null && _mForWmsControl != null)
                {
                    _mHttpServer.WmsModelAdd -= _mForWmsControl.WriteTaskToWCS;
                }

                if (_mNDCControl != null && _mForAGVControl != null)
                {
                    _mNDCControl.AGVMagicUpdate -= _mForAGVControl.SubmitAgvMagic;
                    _mNDCControl.AGVDataUpdate -= _mForAGVControl.SubmitAgvLoading;
                }


                if(_mSocket!=null)_mSocket.CloseClient();

                if (_mHttpServer != null) _mHttpServer.StopServer();

                if (_mNDCControl != null) _mNDCControl.DoDisConnectNDC();
            }
            return true;
        }
    }
}
