using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using PubResourceManager;
using WcsHttpManager;
using WindowManager;

namespace WCS_phase1
{
    /// <summary>
    /// WcsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WcsWindow : WindowX
    {
        public WcsWindow()
        {
            InitializeComponent();
            DataControl.Init();

            CheckTask_I.IsChecked = PublicParam.IsRunTaskLogic_I;
            CheckTask_O.IsChecked = PublicParam.IsRunTaskLogic_O;
            CheckOrder.IsChecked = PublicParam.IsRunTaskOrder;
            CheckAGV.IsChecked = PublicParam.IsRunSendAGV;
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
            else if ("ABC".Equals(itemTag))
            {
                tabItem.Tag = "ABC";
                tabItem.Content = new W_ABC();
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
            else if ("DevIgnore".Equals(itemTag))
            {
                tabItem.Tag = "DevIgnore";
                tabItem.Content = new W_SettingDevIgnore();
            }
            else if ("DevData".Equals(itemTag))
            {
                tabItem.Tag = "DevData";
                tabItem.Content = new W_SettingDevData();
            }
            else if ("Location".Equals(itemTag))
            {
                tabItem.Tag = "Location";
                tabItem.Content = new W_SettingLocation();
            }
            else if ("ManualWms".Equals(itemTag))
            {
                tabItem.Tag = "ManualWms";
                tabItem.Content = new W_ManualWms();
            }
            else if ("TaskData".Equals(itemTag))
            {
                tabItem.Tag = "TaskData";
                tabItem.Content = new W_TaskData();
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
                DataControl.BeforeClose();
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void GetWmsInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                WmsModel result = DataControl._mHttp.DoBarcodeScanTask("A01","BARCODE");
                if (result != null)
                {
                    Notice.Show(result.ToString(), "信息", 15, MessageBoxIcon.Info);

                }
            }
            catch(Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);

            }
        }

        /// <summary>
        /// 是否执行出入库任务生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTask_I_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunTaskLogic_I = (bool)CheckTask_I.IsChecked;
        }
        private void CheckTask_O_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunTaskLogic_O = (bool)CheckTask_O.IsChecked;
        }

        /// <summary>
        /// 是否执行指令发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOrder_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunTaskOrder = (bool)CheckOrder.IsChecked;
        }

        /// <summary>
        /// 是否运行AGV派送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAGV_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunSendAGV = (bool)CheckAGV.IsChecked;
        }

        /// <summary>
        /// Ndc连接服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NdcConnectCB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ndcConnectCB.IsChecked != null && (bool)ndcConnectCB.IsChecked)
                {
                    DataControl._mNDCControl.DoConnectNDC();
                }
                else
                {
                    DataControl._mNDCControl.DoDisConnectNDC();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
