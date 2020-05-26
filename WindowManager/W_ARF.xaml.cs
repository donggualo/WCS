using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Module;
using ModuleManager;
using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using WindowManager.Datagrid;

using ADS = WcsManager.Administartor;

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

            GetARFNameList();
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
                        > 255)   // 0xFF
                    {
                        Notice.Show("输入值过大！请重新输入！", "提示", 3, MessageBoxIcon.Info);
                        location.Text = "";
                        return;
                    }
                }
            }
        }

        private void GetARFNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(DeviceType.摆渡车);
            foreach (var l in list)
            {
                grid.UpdateDeviceList(l.DEVICE, l.AREA);
            }
        }

        private void AddCombBox()
        {
            // 辊台内容
            //方式
            CBsite1.Items.Add(RollerStatusEnum.辊台1启动.GetHashCode().ToString() + ":启动1辊台");
            CBsite1.Items.Add(RollerStatusEnum.辊台2启动.GetHashCode().ToString() + ":启动2辊台");
            CBsite1.Items.Add(RollerStatusEnum.辊台全启动.GetHashCode().ToString() + ":启动全辊台");
            CBsite1.SelectedIndex = 0;
            //方向
            CBsite2.Items.Add(RollerDiretionEnum.正向.GetHashCode().ToString() + ":正向");
            CBsite2.Items.Add(RollerDiretionEnum.反向.GetHashCode().ToString() + ":反向");
            CBsite2.SelectedIndex = 0;
            //类型
            CBsite3.Items.Add(RollerTypeEnum.接货.GetHashCode().ToString() + ":接货");
            CBsite3.Items.Add(RollerTypeEnum.送货.GetHashCode().ToString() + ":送货");
            CBsite3.SelectedIndex = 0;
            //数量
            CBsite4.Items.Add("1:货物数量1");
            CBsite4.Items.Add("2:货物数量2");
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

                if (!WindowCommon.ConfirmAction("是否发送定位指令！！"))
                {
                    return;
                }

                int loc = Convert.ToInt32(location.Text.Trim());

                ADS.mArf.devices.Find(c => c.devName == dev).ToSite(loc);

                Notice.Show("定位任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("LocateBtn_Click()", "摆渡车界面定位[设备号]", ex.Message, dev);
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
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                if (!WindowCommon.ConfirmAction("是否发送滚筒启动指令！！"))
                {
                    return;
                }

                ADS.mArf.devices.Find(c => c.devName == dev).ControlRoller(
                    Convert.ToInt32(CBsite1.Text.Substring(0, 1)),
                    Convert.ToInt32(CBsite2.Text.Substring(0, 1)),
                    Convert.ToInt32(CBsite3.Text.Substring(0, 1)),
                    Convert.ToInt32(CBsite4.Text.Substring(0, 1)));

                Notice.Show("启动辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNrun_Click()", "摆渡车界面启动辊台[设备号]", ex.Message, dev);
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
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                ADS.mArf.devices.Find(c => c.devName == dev).StopRoller();

                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNstop_Click()", "摆渡车界面停止辊台[设备号]", ex.Message, dev);
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
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                dev = CBdev.Text;
                ADS.mArf.devices.Find(c => c.devName == dev).StopTask();

                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("TerminateBtn_Click()", "摆渡车界面终止[设备号]", ex.Message, dev);
            }
        }

    }
}
