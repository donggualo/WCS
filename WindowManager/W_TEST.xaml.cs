using ModuleManager.WCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManager;
using TaskManager.Devices;
using TaskManager.Functions;
using WcsHttpManager;

namespace WindowManager
{
    /// <summary>
    /// W_TEST.xaml 的交互逻辑
    /// </summary>
    public partial class W_TEST : Window
    {
        public W_TEST()
        {
            InitializeComponent();

            // 选项框
            AddCombBoxForWMS("A01", CBfrt_P);
            AddCombBoxForWMS("B01", CBfrt_D);

            AddCombBoxForDEV();
            UpdateWindow();
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        // 设置时间格式
        private void DataGrid_TimeFormat(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy/MM/dd HH:mm:ss";
            }
        }

        // 限制仅输入数字
        private void InputNum(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }

        #region WMS

        /// <summary>
        /// Add 选项
        /// </summary>
        /// <param name="area"></param>
        /// <param name="box"></param>
        private void AddCombBoxForWMS(string area, System.Windows.Controls.ComboBox box)
        {
            try
            {
                string sql = string.Format(@"select distinct DEVICE from wcs_config_device where TYPE = 'FRT' and AREA = '{0}' and (FLAG = 'Y'
                                                 or LOCK_WCS_NO not in (select WCS_NO From wcs_command_master where TASK_UID_2 is not null))", area);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 遍历执行入库任务
                foreach (WCS_CONFIG_DEVICE dev in devList)
                {
                    box.Items.Add(dev.DEVICE);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        /// <summary>
        /// 分配卸货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFRT_Click(object sender, RoutedEventArgs e)
        {
            string frtP = CBfrt_P.Text.Trim();
            string code = TBcode.Text.Trim();
            string frtD = CBfrt_D.Text.Trim();

            if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(frtD))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and BARCODE = '{0}'", code, TaskSite.完成);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (!DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("货物条码已存在任务！");
                    return;
                }
                // 无Task资讯则新增
                // 呼叫WMS 请求入库资讯---区域
                WmsModel wms = new WmsModel()
                {
                    Task_UID = "NW" + System.DateTime.Now.ToString("yyMMddHHmmss"),
                    Task_type = WmsStatus.StockInTask,
                    Barcode = code,
                    W_S_Loc = frtP,
                    W_D_Loc = frtD
                };
                // 写入数据库
                if (new ForWMSControl().WriteTaskToWCS(wms, out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败:" + result);
                }
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask(frtP, code))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }

        /// <summary>
        /// 分配货位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLOC_Click(object sender, RoutedEventArgs e)
        {
            string frtP = CBfrt_P.Text.Trim();
            string code = TBcode.Text.Trim();
            string frtD = CBfrt_D.Text.Trim();

            if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(frtD))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
                {
                    MessageBox.Show("货位不能为空！", "Error");
                    return;
                }
                // 货位
                string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(2, '0') + "-" + TBlocZ.Text.Trim().PadLeft(2, '0');
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.入库);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("不存在任务Task资讯！", "Error");
                    return;
                }

                // 获取对应任务ID
                string taskuid = dt.Rows[0]["TASK_UID"].ToString();
                // 更新任务资讯
                sql = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '{2}' where TASK_UID = '{3}'",
                    TaskType.入库, frtD, LOC, taskuid);
                DataControl._mMySql.ExcuteSql(sql);

                // 对应 WCS 清单
                DataControl._mTaskTools.CreateCommandIn(taskuid, frtD);
                MessageBox.Show("完成！");
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask_Loc(frtP, code))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }

        /// <summary>
        /// 货位出库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOUT_Click(object sender, RoutedEventArgs e)
        {
            // 无用资讯
            CBfrt_P.Text = "";
            TBcode.Text = "";

            string frtD = CBfrt_D.Text.Trim();
            if (string.IsNullOrEmpty(frtD))
            {
                MessageBox.Show("卸货点不能为空！", "Error");
                return;
            }
            if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
            {
                MessageBox.Show("货位不能为空！", "Error");
                return;
            }
            // 货位
            string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(2, '0') + "-" + TBlocZ.Text.Trim().PadLeft(2, '0');

            if ((bool)CheckWMS.IsChecked)
            {
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and TASK_TYPE = '{2}' and W_S_LOC = '{0}'", LOC, TaskSite.完成, TaskType.出库);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (!DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("该货位已存在出库任务！");
                    return;
                }
                // 无Task资讯则新增
                // 呼叫WMS 请求入库资讯---区域
                WmsModel wms = new WmsModel()
                {
                    Task_UID = "NW" + System.DateTime.Now.ToString("yyMMddHHmmss"),
                    Task_type = WmsStatus.StockOutTask,
                    Barcode = "",
                    W_S_Loc = LOC,
                    W_D_Loc = frtD
                };
                // 写入数据库
                if (new ForWMSControl().WriteTaskToWCS(wms, out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！" + result);
                }
            }
            else
            {
                MessageBox.Show("无法请求WMS出库！");
            }
        }

        #endregion

        #region 设备控制

        string checkType;
        string checkArea;
        /// <summary>
        /// 更新设备号内容
        /// </summary>
        private void ResetDev()
        {
            string type = CBdevtype.SelectedValue.ToString();
            string area = CBdevarea.SelectedValue.ToString();
            if (checkType == type && checkArea == area)
            {
                return;
            }
            checkType = type;
            checkArea = area;
            CBnum.Items.Clear();
            switch (type)
            {
                case "固定辊台":
                    type = DeviceType.固定辊台;
                    break;
                case "摆渡车":
                    type = DeviceType.摆渡车;
                    break;
                case "运输车":
                    type = DeviceType.运输车;
                    break;
                case "行车":
                    type = DeviceType.行车;
                    break;
                default:
                    break;
            }

            string sql = string.Format(@"select distinct DEVICE from wcs_config_device where TYPE = '{0}' and AREA = '{1}'", type, area);
            DataTable dt = DataControl._mMySql.SelectAll(sql);
            if (DataControl._mStools.IsNoData(dt))
            {
                return;
            }
            List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
            foreach (WCS_CONFIG_DEVICE dev in devList)
            {
                CBnum.Items.Add(dev.DEVICE);
            }
        }

        private void CBnum_DropDownOpened(object sender, EventArgs e)
        {
            ResetDev();
        }

        FRT frt;
        ARF arf;
        RGV rgv;
        ABC abc;
        string dev;

        /// <summary>
        /// 获取当前设备资讯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetDevData_Click(object sender, EventArgs e)
        {
            try
            {
                dev = CBnum.SelectedValue.ToString();
                string type = CBdevtype.SelectedValue.ToString();
                UpdateWindow();

                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(dev))
                {
                    LAdevice.Content = "— — — —";
                    LAactS.Content = "— — — —";
                    LAdevS.Content = "— — — —";
                    LAcmdS.Content = "— — — —";
                    LAtask.Content = "— — — —";
                    LAfinish.Content = "— — — —";
                    LAlocC.Content = "— — — —";
                    LAlocT.Content = "— — — —";
                    LAgoodsS.Content = "— — — —";
                    LAerr.Content = "— — — —";
                    LArollS.Content = "— — — —";
                    LArollD.Content = "— — — —";
                    LAdevtype.Content = "— — — —";
                    return;
                }

                switch (type)
                {
                    case "固定辊台":
                        frt = new FRT(dev);

                        LAdevice.Content = frt.FRTNum();
                        LAactS.Content = frt.ActionStatus() == FRT.Stop ? "停止" : "运行中";
                        LAdevS.Content = frt.DeviceStatus() == FRT.DeviceError ? "故障" : "正常";
                        LAcmdS.Content = frt.CommandStatus() == FRT.CommandError ? "命令异常" : "命令正常";
                        LAtask.Content = frt.CurrentTask() == FRT.TaskTake ? "辊台任务" : "停止辊台任务";
                        LAfinish.Content = frt.FinishTask() == FRT.TaskTake ? "辊台任务" : "停止辊台任务";
                        LAlocC.Content = "— — — —";
                        LAlocT.Content = "— — — —";
                        LAgoodsS.Content = FRT.GetGoodsStatusMes(frt.GoodsStatus());
                        LAerr.Content = "— — — —";
                        LArollS.Content = FRT.GetRollerStatusMes(frt.CurrentStatus());
                        LArollD.Content = frt.RunDirection() == FRT.RunFront ? "正向启动" : "反向启动";
                        LAdevtype.Content = DeviceType.固定辊台;

                        BTNover.Visibility = Visibility.Visible;
                        CBsite1.Visibility = Visibility.Visible;
                        CBsite2.Visibility = Visibility.Visible;
                        CBsite3.Visibility = Visibility.Visible;
                        CBsite4.Visibility = Visibility.Visible;
                        BTNrun.Visibility = Visibility.Visible;
                        BTNstop.Visibility = Visibility.Visible;
                        break;
                    case "摆渡车":
                        arf = new ARF(dev);

                        LAdevice.Content = arf.ARFNum();
                        LAactS.Content = arf.ActionStatus() == ARF.Stop ? "停止" : "运行中";
                        LAdevS.Content = arf.DeviceStatus() == ARF.DeviceError ? "故障" : "正常";
                        LAcmdS.Content = arf.CommandStatus() == ARF.CommandError ? "命令异常" : "命令正常";
                        LAtask.Content = ARF.GetTaskMes(arf.CurrentTask());
                        LAfinish.Content = ARF.GetTaskMes(arf.FinishTask());
                        LAlocC.Content = arf.CurrentSite();
                        LAlocT.Content = arf.Goods1site();
                        LAgoodsS.Content = ARF.GetGoodsStatusMes(arf.GoodsStatus());
                        LAerr.Content = "— — — —";
                        LArollS.Content = ARF.GetRollerStatusMes(arf.CurrentStatus());
                        LArollD.Content = arf.RunDirection() == ARF.RunFront ? "正向启动" : "反向启动";
                        LAdevtype.Content = DeviceType.摆渡车;

                        BTNover.Visibility = Visibility.Visible;
                        CBsite1.Visibility = Visibility.Visible;
                        CBsite2.Visibility = Visibility.Visible;
                        CBsite3.Visibility = Visibility.Visible;
                        CBsite4.Visibility = Visibility.Visible;
                        BTNrun.Visibility = Visibility.Visible;
                        BTNstop.Visibility = Visibility.Visible;
                        TBlocARF.Visibility = Visibility.Visible;
                        BTNlocate.Visibility = Visibility.Visible;
                        break;
                    case "运输车":
                        rgv = new RGV(dev);

                        LAdevice.Content = rgv.RGVNum();
                        LAactS.Content = rgv.ActionStatus() == RGV.Stop ? "停止" : "运行中";
                        LAdevS.Content = rgv.DeviceStatus() == RGV.DeviceError ? "故障" : "正常";
                        LAcmdS.Content = rgv.CommandStatus() == RGV.CommandError ? "命令异常" : "命令正常";
                        LAtask.Content = RGV.GetTaskMes(rgv.CurrentTask());
                        LAfinish.Content = RGV.GetTaskMes(rgv.FinishTask());
                        LAlocC.Content = rgv.GetCurrentSite();
                        LAlocT.Content = rgv.GetGoodsSite();
                        LAgoodsS.Content = RGV.GetGoodsStatusMes(rgv.GoodsStatus());
                        LAerr.Content = "— — — —";
                        LArollS.Content = RGV.GetRollerStatusMes(rgv.CurrentStatus());
                        LArollD.Content = rgv.RunDirection() == RGV.RunFront ? "正向启动" : "反向启动";
                        LAdevtype.Content = DeviceType.运输车;

                        BTNover.Visibility = Visibility.Visible;
                        CBsite1.Visibility = Visibility.Visible;
                        CBsite2.Visibility = Visibility.Visible;
                        CBsite3.Visibility = Visibility.Visible;
                        CBsite4.Visibility = Visibility.Visible;
                        BTNrun.Visibility = Visibility.Visible;
                        BTNstop.Visibility = Visibility.Visible;
                        TBlocRGV.Visibility = Visibility.Visible;
                        BTNlocate.Visibility = Visibility.Visible;
                        break;
                    case "行车":
                        abc = new ABC(dev);

                        LAdevice.Content = abc.ABCNum();
                        LAactS.Content = abc.ActionStatus() == ABC.Stop ? "停止" : "运行中";
                        LAdevS.Content = abc.DeviceStatus() == ABC.DeviceError ? "故障" : "正常";
                        LAcmdS.Content = abc.CommandStatus() == ABC.CommandError ? "命令异常" : "命令正常";
                        LAtask.Content = ABC.GetTaskMes(abc.CurrentTask());
                        LAfinish.Content = ABC.GetTaskMes(abc.FinishTask());
                        LAlocC.Content = abc.GetCurrentSite();
                        LAlocT.Content = abc.GetGoodsSite();
                        LAgoodsS.Content = abc.GoodsStatus() == ABC.GoodsNo ? "无货" : "有货";
                        LAerr.Content = "— — — —";
                        LArollS.Content = "— — — —";
                        LArollD.Content = "— — — —";
                        LAdevtype.Content = DeviceType.行车;

                        BTNover.Visibility = Visibility.Visible;
                        TBlocABCx.Visibility = Visibility.Visible;
                        TBlocABCy.Visibility = Visibility.Visible;
                        TBlocABCz.Visibility = Visibility.Visible;
                        BTNlocate.Visibility = Visibility.Visible;
                        BTNtake.Visibility = Visibility.Visible;
                        BTNrelease.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接设备获取资讯失败：" + ex.ToString());
            }
        }

        private void UpdateWindow()
        {
            BTNover.Visibility = Visibility.Hidden;
            CBsite1.Visibility = Visibility.Hidden;
            CBsite2.Visibility = Visibility.Hidden;
            CBsite3.Visibility = Visibility.Hidden;
            CBsite4.Visibility = Visibility.Hidden;
            BTNrun.Visibility = Visibility.Hidden;
            BTNstop.Visibility = Visibility.Hidden;
            TBlocARF.Visibility = Visibility.Hidden;
            TBlocRGV.Visibility = Visibility.Hidden;
            TBlocABCx.Visibility = Visibility.Hidden;
            TBlocABCy.Visibility = Visibility.Hidden;
            TBlocABCz.Visibility = Visibility.Hidden;
            BTNlocate.Visibility = Visibility.Hidden;
            BTNtake.Visibility = Visibility.Hidden;
            BTNrelease.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNover_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] order = null;
                string type = LAdevtype.Content.ToString();
                if (type == "— — — —")
                {
                    return;
                }
                switch (type)
                {
                    case DeviceType.固定辊台:
                        order = FRT._StopTask(frt.FRTNum());

                        break;
                    case DeviceType.摆渡车:
                        order = ARF._StopTask(arf.ARFNum());

                        break;
                    case DeviceType.运输车:
                        order = RGV._StopTask(rgv.RGVNum());

                        break;
                    case DeviceType.行车:
                        order = ABC._StopTask(abc.ABCNum());

                        break;
                }
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
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
                byte[] order = null;
                string type = LAdevtype.Content.ToString();
                if (type == "— — — —")
                {
                    return;
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

                switch (type)
                {
                    case DeviceType.固定辊台:
                        if (frt.ActionStatus() == FRT.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (frt.DeviceStatus() == FRT.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = FRT._RollerControl(frt.FRTNum(), site1, site2, site3, site4);

                        break;
                    case DeviceType.摆渡车:
                        if (arf.ActionStatus() == ARF.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (arf.DeviceStatus() == ARF.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = ARF._RollerControl(arf.ARFNum(), site1, site2, site3, site4);

                        break;
                    case DeviceType.运输车:
                        if (rgv.ActionStatus() == RGV.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (rgv.DeviceStatus() == RGV.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = RGV._RollerControl(rgv.RGVNum(), site1, site2, site3, site4);

                        break;
                }
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
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
                byte[] order = null;
                string type = LAdevtype.Content.ToString();
                if (type == "— — — —")
                {
                    return;
                }
                switch (type)
                {
                    case DeviceType.固定辊台:
                        if (frt.DeviceStatus() == FRT.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = FRT._StopRoller(frt.FRTNum());

                        break;
                    case DeviceType.摆渡车:
                        if (arf.DeviceStatus() == ARF.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = ARF._StopRoller(arf.ARFNum());

                        break;
                    case DeviceType.运输车:
                        if (rgv.DeviceStatus() == RGV.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = RGV._StopRoller(rgv.RGVNum());

                        break;
                }
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 定位任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNlocate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] order = null;
                string type = LAdevtype.Content.ToString();
                if (type == "— — — —")
                {
                    return;
                }
                switch (type)
                {
                    case DeviceType.摆渡车:
                        if (string.IsNullOrEmpty(TBlocARF.Text.Trim()))
                        {
                            MessageBox.Show("请填写坐标！");
                            return;
                        }
                        int ARFloc = Convert.ToInt32(TBlocARF.Text.Trim());

                        if (arf.ActionStatus() == ARF.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (arf.DeviceStatus() == ARF.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = ARF._Position(arf.ARFNum(), (byte)ARFloc);

                        break;
                    case DeviceType.运输车:
                        if (string.IsNullOrEmpty(TBlocRGV.Text.Trim()))
                        {
                            MessageBox.Show("请填写坐标！");
                            return;
                        }
                        int RGVloc = Convert.ToInt32(TBlocRGV.Text.Trim());

                        if (rgv.ActionStatus() == RGV.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (rgv.DeviceStatus() == RGV.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = RGV._Position(rgv.RGVNum(), DataControl._mStools.IntToBytes(RGVloc));

                        break;
                    case DeviceType.行车:
                        if (string.IsNullOrEmpty(TBlocABCx.Text.Trim()) || string.IsNullOrEmpty(TBlocABCy.Text.Trim()) || string.IsNullOrEmpty(TBlocABCz.Text.Trim()))
                        {
                            MessageBox.Show("请填写坐标！");
                            return;
                        }
                        int x = Convert.ToInt32(TBlocABCx.Text.Trim());
                        int y = Convert.ToInt32(TBlocABCy.Text.Trim());
                        int z = Convert.ToInt32(TBlocABCz.Text.Trim());

                        if (abc.ActionStatus() == ABC.Run)
                        {
                            MessageBox.Show("设备运行中！");
                        }
                        if (abc.DeviceStatus() == ABC.DeviceError)
                        {
                            MessageBox.Show("设备故障！");
                        }
                        order = ABC._TaskControl(ABC.TaskLocate, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));

                        break;
                }
                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 取货任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNtake_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] order = null;
                if (string.IsNullOrEmpty(TBlocABCx.Text.Trim()) || string.IsNullOrEmpty(TBlocABCy.Text.Trim()) || string.IsNullOrEmpty(TBlocABCz.Text.Trim()))
                {
                    MessageBox.Show("请填写坐标！");
                    return;
                }
                int x = Convert.ToInt32(TBlocABCx.Text.Trim());
                int y = Convert.ToInt32(TBlocABCy.Text.Trim());
                int z = Convert.ToInt32(TBlocABCz.Text.Trim());

                if (abc.ActionStatus() == ABC.Run)
                {
                    MessageBox.Show("设备运行中！");
                }
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    MessageBox.Show("设备故障！");
                }
                order = ABC._TaskControl(ABC.TaskRelease, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));

                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 放货任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNrelease_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] order = null;
                if (string.IsNullOrEmpty(TBlocABCx.Text.Trim()) || string.IsNullOrEmpty(TBlocABCy.Text.Trim()) || string.IsNullOrEmpty(TBlocABCz.Text.Trim()))
                {
                    MessageBox.Show("请填写坐标！");
                    return;
                }
                int x = Convert.ToInt32(TBlocABCx.Text.Trim());
                int y = Convert.ToInt32(TBlocABCy.Text.Trim());
                int z = Convert.ToInt32(TBlocABCz.Text.Trim());

                if (abc.ActionStatus() == ABC.Run)
                {
                    MessageBox.Show("设备运行中！");
                }
                if (abc.DeviceStatus() == ABC.DeviceError)
                {
                    MessageBox.Show("设备故障！");
                }
                order = ABC._TaskControl(ABC.TaskRelease, abc.ABCNum(), DataControl._mStools.IntToBytes(x), DataControl._mStools.IntToBytes(y), DataControl._mStools.IntToBytes(z));

                if (!DataControl._mSocket.SendToClient(dev, order, out string result))
                {
                    throw new Exception(result);
                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.ToString());
            }
        }
        #endregion

        private void AddCombBoxForDEV()
        {
            try
            {
                // 搜索设备类型
                CBdev.Items.Add(" ");
                CBdev.Items.Add(DeviceType.固定辊台 + " : 固定辊台");
                CBdev.Items.Add(DeviceType.摆渡车 + " : 摆渡车");
                CBdev.Items.Add(DeviceType.运输车 + " : 运输车");
                CBdev.Items.Add(DeviceType.行车 + " : 行车");
                CBdev.SelectedIndex = 0;

                // 搜索设备区域
                CBarea.Items.Add(" ");
                CBarea.SelectedIndex = 0;
                String sql = "select distinct AREA from wcs_config_device";
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> areaList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                foreach (WCS_CONFIG_DEVICE area in areaList)
                {
                    CBarea.Items.Add(area.AREA);
                    CBdevarea.Items.Add(area.AREA);
                }

                // 所属设备类型
                CBtype.Items.Add("固定辊台");
                CBtype.Items.Add("摆渡车");
                CBtype.Items.Add("运输车");
                CBtype.Items.Add("行车");

                CBdevtype.Items.Add("固定辊台");
                CBdevtype.Items.Add("摆渡车");
                CBdevtype.Items.Add("运输车");
                CBdevtype.Items.Add("行车");

                CBdevtype.SelectedIndex = 0;
                CBdevarea.SelectedIndex = 0;

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
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        #region 设备设定

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshDev_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空数据
                DGdevice.ItemsSource = null;

                string sql = @"select DEVICE 设备号, IP, PORT, AREA 所属区域, REMARK 备注, 
             (case when TYPE = 'FRT' then '固定辊台'
			       when TYPE = 'ARF' then '摆渡车'
						 when TYPE = 'RGV' then '运输车' else '行车' end) 设备类型, 
			 (case when FLAG = 'L' then '锁定'
					   when FLAG = 'Y' then '空闲' else '未知' end) 状态, LOCK_WCS_NO 锁定清单号, CREATION_TIME 创建时间, UPDATE_TIME 更新时间
             from wcs_config_device where 1=1";
                if (!string.IsNullOrWhiteSpace(CBdev.Text))
                {
                    sql = sql + string.Format(" and TYPE = '{0}'", CBdev.Text.Substring(0, 3));
                }
                if (!string.IsNullOrWhiteSpace(CBarea.Text))
                {
                    sql = sql + string.Format(" and AREA = '{0}'", CBarea.Text.Substring(0, 3));
                }
                // 获取数据
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                DGdevice.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 重连设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReLinkDev_Click(object sender, EventArgs e)
        {
            try
            {
                DataControl._mTaskTools.LinkDevicesClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("重连失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取所选数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGdevice_DoubleClick(object sender, System.EventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }

                TBdevice.Text = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();
                TBip.Text = (DGdevice.SelectedItem as DataRowView)["IP"].ToString();
                TBport.Text = (DGdevice.SelectedItem as DataRowView)["PORT"].ToString();
                TBarea.Text = (DGdevice.SelectedItem as DataRowView)["所属区域"].ToString();
                TBmark.Text = (DGdevice.SelectedItem as DataRowView)["备注"].ToString();
                CBtype.Text = (DGdevice.SelectedItem as DataRowView)["设备类型"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    MessageBox.Show("请选中目标！", "提示");
                    return;
                }

                string flag = (DGdevice.SelectedItem as DataRowView)["状态"].ToString();
                if (flag == "锁定")
                {
                    MessageBox.Show("设备已锁定，暂无法修改！", "提示");
                    return;
                }

                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();
                string ip = (DGdevice.SelectedItem as DataRowView)["IP"].ToString();
                string port = (DGdevice.SelectedItem as DataRowView)["PORT"].ToString();

                string deviceNew = TBdevice.Text.Trim();
                string ipNew = TBip.Text.Trim();
                string portNew = TBport.Text.Trim();
                string area = TBarea.Text.Trim();
                string remark = TBmark.Text.Trim();
                string type = CBtype.Text;
                switch (type)
                {
                    case "固定辊台":
                        type = DeviceType.固定辊台;
                        break;
                    case "摆渡车":
                        type = DeviceType.摆渡车;
                        break;
                    case "运输车":
                        type = DeviceType.运输车;
                        break;
                    case "行车":
                        type = DeviceType.行车;
                        break;
                }

                if (string.IsNullOrEmpty(deviceNew) || string.IsNullOrEmpty(ipNew) || string.IsNullOrEmpty(portNew) ||
                    string.IsNullOrEmpty(area) || string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("请填入明细！", "提示");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("确认修改设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlupdate = String.Format(@"update wcs_config_device set DEVICE = '{0}',IP = '{1}',PORT = '{2}',TYPE = '{3}', AREA = '{4}',REMARK = '{5}', UPDATE_TIME = NOW() 
                    where DEVICE = '{6}' and IP = '{7}' and PORT = '{8}'", deviceNew, ipNew, portNew, type, area, remark, device, ip, port);
                DataControl._mMySql.ExcuteSql(sqlupdate);

                MessageBox.Show("修改成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("PRIMARY"))
                {
                    MessageBox.Show("添加失败： 重复设备号！", "Error");
                }
                else if (ex.ToString().Contains("IP_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复IP！", "Error");
                }
                else if (ex.ToString().Contains("PORT_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复PORT！", "Error");
                }
                else
                {
                    MessageBox.Show("添加失败： " + ex.ToString(), "Error");
                }
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string device = TBdevice.Text.Trim();
                string ip = TBip.Text.Trim();
                string port = TBport.Text.Trim();
                string area = TBarea.Text.Trim();
                string remark = TBmark.Text.Trim();
                string type = CBtype.Text;
                switch (type)
                {
                    case "固定辊台":
                        type = DeviceType.固定辊台;
                        break;
                    case "摆渡车":
                        type = DeviceType.摆渡车;
                        break;
                    case "运输车":
                        type = DeviceType.运输车;
                        break;
                    case "行车":
                        type = DeviceType.行车;
                        break;
                }

                if (string.IsNullOrEmpty(device) || string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) ||
                    string.IsNullOrEmpty(area) || string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("请填入明细！", "提示");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("确认添加设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlinsert = String.Format(@"insert into wcs_config_device(DEVICE,IP,PORT,AREA,REMARK,TYPE,FLAG) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','N')",
                    device, ip, port, area, remark, type);
                DataControl._mMySql.ExcuteSql(sqlinsert);

                MessageBox.Show("添加成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("PRIMARY"))
                {
                    MessageBox.Show("添加失败： 重复设备号！", "Error");
                }
                else if (ex.ToString().Contains("IP_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复IP！", "Error");
                }
                else if (ex.ToString().Contains("PORT_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复PORT！", "Error");
                }
                else
                {
                    MessageBox.Show("添加失败： " + ex.ToString(), "Error");
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();

                MessageBoxResult result = MessageBox.Show("确认删除设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"delete from wcs_config_device where DEVICE = '{0}'", device);
                DataControl._mMySql.ExcuteSql(sqldelete);

                MessageBox.Show("删除成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败： " + ex.ToString(), "Error");
            }
        }

        /// <summary>
        /// 导出Excel文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveToExcel_Click(object sender, EventArgs e)
        {
            DataControl._mStools.SaveToExcel(DGdevice);
        }

        #endregion

    }

}
