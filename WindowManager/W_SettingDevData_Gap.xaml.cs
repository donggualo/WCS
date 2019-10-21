using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
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
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_SettingDevData_Gap.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingDevData_Gap : Window
    {
        public string _DEV;
        public string _TYPE;
        /// <summary>
        /// 设置设备坐标偏差
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="type"></param>
        public W_SettingDevData_Gap(string dev, string type)
        {
            InitializeComponent();

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

            _DEV = dev;
            _TYPE = type;

            GetInfo();
        }

        /// <summary>
        /// 获取资讯
        /// </summary>
        private void GetInfo()
        {
            try
            {
                // 运输车只有X轴值
                if (_TYPE == DeviceType.运输车)
                {
                    GapY.IsEnabled = false;
                    GapZ.IsEnabled = false;
                }

                string sql = string.Format(@"select * from wcs_config_dev_gap where DEVICE = '{0}'", _DEV);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                WCS_CONFIG_DEV_GAP info = dt.ToDataEntity<WCS_CONFIG_DEV_GAP>();
                GapX.Text = info.GAP_X.ToString();
                GapY.Text = info.GAP_Y.ToString();
                GapZ.Text = info.GAP_Z.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 确认更新资讯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string x = string.IsNullOrEmpty(GapX.Text.Trim()) ? "0" : GapX.Text.Trim();
                string y = string.IsNullOrEmpty(GapY.Text.Trim()) ? "0" : GapY.Text.Trim();
                string z = string.IsNullOrEmpty(GapZ.Text.Trim()) ? "0" : GapZ.Text.Trim();

                if (_TYPE == DeviceType.运输车 || _TYPE == DeviceType.行车)
                {
                    string sql = string.Format(@"delete from wcs_config_dev_gap where DEVICE = '{0}';
                        insert into wcs_config_dev_gap(DEVICE, TYPE, GAP_X, GAP_Y, GAP_Z) VALUES('{0}','{1}',{2},{3},{4})",
                        _DEV, _TYPE, x, y, z);
                    DataControl._mMySql.ExcuteSql(sql);

                    Notice.Show("设定成功！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    Notice.Show("仅运输车&行车需要设定偏差值！", "错误", 3, MessageBoxIcon.Error);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
