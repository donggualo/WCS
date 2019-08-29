using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WCS_phase1.Action;
using WCS_phase1.Functions;
using WCS_phase1.Http;
using WCS_phase1.Models;

namespace WCS_phase1.WCSWindow
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
            AddCombBoxForDEV();
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        // 赋值 VIEW INDEX
        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
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

        #region 扫码

        /// <summary>
        /// Add 选项
        /// </summary>
        /// <param name="area"></param>
        /// <param name="box"></param>
        private void AddCombBoxForWMS(string area, System.Windows.Controls.ComboBox box)
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_device where TYPE = 'FRT' and AREA = '{0}' and (FLAG = 'Y'
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

        private void CheckWMS_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckWMS.IsChecked)
            {
                CBfrt_D.IsEnabled = true;
                TBlocX.IsEnabled = true;
                TBlocY.IsEnabled = true;
                TBlocZ.IsEnabled = true;
                AddCombBoxForWMS("B01", CBfrt_D);
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
        }

        /// <summary>
        /// 分配卸货点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFRT_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CBfrt_P.Text) || string.IsNullOrEmpty(TBcode.Text))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(CBfrt_D.Text))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and BARCODE = '{0}'", TBcode.Text, TaskSite.完成);
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
                    Task_UID = "Test" + System.DateTime.Now.ToString("MMddHHmmss"),
                    Task_type = WmsStatus.StockInTask,
                    Barcode = TBcode.Text,
                    W_S_Loc = CBfrt_P.Text,
                    W_D_Loc = CBfrt_D.Text
                };
                // 写入数据库
                if (new ForWMSControl().WriteTaskToWCS(wms))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask(CBfrt_P.Text, TBcode.Text))
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
            if (string.IsNullOrEmpty(CBfrt_P.Text) || string.IsNullOrEmpty(TBcode.Text))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(CBfrt_D.Text))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                if (string.IsNullOrEmpty(TBlocX.Text) && string.IsNullOrEmpty(TBlocY.Text) && string.IsNullOrEmpty(TBlocZ.Text))
                {
                    MessageBox.Show("货位不能为空！", "Error");
                    return;
                }
                string locX = TBlocX.Text.PadLeft(3, '0');
                string locY = TBlocY.Text.PadLeft(3, '0');
                string locZ = TBlocZ.Text.PadLeft(3, '0');
                string LOC = "C" + locX + "-" + locX + "-" + locZ;
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", TBcode.Text, TaskType.入库);
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
                    TaskType.入库, CBfrt_D.Text, LOC, taskuid);
                DataControl._mMySql.ExcuteSql(sql);

                // 对应 WCS 清单
                DataControl._mTaskTools.CreateCommandIn(taskuid, CBfrt_D.Text);
                MessageBox.Show("完成！");
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask_Loc(CBfrt_P.Text, TBcode.Text))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }

        #endregion

        #region 设备

        private void AddCombBoxForDEV()
        {
            try
            {
                CBdev.Items.Add(" ");
                CBdev.Items.Add(DeviceType.固定辊台 + " : 固定辊台");
                CBdev.Items.Add(DeviceType.摆渡车 + " : 摆渡车");
                CBdev.Items.Add(DeviceType.运输车 + " : 运输车");
                CBdev.Items.Add(DeviceType.行车 + " : 行车");
                CBdev.SelectedIndex = 0;

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
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

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
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDev_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDev_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDev_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("确认删除此笔记录？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
