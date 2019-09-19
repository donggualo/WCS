using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManager;
using TaskManager.Functions;
using ModuleManager.WCS;
using WcsHttpManager;
using System.Windows.Threading;
using Panuon.UI.Silver;
using PubResourceManager;

namespace WindowManager
{
    /// <summary>
    /// W_ManualWms.xaml 的交互逻辑
    /// </summary>
    public partial class W_ManualWms : UserControl
    {
        public W_ManualWms()
        {
            InitializeComponent();

            // 选项框
            AddCombBoxForWMS("A01", CBfrt_P);
            AddCombBoxForWMS("B01", CBfrt_D);

            //明细
            GetInfo();
            //OnTimeToLoadData();
        }

        // 限制仅输入数字
        private void InputNum(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
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

        /// <summary>
        /// Add 选项
        /// </summary>
        /// <param name="area"></param>
        /// <param name="box"></param>
        private void AddCombBoxForWMS(string area, System.Windows.Controls.ComboBox box)
        {
            try
            {
                string sql = string.Format(@"select distinct DEVICE from wcs_config_device where TYPE = 'FRT' and AREA = '{0}' and (FLAG in ('{1}','{2}')
                                                 or LOCK_WCS_NO not in (select WCS_NO From wcs_command_master where TASK_UID_2 is not null))", area, DeviceFlag.占用, DeviceFlag.空闲);
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
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取资讯
        /// </summary>
        private void GetInfo()
        {
            try
            {
                // 清空数据
                DGinfo.ItemsSource = null;

                string sql = @"select a.ID AGV任务ID, a.AGV AGV设备号, a.PICKSTATION AGV装货点, a.DROPSTATION AGV卸货点, 
             (case when a.MAGIC = '1' then '任务生成'
			       when a.MAGIC = '2' then '分配装货卸货点'
				   when a.MAGIC = '3' then '前往装货点'
				   when a.MAGIC = '4' then '到达装货点'
				   when a.MAGIC = '6' then '装货完成'
				   when a.MAGIC = '8' then '到达卸货点'
				   when a.MAGIC = '10' then '卸货完成'
				   when a.MAGIC = '11' then '任务完成'
				   when a.MAGIC = '254' then '重新定位卸货' else NULL end) AGV当前步骤,
                 a.CREATION_TIME AGV任务生成时间, B.CREATION_TIME WCS任务生成时间,
                 b.TASK_UID WCS任务ID, b.BARCODE 货物码, b.W_S_LOC 起点, b.W_D_LOC 终点,
			 (case when b.TASK_TYPE = '1' then '入库任务'
			       when b.TASK_TYPE = '2' then '出库任务'
				   when b.TASK_TYPE = '3' then '移仓任务'
				   when b.TASK_TYPE = '4' then '盘点任务'
				   when b.TASK_TYPE = '0' then 'AGV搬运' else NULL end) WCS任务类型,
			 (case when b.SITE = 'N' then '未执行'
			       when b.SITE = 'W' then '任务中'
				   when b.SITE = 'Y' then '完成'
				   when b.SITE = 'X' then '失效' else NULL end) WCS任务状态
	          from wcs_agv_info a left join wcs_task_info b on a.TASK_UID = b.TASK_UID";
                // 获取数据
                DGinfo.ItemsSource = DataControl._mMySql.SelectAll(sql).DefaultView;
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            GetInfo();
        }

        private void DGinfo_DoubleClick(object sender, System.EventArgs e)
        {
            try
            {
                CBfrt_P.Text = (DGinfo.SelectedItem as DataRowView)["AGV装货点"].ToString();
                CBfrt_D.Text = (DGinfo.SelectedItem as DataRowView)["AGV卸货点"].ToString();
                TBcode.Text = (DGinfo.SelectedItem as DataRowView)["货物码"].ToString();

                string loc = (DGinfo.SelectedItem as DataRowView)["终点"].ToString();
                if (!string.IsNullOrEmpty(loc))
                {
                    string[] LOC = loc.Split('-');
                    TBlocX.Text = LOC[0].ToString();
                    TBlocY.Text = LOC[1].ToString();
                    TBlocZ.Text = LOC[2].ToString();
                }
            }
            catch (Exception ex)
            {
                Notice.Show(ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 分配卸货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFRT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string frtP = CBfrt_P.Text.Trim();
                string code = TBcode.Text.Trim();
                string frtD = CBfrt_D.Text.Trim();

                if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
                {
                    Notice.Show("包装线辊台设备号 / 货物条码 不能为空！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                if ((bool)CheckWMS.IsChecked)
                {
                    if (string.IsNullOrEmpty(frtD))
                    {
                        Notice.Show("卸货点不能为空！", "错误", 3, MessageBoxIcon.Error);
                        return;
                    }
                    // 获取Task资讯
                    String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and BARCODE = '{0}'", code, TaskSite.完成);
                    DataTable dt = DataControl._mMySql.SelectAll(sql);
                    if (!DataControl._mStools.IsNoData(dt))
                    {
                        Notice.Show("货物条码已存在任务！", "提示", 3, MessageBoxIcon.Info);
                        return;
                    }
                    // 无Task资讯则新增
                    // 呼叫WMS 请求入库资讯---区域
                    WmsModel wms = new WmsModel()
                    {
                        Task_UID = "NW" + System.DateTime.Now.ToString("yyMMddHHmmss"),
                        Task_type = WmsStatus.StockInTask,
                        Barcode = code,
                        W_S_Loc = DataControl._mTaskTools.GetArea(frtP),
                        W_D_Loc = DataControl._mTaskTools.GetArea(frtD)
                    };
                    // 写入数据库
                    if (new ForWMSControl().WriteTaskToWCS(wms, out string result))
                    {
                        Notice.Show("完成！", "成功", 3, MessageBoxIcon.Success);
                    }
                    else
                    {
                        Notice.Show("失败:" + result, "错误", 3, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (new ForWMSControl().ScanCodeTask(frtP, code))
                    {
                        Notice.Show("完成！", "成功", 3, MessageBoxIcon.Success);
                    }
                    else
                    {
                        Notice.Show("失败！", "错误", 3, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Notice.Show(ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 分配货位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLOC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string frtP = CBfrt_P.Text.Trim();
                string code = TBcode.Text.Trim();
                string frtD = CBfrt_D.Text.Trim();

                if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
                {
                    Notice.Show("包装线辊台设备号 / 货物条码 不能为空！", "错误", 3, MessageBoxIcon.Error);
                    return;
                }

                if ((bool)CheckWMS.IsChecked)
                {
                    if (string.IsNullOrEmpty(frtD))
                    {
                        Notice.Show("卸货点不能为空！", "错误", 3, MessageBoxIcon.Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
                    {
                        Notice.Show("货位不能为空！", "错误", 3, MessageBoxIcon.Error);
                        return;
                    }
                    // 货位
                    string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(2, '0') + "-" + TBlocZ.Text.Trim().PadLeft(2, '0');
                    // 获取Task资讯
                    String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.入库);
                    DataTable dt = DataControl._mMySql.SelectAll(sql);
                    if (DataControl._mStools.IsNoData(dt))
                    {
                        Notice.Show("不存在任务Task资讯！", "错误", 3, MessageBoxIcon.Error);
                        return;
                    }

                    // 获取对应任务ID
                    string taskuid = dt.Rows[0]["TASK_UID"].ToString();
                    // 更新任务资讯
                    sql = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '{2}' where TASK_UID = '{3}'",
                        TaskType.入库, DataControl._mTaskTools.GetArea(frtD), LOC, taskuid);
                    DataControl._mMySql.ExcuteSql(sql);

                    // 对应 WCS 清单
                    DataControl._mTaskTools.CreateCommandIn(taskuid, frtD);
                    Notice.Show("完成！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    if (new ForWMSControl().ScanCodeTask_Loc(frtP, code))
                    {
                        Notice.Show("完成！", "成功", 3, MessageBoxIcon.Success);
                    }
                    else
                    {
                        Notice.Show("失败！", "错误", 3, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Notice.Show(ex.ToString(), "错误", 3, MessageBoxIcon.Error);
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
                Notice.Show("卸货点不能为空！", "错误", 3, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
            {
                Notice.Show("货位不能为空！", "错误", 3, MessageBoxIcon.Error);
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
                    Notice.Show("该货位已存在出库任务！", "错误", 3, MessageBoxIcon.Error);
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
                    Notice.Show("完成！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    MessageBox.Show("失败！" + result);
                    Notice.Show("失败！" + result, "错误", 3, MessageBoxIcon.Error);
                }
            }
            else
            {
                Notice.Show("无法请求WMS出库！", "错误", 3, MessageBoxIcon.Error);
            }
        }

        #region 定时刷新数据    

        /// <summary>
        /// 每隔一个时间段执行一段代码
        /// </summary>
        //private void OnTimeToLoadData()
        //{
        //    DispatcherTimer ShowTimer = new DispatcherTimer();
        //    //起个Timer一直获取当前时间
        //    ShowTimer.Tick += OnTimeLoadData;
        //    ShowTimer.Interval = new TimeSpan(0, 0, 0, 5, 0); //天，时，分，秒，毫秒
        //    ShowTimer.Start();
        //}

        /// <summary>
        /// 计时加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnTimeLoadData(object sender, EventArgs e)
        //{
        //    if (CBrefresh.IsChecked == true)
        //    {
        //        GetInfo(); // 获取数据
        //    }
        //}

        #endregion
    }
}
