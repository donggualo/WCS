using Panuon.UI.Silver;
using PubResourceManager;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using ModuleManager;

using ADS = WcsManager.Administartor;
using WcsManager.DevModule;

namespace WindowManager
{
    /// <summary>
    /// W_INDEX.xaml 的交互逻辑
    /// </summary>
    public partial class W_INDEX : UserControl, ITabWin
    {
        W_WARN _warn;

        List<DevItem> DList;
        List<DevError> EList;

        DispatcherTimer ShowTimer;

        public W_INDEX()
        {
            InitializeComponent();

            _warn = new W_WARN();

            RefreshData();

            OnTimeToLoadData();
        }

        /// <summary>
        /// 关闭窗口的时候执行释放的动作
        /// </summary>
        public void Close()
        {
            ShowTimer.Stop();
        }

        private void RefreshData()
        {
            try
            {
                DList = ADS.GetDevInfo();
                EList = ADS.GetDevError();
                ShowData();
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "异常", 3, MessageBoxIcon.Error);
            }
        }

        private void ShowData()
        {
            DGd.ItemsSource = null;
            DGd.ItemsSource = DList;

            if (EList == null || EList.Count == 0)
            {
                ShowOK();
            }
            else
            {
                ShowNG();
            }
            _warn.UpdateData(EList);
        }
        private void DGd_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if ((e.Row.Item as DevItem).Connected.Equals("离线"))
            {
                e.Row.Foreground = new SolidColorBrush(Colors.IndianRed);
            }
        }

        private void ShowOK()
        {
            BTNok.Visibility = Visibility.Visible;
            BTNng.Visibility = Visibility.Hidden;
        }

        private void ShowNG()
        {
            BTNng.Visibility = Visibility.Visible;
            BTNok.Visibility = Visibility.Hidden;
        }

        private void BTNng_Click(object sender, RoutedEventArgs e)
        {
            _warn.Show();
        }


        #region 定时刷新数据    

        /// <summary>
        /// 每隔一个时间段执行一段代码
        /// </summary>
        private void OnTimeToLoadData()
        {
            ShowTimer = new DispatcherTimer();
            //起个Timer一直获取当前时间
            ShowTimer.Tick += OnTimeLoadData;
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 5, 0); //天，时，分，秒，毫秒
            ShowTimer.Start();
        }

        /// <summary>
        /// 计时加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeLoadData(object sender, EventArgs e)
        {
            RefreshData();
        }

        #endregion

    }
}
