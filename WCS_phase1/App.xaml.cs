using System.Windows;
using ToolManager;

namespace WCS_phase1
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class Application : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //app.Run()时触发
            base.OnStartup(e);

            //注册Application_Error ——全局异常捕获
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            #if DEBUG
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            #endif
        }

        Log log;

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (log == null) log = new Log("app_error");
            MessageBox.Show("妈的！发现异常：\n" + e.Exception.Message+e.Exception.Source, "Error");
            //Notice.Show("妈的！发现异常：\n" + e.Exception.Message + "\n来源：" + e.Exception.Source, "错误", 3, MessageBoxIcon.Error);
            //处理完后，需要将 Handler = true 表示已处理过此异常
            e.Handled = true;
            log.LOG(e.Exception.Message + e.Exception.StackTrace);
        }

    }
}
