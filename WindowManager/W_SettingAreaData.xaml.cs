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

                string sql = @"select AREA 区域,
			 REMARK 区域描述,
			 AWC_DIS_SAFE 行车安全间距,
			 AWC_DIS_TAKE 行车取货运输车后安全高度,
			 AWC_DIS_GIVE 行车放货运输车后安全高度,
			 RGV_DIS_SAFE 运输车安全间距,
			 RGV_DIS_BUTT 运输车对接间距,
			 RGV_P_CENTER 运输车轨道中点,
			 RGV_P_ARF 运输车对接摆渡车点位,
			 ARF_DIS_SAFE 摆渡车安全间距,
			 ARF_P_RGV 摆渡车对接运输车点位,
			 ARF_P_STAND1 入库摆渡车待命点1,
			 ARF_P_STAND2 出库摆渡车待命点2
  from wcs_config_area";
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
            W_SettingAreaDetail wd = new W_SettingAreaDetail(
                (DGarea.SelectedItem as DataRowView)["区域"].ToString(),
                (DGarea.SelectedItem as DataRowView)["区域描述"].ToString(),
                (DGarea.SelectedItem as DataRowView)["行车安全间距"].ToString(),
                (DGarea.SelectedItem as DataRowView)["行车取货运输车后安全高度"].ToString(),
                (DGarea.SelectedItem as DataRowView)["行车放货运输车后安全高度"].ToString(),
                (DGarea.SelectedItem as DataRowView)["运输车安全间距"].ToString(),
                (DGarea.SelectedItem as DataRowView)["运输车对接间距"].ToString(),
                (DGarea.SelectedItem as DataRowView)["运输车轨道中点"].ToString(),
                (DGarea.SelectedItem as DataRowView)["运输车对接摆渡车点位"].ToString(),
                (DGarea.SelectedItem as DataRowView)["摆渡车安全间距"].ToString(),
                (DGarea.SelectedItem as DataRowView)["摆渡车对接运输车点位"].ToString(),
                (DGarea.SelectedItem as DataRowView)["入库摆渡车待命点1"].ToString(),
                (DGarea.SelectedItem as DataRowView)["出库摆渡车待命点2"].ToString());
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
                string area = (DGarea.SelectedItem as DataRowView)["区域"].ToString();

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
