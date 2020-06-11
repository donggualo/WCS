using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModuleManager;
using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using WindowManager.Datagrid;

using ADS = WcsManager.Administartor;

namespace WindowManager
{
    /// <summary>
    /// W_ABC.xaml 的交互逻辑
    /// </summary>
    public partial class W_AWC : UserControl, ITabWin
    {
        private AwcDataGrid grid;
        private bool runRefresh = true;

        public W_AWC()
        {
            InitializeComponent();
            grid = new AwcDataGrid();

            DataContext = grid;

            GetAWCNameList();

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

        private void GetAWCNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.行车);
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

        /// <summary>
        /// 限制仅输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputNum(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);

            if (int.TryParse(e.Text, out int result))
            {
                if (xlocation.IsFocused)
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(xlocation.Text.Trim()) ? "0" : xlocation.Text.Trim()) + e.Text)
                        > 16777215) //0xFF, 0xFF, 0xFF
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        xlocation.Text = "";
                        return;
                    }
                }
                else if (ylocation.IsFocused)
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(ylocation.Text.Trim()) ? "0" : ylocation.Text.Trim()) + e.Text)
                        > 65535) //0xFF, 0xFF
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        ylocation.Text = "";
                        return;
                    }
                }
                else
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(zlocation.Text.Trim()) ? "0" : zlocation.Text.Trim()) + e.Text)
                        > 65535) //0xFF, 0xFF
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        zlocation.Text = "";
                        return;
                    }
                }
            }

        }

        /// <summary>
        /// 定位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocateBtn_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                if (string.IsNullOrEmpty(xlocation.Text.Trim()) || string.IsNullOrEmpty(ylocation.Text.Trim()) || string.IsNullOrEmpty(zlocation.Text.Trim()))
                {
                    Notice.Show("请填写目的坐标！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!ADS.mSocket.IsConnected(dev))
                {
                    Notice.Show(dev + "已离线，无法操作！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否进行[定位任务]！！"))
                {
                    return;
                }

                int x = Convert.ToInt32(xlocation.Text.Trim());
                int y = Convert.ToInt32(ylocation.Text.Trim());
                ADS.mAwc.devices.Find(c => c.devName == dev).ToSite(x, y);

                Notice.Show("定位任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("LocateBtn_Click()", "行车界面定位[设备号]", ex.Message, dev);
            }
        }

        /// <summary>
        /// 取货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                if (string.IsNullOrEmpty(xlocation.Text.Trim()) || string.IsNullOrEmpty(ylocation.Text.Trim()) || string.IsNullOrEmpty(zlocation.Text.Trim()))
                {
                    Notice.Show("请填写目的坐标！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!ADS.mSocket.IsConnected(dev))
                {
                    Notice.Show(dev + "已离线，无法操作！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否进行[取货任务]！！"))
                {
                    return;
                }

                int z = Convert.ToInt32(zlocation.Text.Trim());

                ADS.mAwc.devices.Find(c => c.devName == dev).StartTake(z);

                Notice.Show("取货任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("LoadBtn_Click()", "行车界面取货[设备号]", ex.Message, dev);
            }
        }

        /// <summary>
        /// 放货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                if (string.IsNullOrEmpty(xlocation.Text.Trim()) || string.IsNullOrEmpty(ylocation.Text.Trim()) || string.IsNullOrEmpty(zlocation.Text.Trim()))
                {
                    Notice.Show("请填写目的坐标！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!ADS.mSocket.IsConnected(dev))
                {
                    Notice.Show(dev + "已离线，无法操作！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否进行[放货任务]！！"))
                {
                    return;
                }

                int z = Convert.ToInt32(zlocation.Text.Trim());

                ADS.mAwc.devices.Find(c => c.devName == dev).StartGive(z);

                Notice.Show("放货任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("UnloadBtn_Click()", "行车界面放货[设备号]", ex.Message, dev);
            }
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Relocate_Click(object sender, RoutedEventArgs e)
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

                ADS.mAwc.devices.Find(c => c.devName == dev).ResetTask();

                Notice.Show("复位任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);

            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("Relocate_Click()", "行车界面复位[设备号]", ex.Message, dev);
            }
        }

        /// <summary>
        /// 终止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminateBtn_Click(object sender, RoutedEventArgs e)
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

                ADS.mAwc.devices.Find(c => c.devName == dev).StopTask();

                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);

            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("TerminateBtn_Click()", "行车界面终止[设备号]", ex.Message, dev);
            }
        }

    }
}
