using Module;
using ModuleManager;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WindowManager
{
    /// <summary>
    /// W_WmsTaskData.xaml 的交互逻辑
    /// </summary>
    public partial class W_WmsTaskData : UserControl, ITabWin
    {
        public W_WmsTaskData()
        {
            InitializeComponent();
            AddCombBox();
        }

        public void Close()
        {

        }

        /// <summary>
        /// 时间格式
        /// </summary>
        private void DGtask_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }

        private void AddCombBox()
        {
            try
            {
                // 搜索任务类型
                CBtype.Items.Add(" ");
                CBtype.Items.Add(TaskTypeEnum.入库.GetHashCode().ToString() + ":" + TaskTypeEnum.入库);
                CBtype.Items.Add(TaskTypeEnum.出库.GetHashCode().ToString() + ":" + TaskTypeEnum.出库);
                CBtype.SelectedIndex = 0;

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 清空数据
                DGtask.ItemsSource = null;

                string sql = @"select TASK_ID 任务号, 
       CONCAT(TASK_TYPE) 任务类型, 
			 CONCAT(TASK_STATUS) 任务状态, 
			 BARCODE 货物二维码, 
			 FRT 作业辊台, 
			 WMS_LOC_FROM 来源, 
			 WMS_LOC_TO 目的, 
			 CREATION_TIME 创建时间, 
			 UPDATE_TIME 更新时间
  from wcs_wms_task where 1=1";
                if (!string.IsNullOrWhiteSpace(CBtype.Text))
                {
                    sql = sql + string.Format(" and TASK_TYPE = '{0}'", CBtype.Text.Substring(0, 1));
                }

                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                DGtask.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGtask.ItemsSource)
                {
                    dr.Row[1] = (TaskTypeEnum)Convert.ToInt32(dr.Row[1]);
                    dr.Row[2] = (WmsTaskStatus)Convert.ToInt32(dr.Row[2]);
                }

            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

    }
}
