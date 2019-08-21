using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

namespace WCS_phase1.WCSWindow
{
    /// <summary>
    /// W_DeviceData.xaml 的交互逻辑
    /// </summary>
    public partial class W_DeviceData : Window
    {
        String sql;
        DataTable dt;

        public W_DeviceData()
        {
            InitializeComponent();
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
            if(e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy/MM/dd HH:mm:ss";
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空数据
                DGdevice.ItemsSource = null;

                sql = @"select DEVICE 设备号, IP, PORT, AREA 所属区域, REMARK 备注, 
             (case when TYPE = 'FRT' then '固定辊台'
			       when TYPE = 'ARF' then '摆渡车'
						 when TYPE = 'RGV' then '运输车' else '行车' end) 设备类型, 
			 (case when FLAG = 'L' then '锁定'
					   when FLAG = 'Y' then '空闲' else '未知' end) 状态, LOCK_WCS_NO 锁定清单号, CREATION_TIME 创建时间, UPDATE_TIME 更新时间 
             from wcs_config_device";
                // 获取数据
                dt = DataControl._mMySql.SelectAll(sql);
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
        private void Update_Click(object sender, EventArgs e)
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
        private void Add_Click(object sender, EventArgs e)
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
        private void Delete_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("确认删除此笔记录？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
