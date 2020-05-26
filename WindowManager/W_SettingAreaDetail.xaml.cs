using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Windows;

namespace WindowManager
{
    /// <summary>
    /// W_SettingAreaDetail.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingAreaDetail : Window
    {
        private bool isAdd;

        private string _area;

        /// <summary>
        /// 新增
        /// </summary>
        public W_SettingAreaDetail()
        {
            InitializeComponent();
            isAdd = true;
        }

        /// <summary>
        /// 修改
        /// </summary>
        public W_SettingAreaDetail(string AREA, string REMARK, string AWC_DIS_SAFE, string AWC_DIS_TAKE, string AWC_DIS_GIVE, string RGV_DIS_SAFE, 
            string RGV_DIS_BUTT, string RGV_P_CENTER, string RGV_P_ARF, string ARF_DIS_SAFE, string ARF_P_RGV, string ARF_P_STAND1, string ARF_P_STAND2)
        {
            InitializeComponent();
            isAdd = false;
            _area = AREA;
            TBarea.Text = AREA;
            TBmark.Text = REMARK;
            TBawcDS.Text = AWC_DIS_SAFE;
            TBawcDT.Text = AWC_DIS_TAKE;
            TBawcDG.Text = AWC_DIS_GIVE;
            TBrgvDS.Text = RGV_DIS_SAFE;
            TBrgvDB.Text = RGV_DIS_BUTT;
            TBrgvPC.Text = RGV_P_CENTER;
            TBrgvPA.Text = RGV_P_ARF;
            TBarfDS.Text = ARF_DIS_SAFE;
            TBarfPR.Text = ARF_P_RGV;
            TBarfPS1.Text = ARF_P_STAND1;
            TBarfPS2.Text = ARF_P_STAND2;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            if (!Check(out string mes))
            {
                Notice.Show(mes, "错误", 3, MessageBoxIcon.Error);
                return;
            }

            MessageBoxResult result = MessageBoxX.Show("是否确认操作？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                if (isAdd)
                {
                    String sqlinsert = String.Format(@"INSERT INTO wcs_config_area(AREA, REMARK, AWC_DIS_SAFE, AWC_DIS_TAKE, AWC_DIS_GIVE, 
                        RGV_DIS_SAFE, RGV_DIS_BUTT, RGV_P_CENTER, RGV_P_ARF, ARF_DIS_SAFE, ARF_P_RGV, ARF_P_STAND1, ARF_P_STAND2)
                        VALUES ('{0}', '{1}', {2}, {3}, {4}, 
                                {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                       TBarea.Text, TBmark.Text, TBawcDS.Text, TBawcDT.Text, TBawcDG.Text, 
                       TBrgvDS.Text, TBrgvDB.Text, TBrgvPC.Text, TBrgvPA.Text, TBarfDS.Text, TBarfPR.Text, TBarfPS1.Text, TBarfPS2.Text);
                    CommonSQL.mysql.ExcuteSql(sqlinsert);
                }
                else
                {
                    String sqlupdate = String.Format(@"UPDATE wcs_config_area SET AREA = '{0}', REMARK = '{1}', AWC_DIS_SAFE = {2}, AWC_DIS_TAKE = {3}, 
                        AWC_DIS_GIVE = {4}, RGV_DIS_SAFE = {5}, RGV_DIS_BUTT = {6}, RGV_P_CENTER = {7}, RGV_P_ARF = {8}, ARF_DIS_SAFE = {9}, ARF_P_RGV = {10}, 
                        ARF_P_STAND1 = {11}, ARF_P_STAND2 = {12} WHERE AREA = '{13}'",
                        TBarea.Text, TBmark.Text, TBawcDS.Text, TBawcDT.Text, 
                        TBawcDG.Text, TBrgvDS.Text, TBrgvDB.Text, TBrgvPC.Text, TBrgvPA.Text, TBarfDS.Text, TBarfPR.Text, 
                        TBarfPS1.Text, TBarfPS2.Text, _area);
                    CommonSQL.mysql.ExcuteSql(sqlupdate);
                }

                Notice.Show("成功！", "成功", 3, MessageBoxIcon.Success);
                this.Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY"))
                {
                    Notice.Show("失败： 重复区域！", "错误", 3, MessageBoxIcon.Error);
                }
                else
                {
                    Notice.Show("失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
                }
            }

        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private bool Check(out string mes)
        {
            if (string.IsNullOrWhiteSpace(TBarea.Text) ||
                string.IsNullOrWhiteSpace(TBawcDS.Text) ||
                string.IsNullOrWhiteSpace(TBawcDT.Text) ||
                string.IsNullOrWhiteSpace(TBawcDG.Text) ||
                string.IsNullOrWhiteSpace(TBrgvDS.Text) ||
                string.IsNullOrWhiteSpace(TBrgvDB.Text) ||
                string.IsNullOrWhiteSpace(TBrgvPC.Text) ||
                string.IsNullOrWhiteSpace(TBrgvPA.Text) ||
                string.IsNullOrWhiteSpace(TBarfDS.Text) ||
                string.IsNullOrWhiteSpace(TBarfPR.Text) ||
                string.IsNullOrWhiteSpace(TBarfPS1.Text) ||
                string.IsNullOrWhiteSpace(TBarfPS2.Text))
            {
                mes = "请完整填写数值！";
                return false;
            }
            mes = "";
            return true;
        }

    }
}
