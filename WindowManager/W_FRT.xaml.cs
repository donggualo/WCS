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
    /// W_FRT.xaml 的交互逻辑
    /// </summary>
    public partial class W_FRT : UserControl, ITabWin
    {
        private FrtDataGrid grid;
        private bool runRefresh = true;

        public W_FRT()
        {
            InitializeComponent();
            grid = new FrtDataGrid();

            DataContext = grid;

            GetFRTNameList();
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
        private void GetFRTNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDevInfo(DeviceType.固定辊台);
            foreach (var l in list)
            {
                grid.UpdateDeviceList(l.DEVICE, l.AREA);
            }
        }

        private void AddCombBox()
        {
            // 辊台内容
            //方式
            CBsite1.Items.Add((int)RollerStatusEnum.辊台1启动 + ":启动1辊台");
            CBsite1.Items.Add((int)RollerStatusEnum.辊台2启动 + ":启动2辊台");
            CBsite1.Items.Add((int)RollerStatusEnum.辊台全启动 + ":启动全辊台");
            CBsite1.SelectedIndex = 0;
            //方向
            CBsite2.Items.Add((int)RollerDiretionEnum.正向 + ":正向");
            CBsite2.Items.Add((int)RollerDiretionEnum.反向 + ":反向");
            CBsite2.SelectedIndex = 0;
            //类型
            CBsite3.Items.Add((int)RollerTypeEnum.接货 + ":接货");
            CBsite3.Items.Add((int)RollerTypeEnum.送货 + ":送货");
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
        /// 启动辊台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNrun_Click(object sender, EventArgs e)
        {
            string dev = "";
            try
            {
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

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

                if (!WindowCommon.ConfirmAction("是否发送辊台指令！！"))
                {
                    return;
                }

                ADS.mFrt.devices.Find(c => c.devName == dev).ControlRoller(
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
                CommonSQL.LogErr("BTNrun_Click()", "固定辊台界面启动辊台[设备号]", ex.Message, dev);
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
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

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

                ADS.mFrt.devices.Find(c => c.devName == dev).StopRoller();

                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNstop_Click()", "固定辊台界面停止辊台[设备号]", ex.Message, dev);
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
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

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

                ADS.mFrt.devices.Find(c => c.devName == dev).StopTask();

                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNstop_Click()", "固定辊台界面终止[设备号]", ex.Message, dev);
            }
        }

        /// <summary>
        /// 接货到扫码辊台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNlinkAGV_Click(object sender, RoutedEventArgs e)
        {
            string dev = "";
            try
            {
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

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

                // 只启动辊台2扫码
                ADS.mFrt.devices.Find(c => c.devName == dev).StartTakeRoll(TaskTypeEnum.入库, 1, true);

                Notice.Show("辊台任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
                // LOG
                CommonSQL.LogErr("BTNlinkAGV_Click()", "固定辊台界面接货扫码[设备号]", ex.Message, dev);
            }
        }
    }
}
