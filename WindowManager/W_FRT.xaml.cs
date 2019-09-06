using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TaskManager;
using TaskManager.Devices;
using WindowManager.Datagrid;

namespace WindowManager
{
    /// <summary>
    /// W_FRT.xaml 的交互逻辑
    /// </summary>
    public partial class W_FRT : UserControl
    {
        private FrtDataGrid grid;


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

        private void GetFRTNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(DataControl._mMySql, DeviceType.固定辊台);
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
                while (true)
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
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string dev = CBdev.Text;
                FRT frt = new FRT(dev);
                if (frt.ActionStatus() == FRT.Run)
                {
                    throw new Exception("设备运行中！");
                }
                if (frt.DeviceStatus() == FRT.DeviceError)
                {
                    throw new Exception("设备故障！");
                }

                // 方式
                byte site1 = FRT.RollerRun1;
                if (CBsite1.SelectedValue.ToString() == "启动2#辊台")
                {
                    site1 = FRT.RollerRun2;
                }
                if (CBsite1.SelectedValue.ToString() == "启动全部辊台")
                {
                    site1 = FRT.RollerRunAll;
                }
                // 方向
                byte site2 = FRT.RunFront;
                if (CBsite2.SelectedValue.ToString() == "反向启动")
                {
                    site2 = FRT.RunObverse;
                }
                // 类型
                byte site3 = FRT.GoodsReceive;
                if (CBsite3.SelectedValue.ToString() == "送货")
                {
                    site3 = FRT.GoodsDeliver;
                }
                // 数量
                byte site4 = FRT.GoodsQty1;
                if (CBsite4.SelectedValue.ToString() == "货物数量2")
                {
                    site4 = FRT.GoodsQty2;
                }

                byte[] order = FRT._RollerControl(frt.FRTNum(), site1, site2, site3, site4);
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 停止辊台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNstop_Click(object sender, EventArgs e)
        {
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string dev = CBdev.Text;
                FRT frt = new FRT(dev);
                if (frt.DeviceStatus() == FRT.DeviceError)
                {
                    throw new Exception("设备故障！");
                }

                byte[] order = FRT._StopRoller(frt.FRTNum());
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                Notice.Show("停止辊台 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TerminateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CBdev.SelectedIndex == -1)
                {
                    Notice.Show("请选择设备！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string dev = CBdev.Text;
                FRT frt = new FRT(dev);
                if (frt.DeviceStatus() == FRT.DeviceError)
                {
                    throw new Exception("设备故障！");
                }

                byte[] order = FRT._StopTask(frt.FRTNum());
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                Notice.Show("终止任务 指令发送成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show("指令发送失败：" + ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }
    }
}
