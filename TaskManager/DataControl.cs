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

        /// <summary>
        /// 简易功能
        /// </summary>
        public static SimpleTools _mStools;

        /// <summary>
        /// 任务服务
        /// </summary>
        public static TaskTools _mTaskTools;

        /// <summary>
        /// WCS 任务指令管理
        /// </summary>
        public static TaskControler _mTaskControler;

        /// <summary>
        /// WCS 任务逻辑执行
        /// </summary>
        public static RunTask _mRunTask;

        /// <summary>
        /// AGV 任务
        /// </summary>
        public static ForAGVControl _mForAGVControl;

        /// <summary>
        /// 对接WMS服务
        /// </summary>
        public static ForWMSControl _mForWmsControl;


        private static bool init = false;//是否已经初始化

        public static void Init()
        {
            if (!init)
            {
                init = true;

                _mSocket = new SocketControl();

                _mMySql = new MySQL();

                _mStools = new SimpleTools();

                _mTaskTools = new TaskTools();

                _mHttpServer = new HttpServerControl();

                _mHttp = new HttpControl();

                _mNDCControl = new NDCControl();

                _mTaskControler = new TaskControler();

                _mRunTask = new RunTask();

                _mForAGVControl = new ForAGVControl();

                _mNDCControl.NoticeWcsMagic += _mForAGVControl.SubmitAgvMagic;
                _mNDCControl.NoticeWcsOnLoad += _mForAGVControl.SubmitAgvLoading;

                _mForWmsControl = new ForWMSControl();

                _mHttpServer.WmsModelAdd += _mForWmsControl.WriteTaskToWCS;


            }
        }

        public static bool BeforeClose()
        {
            if (init)
            {
                if (_mHttpServer != null && _mForWmsControl != null)
                {
                    _mHttpServer.WmsModelAdd -= _mForWmsControl.WriteTaskToWCS;
                }

                if (_mNDCControl != null && _mForAGVControl != null)
                {
                    _mNDCControl.NoticeWcsMagic -= _mForAGVControl.SubmitAgvMagic;
                    _mNDCControl.NoticeWcsOnLoad -= _mForAGVControl.SubmitAgvLoading;
                }


                if (_mSocket != null) _mSocket.Exiting();
                if (_mHttpServer != null) _mHttpServer.Close();
                if (_mNDCControl != null) _mNDCControl.Close();

                if (_mRunTask != null) _mRunTask.Close();
                if (_mTaskControler != null) _mTaskControler.Close();
                if (_mForAGVControl != null) _mForAGVControl.Close();
            }
            return true;
        }
    }
}
