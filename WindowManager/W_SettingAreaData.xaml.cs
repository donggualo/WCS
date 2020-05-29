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
    /// W_SettingAreaData.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingAreaData : UserControl, ITabWin
    {
        public W_SettingAreaData()
        {
            InitializeComponent();
        }

        public void Close()
        {

        }
        /// <summary>
        /// 刷新
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 清空数据
                DGarea.ItemsSource = null;

                string sql = @"select AREA, REMARK, AWC_DIS_SAFE, AWC_DIS_TAKE, AWC_DIS_GIVE,
			 RGV_DIS_SAFE, RGV_DIS_BUTT, RGV_P_CENTER, RGV_P_ARF,
			 ARF_DIS_SAFE, ARF_P_RGV, ARF_P_STAND1, ARF_P_STAND2  from wcs_config_area";
                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                DGarea.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            W_SettingAreaDetail wd = new W_SettingAreaDetail();
            wd.ShowDialog();
            Refresh_Click(sender, e);
        }

        /// <summary>
        /// 修改
        /// </summary>
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (DGarea.SelectedItem == null)
            {
                return;
            }
            DataRowView dr = DGarea.SelectedItem as DataRowView;
            W_SettingAreaDetail wd = new W_SettingAreaDetail(
                dr["AREA"].ToString(),
                dr["REMARK"].ToString(),
                dr["AWC_DIS_SAFE"].ToString(),
                dr["AWC_DIS_TAKE"].ToString(),
                dr["AWC_DIS_GIVE"].ToString(),
                dr["RGV_DIS_SAFE"].ToString(),
                dr["RGV_DIS_BUTT"].ToString(),
                dr["RGV_P_CENTER"].ToString(),
                dr["RGV_P_ARF"].ToString(),
                dr["ARF_DIS_SAFE"].ToString(),
                dr["ARF_P_RGV"].ToString(),
                dr["ARF_P_STAND1"].ToString(),
                dr["ARF_P_STAND2"].ToString());
            wd.ShowDialog();
            Refresh_Click(sender, e);
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGarea.SelectedItem == null)
                {
                    return;
                }
                string area = (DGarea.SelectedItem as DataRowView)["AREA"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认删除区域【" + area + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"delete from wcs_config_area where AREA = '{0}'", area);
                CommonSQL.mysql.ExcuteSql(sqldelete);

                Notice.Show("删除成功！", "成功", 3, MessageBoxIcon.Success);
                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show("删除失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

    }
}
