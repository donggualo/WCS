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
using TaskManager;
using TaskManager.Devices;
using WindowManager.Datagrid;

namespace WindowManager 
{
    /// <summary>
    /// W_ABC.xaml 的交互逻辑
    /// </summary>
    public partial class W_ABC : UserControl,ITabWin
    {
        private AbcDataGrid grid;
        private bool runRefresh = true;

        public W_ABC()
        {
            InitializeComponent();
            grid = new AbcDataGrid();

            DataContext = grid;

            getABCNameList();

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

        private void getABCNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(DataControl._mMySql, DeviceType.行车);
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

            if (int.TryParse(e.Text,out int result))
            {
                if (xlocation.IsFocused)    
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(xlocation.Text.Trim()) ? "0" : xlocation.Text.Trim()) + e.Text)
                        >
                        DataControl._mStools.BytesToInt(new byte[] { 0xFF, 0xFF, 0xFF }))   // 3位 byte
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        xlocation.Text = "";
                        return;
                    }
                }
                else if (ylocation.IsFocused)
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(ylocation.Text.Trim()) ? "0" : ylocation.Text.Trim()) + e.Text)
                        >
                        DataControl._mStools.BytesToInt(new byte[] { 0xFF, 0xFF })) // 2位 byte
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        ylocation.Text = "";
                        return;
                    }
                }
                else 
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(zlocation.Text.Trim()) ? "0" : zlocation.Text.Trim()) + e.Text)
                        >
                        DataControl._mStools.BytesToInt(new byte[] { 0xFF, 0xFF })) // 2位 byte
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
            byte[] order = null;
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

                ABC abc = new ABC(dev);
                if (abc.ActionStatus() == ABC.Run)
                {
                    Notice.Show("指令发送失败：设备运行中！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                int x = Convert.ToInt32(xlocation.Text.Trim());
                int y = Convert.ToInt32(ylocation.Text.Trim());
                int z = Convert.ToInt32(zlocation.Text.Trim());
                order = ABC._TaskControl(ABC.TaskLocate, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("LocateBtn_Click()", "行车-定位任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("定位任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, false);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("LocateBtn_Click()", "行车-定位任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), ex.Message);
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
            byte[] order = null;
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

                ABC abc = new ABC(dev);
                if (abc.ActionStatus() == ABC.Run)
                {
                    Notice.Show("指令发送失败：设备运行中！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                int x = Convert.ToInt32(xlocation.Text.Trim());
                int y = Convert.ToInt32(ylocation.Text.Trim());
                int z = Convert.ToInt32(zlocation.Text.Trim());
                order = ABC._TaskControl(ABC.TaskTake, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("LoadBtn_Click()", "行车-取货任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("取货任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, false);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("LoadBtn_Click()", "行车-取货任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), ex.Message);
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
            byte[] order = null;
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

                ABC abc = new ABC(dev);
                if (abc.ActionStatus() == ABC.Run)
                {
                    Notice.Show("指令发送失败：设备运行中！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                int x = Convert.ToInt32(xlocation.Text.Trim());
                int y = Convert.ToInt32(ylocation.Text.Trim());
                int z = Convert.ToInt32(zlocation.Text.Trim());
                order = ABC._TaskControl(ABC.TaskRelease, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("UnloadBtn_Click()", "行车-放货任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("放货任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, false);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("UnloadBtn_Click()", "行车-放货任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), ex.Message);
            }
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Relocate_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 终止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminateBtn_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            byte[] order = null;
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                ABC abc = new ABC(dev);
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                order = ABC._StopTask(abc.ABCNum());
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("TerminateBtn_Click()", "行车-终止任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, true);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("TerminateBtn_Click()", "行车-终止任务[ABC,指令]", dev, DataControl._mStools.BytetToString(order), ex.Message);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CBdev.SelectedIndex == -1)
            {
                Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                return;
            }
            string dev = CBdev.Text;
            DataControl._mSocket.SwithRefresh(dev, true);
        }

    }
}
