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
    /// W_ARF.xaml 的交互逻辑
    /// </summary>
    public partial class W_ARF : UserControl, ITabWin
    {
        private ArfDataGrid grid;
        private bool runRefresh = true;

        public W_ARF()
        {
            InitializeComponent();
            grid = new ArfDataGrid();

            DataContext = grid;

            getARFNameList();
            AddCombBox();

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
                if (location.IsFocused)
                {
                    if (Convert.ToInt32((string.IsNullOrEmpty(location.Text.Trim()) ? "0" : location.Text.Trim()) + e.Text)
                        > 255)   // 1位 byte
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        location.Text = "";
                        return;
                    }
                }
            }
        }

        private void getARFNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(DataControl._mMySql, DeviceType.摆渡车);
            foreach (var l in list)
            {
                grid.UpdateDeviceList(l.DEVICE, l.AREA);
            }
        }

        private void AddCombBox()
        {
            // 辊台内容
            //方式
            CBsite1.Items.Add("启动1#辊台");
            CBsite1.Items.Add("启动2#辊台");
            CBsite1.Items.Add("启动全部辊台");
            CBsite1.SelectedIndex = 0;
            //方向
            CBsite2.Items.Add("正向启动");
            CBsite2.Items.Add("反向启动");
            CBsite2.SelectedIndex = 0;
            //类型
            CBsite3.Items.Add("接货");
            CBsite3.Items.Add("送货");
            CBsite3.SelectedIndex = 0;
            //数量
            CBsite4.Items.Add("货物数量1");
            CBsite4.Items.Add("货物数量2");
            CBsite4.SelectedIndex = 0;
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
        /// 定位任务
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
                if (string.IsNullOrEmpty(location.Text.Trim()))
                {
                    Notice.Show("请填写目的坐标！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;

                ARF arf = new ARF(dev);
                if (arf.ActionStatus() == ARF.Run)
                {
                    Notice.Show("指令发送失败：设备运行中！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }
                if (arf.DeviceStatus() == ARF.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                int loc = Convert.ToInt32(location.Text.Trim());
                order = ARF._Position(arf.ARFNum(), (byte)loc);
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("LocateBtn_Click()", "摆渡车-定位任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("定位任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, false);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("LocateBtn_Click()", "摆渡车-定位任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), ex.ToString());
            }
        }

        /// <summary>
        /// 启动辊台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNrun_Click(object sender, EventArgs e)
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
                ARF arf = new ARF(dev);
                if (arf.ActionStatus() == ARF.Run)
                {
                    Notice.Show("指令发送失败：设备运行中！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }
                if (arf.DeviceStatus() == ARF.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                // 方式
                byte site1 = ARF.RollerRun1;
                if (CBsite1.SelectedValue.ToString() == "启动2#辊台")
                {
                    site1 = ARF.RollerRun2;
                }
                if (CBsite1.SelectedValue.ToString() == "启动全部辊台")
                {
                    site1 = ARF.RollerRunAll;
                }
                // 方向
                byte site2 = ARF.RunFront;
                if (CBsite2.SelectedValue.ToString() == "反向启动")
                {
                    site2 = ARF.RunObverse;
                }
                // 类型
                byte site3 = ARF.GoodsReceive;
                if (CBsite3.SelectedValue.ToString() == "送货")
                {
                    site3 = ARF.GoodsDeliver;
                }
                // 数量
                byte site4 = ARF.GoodsQty1;
                if (CBsite4.SelectedValue.ToString() == "货物数量2")
                {
                    site4 = ARF.GoodsQty2;
                }

                order = ARF._RollerControl(arf.ARFNum(), site1, site2, site3, site4);
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("BTNrun_Click()", "摆渡车-启动辊台任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("启动辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, false);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("BTNrun_Click()", "摆渡车-启动辊台任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), ex.ToString());
            }
        }

        /// <summary>
        /// 停止辊台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNstop_Click(object sender, EventArgs e)
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
                ARF arf = new ARF(dev);
                if (arf.DeviceStatus() == ARF.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                order = ARF._StopRoller(arf.ARFNum());
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("BTNstop_Click()", "摆渡车-停止辊台任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, true);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("BTNstop_Click()", "摆渡车-停止辊台任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), ex.ToString());
            }
        }

        /// <summary>
        /// 终止任务
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
                ARF arf = new ARF(dev);
                if (arf.DeviceStatus() == ARF.DeviceError)
                {
                    Notice.Show("指令发送失败：设备故障！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                order = ARF._StopTask(arf.ARFNum());
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    Notice.Show("指令发送失败：" + result.ToString(), "错误", 3, MessageBoxIcon.Error);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("TerminateBtn_Click()", "摆渡车-终止任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), result.ToString());
                    return;
                }
                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
                DataControl._mSocket.SwithRefresh(dev, true);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("TerminateBtn_Click()", "摆渡车-终止任务[ARF,指令]", dev, DataControl._mStools.BytetToString(order), ex.ToString());
            }
        }

    }
}
