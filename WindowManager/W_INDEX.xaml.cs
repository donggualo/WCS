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
using ModuleManager.PUB;

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

                if (CommonSQL.GetWcsParam("WCS_SCAN_CODE", out List<WCS_PARAM> info))
                {
                    foreach (WCS_PARAM item in info)
                    {
                        bool isOnline = ADS.mSocket.IsConnected(string.Format("{0}-{1}-{2}", "Scan", item.VALUE4, item.VALUE5));
                        switch (item.VALUE3)
                        {
                            case 1:
                                CBscan1.IsChecked = isOnline;
                                break;
                            case 2:
                                CBscan2.IsChecked = isOnline;
                                break;
                            case 3:
                                CBscan3.IsChecked = isOnline;
                                break;
                            default:
                                break;
                        }
                    }
                }

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
            if ((e.Row.Item as DevItem).Connected.Contains("离线") || (e.Row.Item as DevItem).Connected.Contains("故障"))
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
