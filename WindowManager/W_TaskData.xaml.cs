using ModuleManager;
using ModuleManager.WCS;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_WcsTaskData.xaml 的交互逻辑
    /// </summary>
    public partial class W_TaskData : UserControl, ITabWin
    {
        public W_TaskData()
        {
            InitializeComponent();
            RefreshData();
        }
        /// <summary>
        /// 关闭窗口的时候执行释放的动作
        /// </summary>
        public void Close()
        {

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

        DataTable dt;
        String sql;

        /// <summary>
        /// 刷新清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void RefreshData()
        {
            try
            {
                // 清空数据
                dt = null;
                DGcommand.ItemsSource = null;
                DGitem.ItemsSource = null;

                sql = @"select WCS_NO WCS清单号,FRT 固定辊台,CREATION_TIME 创建时间,
             (case when STEP = '1' then '生成单号' 
                   when STEP = '2' then '请求执行' 
                   when STEP = '3' then '执行中' 
                   when STEP = '4' then '结束' else '' end) 清单状态,
			 (case when TASK_TYPE = '0' then 'AGV运输' 
                   when TASK_TYPE = '1' then '入库' 
                   when TASK_TYPE = '2' then '出库' 
                   when TASK_TYPE = '3' then '移仓' 
                   when TASK_TYPE = '4' then '盘点' else '' end) 任务类型,
			 TASK_UID_1 货物①任务号,LOC_FROM_1 货物①来源,LOC_TO_1 货物①目的,
			 (case when SITE_1 = 'N' then '未执行'
				   when SITE_1 = 'W' then '任务中'
				   when SITE_1 = 'Y' then '完成'
				   when SITE_1 = 'X' then '失效' else '' end) 货物①任务状态,
			 TASK_UID_2 货物②任务号,LOC_FROM_2 货物②来源,LOC_TO_2 货物②目的,
			 (case when SITE_2 = 'N' then '未执行'
				   when SITE_2 = 'W' then '任务中'
				   when SITE_2 = 'Y' then '完成'
				   when SITE_2 = 'X' then '失效' else '' end) 货物②任务状态 from wcs_command_v";
                // 获取数据
                dt = DataControl._mMySql.SelectAll(sql);
                DGcommand.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGcommand_DoubleClick(object sender, System.EventArgs e)
        {
            try
            {
                // 获取清单号
                if (DGcommand.SelectedItem == null)
                {
                    return;
                }
                // 清空数据
                dt = null;
                DGitem.ItemsSource = null;

                string wcs_no = (DGcommand.SelectedItem as DataRowView).Row[0].ToString();

                sql = String.Format(@"select WCS_NO WCS清单号,ITEM_ID 作业类型,DEVICE 绑定设备号,LOC_FROM 来源,LOC_TO 目的,
			 (case when STATUS = 'N' then '不可执行'
			       when STATUS = 'Q' then '请求执行'
				   when STATUS = 'W' then '任务中'
				   when STATUS = 'R' then '交接中'
				   when STATUS = 'E' then '出现异常'
				   when STATUS = 'Y' then '完成任务'
				   when STATUS = 'X' then '失效' else '' end) 作业状态,CREATION_TIME 创建时间
             from wcs_task_item where WCS_NO = '{0}'", wcs_no);
                // 获取数据
                dt = DataControl._mMySql.SelectAll(sql);
                DGitem.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGitem.ItemsSource)
                {
                    dr.Row[1] = ItemId.GetItemIdName(dr.Row[1].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
