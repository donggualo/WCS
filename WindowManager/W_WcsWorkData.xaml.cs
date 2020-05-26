using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Module;
using ModuleManager.WCS;
using PubResourceManager;
using Panuon.UI.Silver;
using ModuleManager;

namespace WindowManager
{
    /// <summary>
    /// W_WcsWorkData.xaml 的交互逻辑
    /// </summary>
    public partial class W_WcsWorkData : UserControl, ITabWin
    {
        private string jobID;

        public W_WcsWorkData()
        {
            InitializeComponent();
            AddCombBox();
        }

        public void Close()
        {

        }

        private void AddCombBox()
        {
            try
            {
                // 搜索任务类型
                CBtype.Items.Add(" ");
                CBtype.Items.Add(TaskTypeEnum.AGV搬运.GetHashCode().ToString() + ":" + TaskTypeEnum.AGV搬运);
                CBtype.Items.Add(TaskTypeEnum.入库.GetHashCode().ToString() + ":" + TaskTypeEnum.入库);
                CBtype.Items.Add(TaskTypeEnum.出库.GetHashCode().ToString() + ":" + TaskTypeEnum.出库);
                CBtype.SelectedIndex = 0;

                // 搜索任务区域
                CBarea.Items.Add(" ");
                CBarea.SelectedIndex = 0;
                String sql = "select distinct AREA from wcs_config_area";
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                if (CommonSQL.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_AREA> areaList = dt.ToDataList<WCS_CONFIG_AREA>();
                foreach (WCS_CONFIG_AREA area in areaList)
                {
                    CBarea.Items.Add(area.AREA);
                }

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 时间格式
        /// </summary>
        private void DataGrid_TimeFormat(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
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
                DGheader.ItemsSource = null;
                DGdetail.ItemsSource = null;

                string sql = @"select JOB_ID 作业单号, AREA 作业区域, CONCAT(JOB_TYPE) 作业类型, CONCAT(JOB_STATUS) 作业状态, 
			 TASK_ID1 WMS任务①, TASK_ID2 WMS任务②, CREATION_TIME 创建时间, UPDATE_TIME 更新时间
  from wcs_job_header where 1=1";
                if (!string.IsNullOrWhiteSpace(CBtype.Text))
                {
                    sql = sql + string.Format(" and JOB_TYPE = '{0}'", CBtype.Text.Substring(0, 1));
                }
                if (!string.IsNullOrWhiteSpace(CBarea.Text))
                {
                    sql = sql + string.Format(" and AREA = '{0}'", CBarea.Text);
                }

                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                DGheader.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGheader.ItemsSource)
                {
                    dr.Row[2] = (TaskTypeEnum)Convert.ToInt32(dr.Row[2]);

                    if (dr.Row[2].Equals(TaskTypeEnum.AGV搬运.ToString()))
                    {
                        dr.Row[3] = (WcsAgvStatus)Convert.ToInt32(dr.Row[3]);
                    }
                    if (dr.Row[2].Equals(TaskTypeEnum.入库.ToString()))
                    {
                        dr.Row[3] = (WcsInStatus)Convert.ToInt32(dr.Row[3]);
                    }
                    if (dr.Row[2].Equals(TaskTypeEnum.出库.ToString()))
                    {
                        dr.Row[3] = (WcsOutStatus)Convert.ToInt32(dr.Row[3]);
                    }
                }

            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取表头
        /// </summary>
        private void DGheader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取清单号
            if (DGheader.SelectedItem == null)
            {
                return;
            }

            jobID = (DGheader.SelectedItem as DataRowView).Row[0].ToString();

            GetJobDetail();
        }

        /// <summary>
        /// 获取表体
        /// </summary>
        private void GetJobDetail()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobID))
                {
                    return;
                }

                // 清空数据
                DGdetail.ItemsSource = null;

                string sql = @"select ID,
			 JOB_ID 作业单号,
			 AREA 作业区域,
			 TASK_ID	WMS任务号,
			 CONCAT(TASK_STATUS)	任务状态,
			 DEV_TYPE	设备类型,
			 DEVICE	设备名,
			 TAKE_NUM	接货数量,
			 CONCAT(TAKE_SITE_X,'-',TAKE_SITE_Y,'-',TAKE_SITE_Z) 接货点,
			 DEV_FROM	来源对接设备,
			 DEV_TO	目的对接设备,
			 GIVE_NUM	送货数量,
			 CONCAT(GIVE_SITE_X,'-',GIVE_SITE_Y,'-',GIVE_SITE_Z) 送货点,
			 CREATION_TIME	创建时间,
			 UPDATE_TIME	更新时间
  from wcs_job_detail;";

                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                DGdetail.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGdetail.ItemsSource)
                {
                    dr.Row[4] = GetTaskStatus((TaskStatus)Convert.ToInt32(dr.Row[4]));
                    dr.Row[5] = DeviceType.GetDevTypeName(dr.Row[5].ToString());
                    dr.Row[9] = DeviceType.GetDevTypeName(dr.Row[9].ToString());
                    dr.Row[10] = DeviceType.GetDevTypeName(dr.Row[10].ToString());
                }

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private string GetTaskStatus(TaskStatus ts)
        {
            string res;
            switch (ts)
            {
                case TaskStatus.init:
                    res = "初始化";
                    break;
                case TaskStatus.totakesite:
                    res = "前往接货点";
                    break;
                case TaskStatus.ontakesite:
                    res = "抵达接货点";
                    break;
                case TaskStatus.taking:
                    res = "接货中";
                    break;
                case TaskStatus.taked:
                    res = "接货完成";
                    break;
                case TaskStatus.togivesite:
                    res = "前往送货点";
                    break;
                case TaskStatus.ongivesite:
                    res = "抵达送货点";
                    break;
                case TaskStatus.giving:
                    res = "送货中";
                    break;
                case TaskStatus.gived:
                    res = "送货完成";
                    break;
                case TaskStatus.finish:
                    res = "完成任务";
                    break;
                default:
                    res = "";
                    break;
            }
            return res;
        }
    }
}
