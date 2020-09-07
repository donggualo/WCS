using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using ToolManager;

namespace WCS_phase1
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class Application : System.Windows.Application
    {
        #region 异常捕获

        protected override void OnStartup(StartupEventArgs e)
        {
            //app.Run()时触发
            base.OnStartup(e);

            //注册Application_Error ——全局异常捕获
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            //  当某个异常未被捕获时出现。主要指的是非UI线程
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #if DEBUG
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            #endif
        }

        Log log;

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //可以记录日志并转向错误bug窗口友好提示用户
            if (e.ExceptionObject is System.Exception)
            {
                if (log == null) log = new Log("app_error");
                Exception ex = (System.Exception)e.ExceptionObject;
                MessageBox.Show("确认后自动重启，异常提示：\n" + ex.Message + "\n" + ex.Source, "Error");
                //Notice.Show("妈的！发现异常：\n" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                log.LOG(ex.Message + ex.StackTrace);
                //////开启新的实例
                System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);

                //////关闭当前实例
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (log == null) log = new Log("app_error");
            MessageBox.Show("确认后自动重启，发现异常：\n" + e.Exception.Message + e.Exception.Source, "Error");
            //Notice.Show("妈的！发现异常：\n" + e.Exception.Message + "\n来源：" + e.Exception.Source, "错误", 3, MessageBoxIcon.Error);
            //处理完后，需要将 Handler = true 表示已处理过此异常
            e.Handled = true;
            log.LOG(e.Exception.Message + e.Exception.StackTrace);

            //////开启新的实例
            System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);

            //////关闭当前实例
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        #endregion

        #region 单开应用

        public Application()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            CheckApplicationMutex("WcsWindow", "WCS设备调度系统");
        }

        /// <summary>
        /// 进程
        /// </summary>
        private Mutex mutex;

        /// <summary>
        /// 检查应用程序是否在进程中已经存在了
        /// </summary>
        /// <param name="APP_NameSpace">工程命名空间名</param>
        /// <param name="APP_Title">窗体标题名</param>
        private void CheckApplicationMutex(string APP_NameSpace, string APP_Title)
        {

            // 第二个参数为 你的工程命名空间名。
            // out 给 ret 为 false 时，表示已有相同实例运行。
            mutex = new Mutex(true, APP_NameSpace, out bool mutexResult);

            if (!mutexResult)
            {
                // 找到已经在运行的实例句柄(给出你的窗体标题名 “Deamon Club”)
                IntPtr hWndPtr = FindWindow(null, APP_Title);

                // 还原窗口
                ShowWindow(hWndPtr, SW_RESTORE);

                // 激活窗口
                SetForegroundWindow(hWndPtr);

                // 退出当前实例程序
                Environment.Exit(0);
            }
        }

        #region Windows API

        // ShowWindow 参数  
        private const int SW_RESTORE = 9;

        /// <summary>
        /// 在桌面窗口列表中寻找与指定条件相符的第一个窗口。
        /// </summary>
        /// <param name="lpClassName">指向指定窗口的类名。如果 lpClassName 是 NULL，所有类名匹配。</param>
        /// <param name="lpWindowName">指向指定窗口名称(窗口的标题）。如果 lpWindowName 是 NULL，所有windows命名匹配。</param>
        /// <returns>返回指定窗口句柄</returns>
        [DllImport("USER32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 将窗口还原,可从最小化还原
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 激活指定窗口
        /// </summary>
        /// <param name="hWnd">指定窗口句柄</param>
        /// <returns></returns>
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        #endregion

    }
}
