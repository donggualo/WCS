using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModuleManager;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using PubResourceManager;
using WindowManager;

using ADS = WcsManager.Administartor;

namespace WCS_phase1
{
    /// <summary>
    /// WcsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WcsWindow : WindowX
    {
        public ADS admin;
        public WcsWindow()
        {
            InitializeComponent();
            admin = new ADS();

            CheckIn.IsChecked = PublicParam.IsDoJobIn;
            CheckOut.IsChecked = PublicParam.IsDoJobOut;
            CheckDev.IsChecked = PublicParam.IsDoTask;
            CheckAGV.IsChecked = PublicParam.IsDoJobAGV;
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

            TreeViewItem item = sender as TreeViewItem;
            string itemTag = item.Tag.ToString();

            foreach(TabItem i in wcsTabControl.Items)
            {
                if (itemTag.Equals(i.Tag))
                {
                    i.IsSelected = true;
                    return;
                }
            }

            TabItem tabItem = new TabItem();
            tabItem.Header = item.Header;
            //主页面
            if ("Home".Equals(itemTag))
            {
                wcsTabControl.SelectedIndex = 0;
                return;
            }
            else if ("AWC".Equals(itemTag))
            {
                tabItem.Tag = "AWC";
                tabItem.Content = new W_AWC();
            }
            else if ("RGV".Equals(itemTag))
            {
                tabItem.Tag = "RGV";
                tabItem.Content = new W_RGV();
            }
            else if ("FRT".Equals(itemTag))
            {
                tabItem.Tag = "FRT";
                tabItem.Content = new W_FRT();
            }
            else if ("ARF".Equals(itemTag))
            {
                tabItem.Tag = "ARF";
                tabItem.Content = new W_ARF();
            }
            else if ("PKL".Equals(itemTag))
            {
                tabItem.Tag = "PKL";
                tabItem.Content = new W_PKL();
            }
            else if ("AreaData".Equals(itemTag))
            {
                tabItem.Tag = "AreaData";
                tabItem.Content = new W_SettingAreaData();
            }
            else if ("DevData".Equals(itemTag))
            {
                tabItem.Tag = "DevData";
                tabItem.Content = new W_SettingDevData();
            }
            else if ("LocData".Equals(itemTag))
            {
                tabItem.Tag = "LocData";
                tabItem.Content = new W_SettingLocation();
            }
            else if ("WorkData".Equals(itemTag))
            {
                tabItem.Tag = "WorkData";
                tabItem.Content = new W_WcsWorkData();
            }
            else if ("TaskData".Equals(itemTag))
            {
                tabItem.Tag = "TaskData";
                tabItem.Content = new W_WmsTaskData();
            }
            else if ("ErrLogs".Equals(itemTag))
            {
                tabItem.Tag = "ErrLogs";
                tabItem.Content = new W_ErrLogs();
            }
            else if ("WcsNdcSite".Equals(itemTag))
            {
                tabItem.Tag = "WcsNdcSite";

                tabItem.Content = new W_WcsNdcSite();
            }
            else
            {
                tabItem.Tag = "AGV";
                tabItem.Content = new W_NdcAgv();
            }

            wcsTabControl.Items.Add(tabItem);
            wcsTabControl.SelectedIndex = wcsTabControl.Items.Count-1;
        }

        private void WcsTabControl_Removed(object sender, RoutedPropertyChangedEventArgs<TabItem> e)
        {
            try
            {
                wcsTabControl.SelectedIndex = wcsTabControl.Items.Count - 2;
                TabItem t = e.NewValue as TabItem;
                ITabWin _a = t.Content as ITabWin;
                _a.Close();
                wcsTabControl.Items.Remove(e.NewValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBoxX.Show("是否退出程序", "警告", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                MessageBoxStyle = MessageBoxStyle.Standard,
                MessageBoxIcon = MessageBoxIcon.Warning
            });
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                admin.BeforeClose();
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 是否生成入库任务
        /// </summary>
        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsDoJobIn = (bool)CheckIn.IsChecked;
        }

        /// <summary>
        /// 是否生成出库任务
        /// </summary>
        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsDoJobOut = (bool)CheckOut.IsChecked;
        }

        /// <summary>
        /// 是否运作设备
        /// </summary>
        private void CheckDev_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsDoTask = (bool)CheckDev.IsChecked;
        }

        /// <summary>
        /// 是否运行AGV派送
        /// </summary>
        private void CheckAGV_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsDoJobAGV = (bool)CheckAGV.IsChecked;
        }

        /// <summary>
        /// Ndc连接服务
        /// </summary>
        private void NdcConnectCB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ndcConnectCB.IsChecked != null && (bool)ndcConnectCB.IsChecked)
                {
                    ADS.mNDCControl.DoConnectNDC();
                }
                else
                {
                    ADS.mNDCControl.DoDisConnectNDC();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
