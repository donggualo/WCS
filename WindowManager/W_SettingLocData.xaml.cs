using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using ADS = WcsManager.Administartor;

namespace WindowManager
{
    /// <summary>
    /// W_SettingLocData.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingLocData : Window
    {
        private string wmsloc = "";

        /// <summary>
        /// 1-2 层高度差
        /// </summary>
        private readonly int high = 2540;

        public W_SettingLocData()
        {
            InitializeComponent();
            AddCombBox();
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

        private void AddCombBox()
        {
            try
            {
                // 搜索区域
                CBarea.Items.Add("B01");
                CBarea.SelectedIndex = 0;
                //String sql = "select distinct AREA from wcs_config_area";
                //DataTable dt = CommonSQL.mysql.SelectAll(sql);
                //if (CommonSQL.IsNoData(dt))
                //{
                //    return;
                //}
                //foreach (WCS_CONFIG_AREA area in dt.ToDataList<WCS_CONFIG_AREA>())
                //{
                //    CBarea.Items.Add(area.AREA);
                //}

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 清空
                TBawcX.Text = "";
                TBawcYt.Text = "";
                TBawcZt.Text = "";
                TBawcYs.Text = "";
                TBawcZs.Text = "";
                TBrgv1.Text = "";
                TBrgv2.Text = "";

                string area = CBarea.Text;
                string x = TBx.Text;
                string y = TBy.Text;
                string z = TBz.Text;

                if (string.IsNullOrEmpty(area) ||
                    string.IsNullOrEmpty(x) ||
                    string.IsNullOrEmpty(y) ||
                    string.IsNullOrEmpty(z))
                {
                    Notice.Show("请完整填空！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                wmsloc = string.Format("{0}-{1}-{2}-{3}", area, x, y, z);
                WCS_CONFIG_LOC loc = CommonSQL.GetWcsLoc(wmsloc);
                if (string.IsNullOrEmpty(loc.WMS_LOC))
                {
                    Notice.Show("无对应坐标数据！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string[] t = loc.AWC_LOC_TRACK.Split('-');
                string[] s = loc.AWC_LOC_STOCK.Split('-');
                TBawcX.Text = t[0];
                TBawcYt.Text = t[1];
                TBawcZt.Text = t[2];
                TBawcYs.Text = s[1];
                TBawcZs.Text = s[2];
                TBrgv1.Text = loc.RGV_LOC_1;
                TBrgv2.Text = loc.RGV_LOC_2;
                BtnSave.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                string awcX = TBawcX.Text;
                string awcYt = TBawcYt.Text;
                string awcZt = TBawcZt.Text;
                string awcYs = TBawcYs.Text;
                string awcZs = TBawcZs.Text;
                string rgv1 = TBrgv1.Text;
                string rgv2 = TBrgv2.Text;

                if (string.IsNullOrEmpty(awcX) ||
                    string.IsNullOrEmpty(awcYt) ||
                    string.IsNullOrEmpty(awcZt) ||
                    string.IsNullOrEmpty(awcYs) ||
                    string.IsNullOrEmpty(awcZs) ||
                    string.IsNullOrEmpty(rgv1) ||
                    string.IsNullOrEmpty(rgv2))
                {
                    Notice.Show("请完整填空！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否保存修改数据！！"))
                {
                    return;
                }

                CommonSQL.UpdateWcsLoc(wmsloc, rgv1, rgv2,
                    string.Format("{0}-{1}-{2}", awcX, awcYt, awcZt), string.Format("{0}-{1}-{2}", awcX, awcYs, awcZs));
                ADS.mAwc.ClearLoc(wmsloc);

                if (wmsloc.Last() == '1')
                {
                    wmsloc = wmsloc.Remove(wmsloc.Length - 1, 1) + "2";
                    awcZs = (int.Parse(awcZs) + high).ToString();
                    CommonSQL.UpdateWcsLoc(wmsloc, rgv1, rgv2,
                        string.Format("{0}-{1}-{2}", awcX, awcYt, awcZt), string.Format("{0}-{1}-{2}", awcX, awcYs, awcZs));
                    ADS.mAwc.ClearLoc(wmsloc);
                }

                BtnSave.IsEnabled = false;
                wmsloc = "";
                Notice.Show("保存修改成功！", "成功", 3, MessageBoxIcon.Success);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

    }
}
