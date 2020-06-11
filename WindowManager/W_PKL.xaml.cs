using Module;
using ModuleManager;
using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WindowManager.Datagrid;

using ADS = WcsManager.Administartor;

namespace WindowManager
{
    /// <summary>
    /// W_PKL.xaml 的交互逻辑
    /// </summary>
    public partial class W_PKL : UserControl, ITabWin
    {
        private PklDataGrid grid;
        private bool runRefresh = true;

        public W_PKL()
        {
            InitializeComponent();
            grid = new PklDataGrid();

            DataContext = grid;

            GetPKLNameList();

            new Thread(DoRefresh)
            {
                IsBackground = true
            }.Start();
        }
        /// <summary>
        /// 关闭窗口的时候执行释放的动作
        /// </summary>
        public void Close()
        {
            runRefresh = false;
        }

        private void GetPKLNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.包装线辊台);
            foreach (var l in list)
            {
                grid.UpdateDeviceList(l.DEVICE, l.AREA);
            }
        }

        private void DoRefresh()
        {
            try
            {
                while (runRefresh)
                {
                    Thread.Sleep(1000);

                    Application.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        grid.UpdateDeviceList();
                    }));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("更新终止：" + e.Message);
            }
        }

        private void BTNrun_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!ADS.mSocket.IsConnected(dev))
                {
                    Notice.Show(dev + "已离线，无法操作！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                ADS.mPkl.devices.Find(c => c.devName == dev).StartGiveRoll();
                Notice.Show("启动辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNrun_Click()", "包装线界面启动辊台[设备号]", ex.Message, dev);
            }
        }

        private void BTNstop_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!ADS.mSocket.IsConnected(dev))
                {
                    Notice.Show(dev + "已离线，无法操作！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                ADS.mPkl.devices.Find(c => c.devName == dev).StopTask();

                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNstop_Click()", "包装线界面停止辊台[设备号]", ex.Message, dev);
            }
        }
    }
}
