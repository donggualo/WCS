using Module;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using PubResourceManager;
using Socket;
using System;
using System.Threading;
using System.Windows;
using WcsManager;

using ADS = WcsManager.Administartor;

namespace TestWim
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ADS admin;
        public MainWindow()
        {
            InitializeComponent();
            admin = new ADS();
        }

        private void Awc2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string name = "AWC02";
            ADS.mAwc.AddUpdateDev(name, DevFlag.远离入库口, (int)Awc2.Value);
        }

        private void Awc1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string name = "AWC01";
            ADS.mAwc.AddUpdateDev(name, DevFlag.靠近入库口, (int)Awc1.Value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // int goods = int.Parse();
            GoodsList.Items.Add(GoodsP.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (GoodsList.SelectedItem != null)
            {
                GoodsList.Items.Remove(GoodsList.SelectedItem);
            }
        }
        int taskuid = 123;

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var i in GoodsList.Items)
            {
                admin.AddAwcTempJob(new WmsTask
                {
                    givesite = "B01",
                    takesite = i.ToString(),
                    tasktype = TaskTypeEnum.出库,
                    taskstatus = WmsTaskStatus.init,
                    taskuid = "" + taskuid++
                });
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //admin.CheckOutTempJob("B01");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            goodsresult.Document.Blocks.Clear();
            goodsresult.AppendText("\n结果=>" + admin.GetOutTempInfo());
        }

        private void BTNdev_Click(object sender, RoutedEventArgs e)
        {
            Device d = new Device();
            d.Show();
        }

        private void BTNtask_Click(object sender, RoutedEventArgs e)
        {
            TaskTest t = new TaskTest(admin);
            t.Show();
        }

        /// <summary>
        /// 退出
        /// </summary>
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

        private void BTNtest_Click(object sender, RoutedEventArgs e)
        {
            WmsTest d = new WmsTest();
            d.Show();
        }
    }
}
