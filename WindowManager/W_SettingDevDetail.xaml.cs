using ModuleManager.WCS;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace WindowManager
{
    /// <summary>
    /// W_SettingDevDetail.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingDevDetail : Window
    {
        private string _type = " ";
        private string _area = " ";
        private string _flag = " ";
        private string _lock = "0";
        private string _user = "0";

        private string _dev;

        private bool isAdd;

        /// <summary>
        /// 新增
        /// </summary>
        public W_SettingDevDetail()
        {
            InitializeComponent();
            isAdd = true;
            AddCombBoxForDEV();
        }

        /// <summary>
        /// 修改
        /// </summary>
        public W_SettingDevDetail(string DEVICE, string AREA, string IP, string PORT, string TYPE, string REMARK, string FLAG, 
            string IS_USEFUL, string IS_LOCK, string LOCK_ID, string GAP_X, string GAP_Y, string GAP_Z, string LIMIT_X, string LIMIT_Y)
        {
            InitializeComponent();
            isAdd = false;
            _dev = DEVICE;
            _type = TYPE;
            _area = AREA;
            _flag = FLAG;
            _lock = IS_LOCK;
            _user = IS_USEFUL;
            TBdev.Text = DEVICE;
            TBip.Text = IP;
            TBport.Text = PORT;
            TBmark.Text = REMARK;
            TBlockid.Text = LOCK_ID;
            TBgapX.Text = GAP_X;
            TBgapY.Text = GAP_Y;
            TBgapZ.Text = GAP_Z;
            TBlimX.Text = LIMIT_X;
            TBlimY.Text = LIMIT_Y;
            AddCombBoxForDEV();
        }

        /// <summary>
        /// 输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]");
            e.Handled = re.IsMatch(e.Text);
        }

        /// <summary>
        /// 选项
        /// </summary>
        private void AddCombBoxForDEV()
        {
            try
            {
                // 设备类型
                CBtype.Items.Add(_type);
                CBtype.Items.Add(DeviceType.固定辊台 + " : 固定辊台");
                CBtype.Items.Add(DeviceType.摆渡车 + " : 摆渡车");
                CBtype.Items.Add(DeviceType.运输车 + " : 运输车");
                CBtype.Items.Add(DeviceType.行车 + " : 行车");
                CBtype.Items.Add(DeviceType.包装线辊台 + " : 包装线辊台");
                CBtype.SelectedIndex = 0;

                // 设备区域
                CBarea.Items.Add(_area);
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

                // 设备属性
                CBflag.Items.Add(_flag);
                CBflag.Items.Add("1：仅负责入库/靠近入库口");
                CBflag.Items.Add("2：仅负责出库/远离入库口");
                CBflag.SelectedIndex = 0;

                // 工作状态
                CBlock.Items.Add(_lock);
                CBlock.Items.Add("0：空闲");
                CBlock.Items.Add("1：锁定");
                CBlock.SelectedIndex = 0;

                // 使用状态
                CBuser.Items.Add(_user);
                CBuser.Items.Add("0：失效");
                CBuser.Items.Add("1：可用");
                CBuser.SelectedIndex = 0;

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
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
                    String sqlinsert = String.Format(@"INSERT INTO wcs_config_device(DEVICE, AREA, IP, PORT, TYPE, REMARK, FLAG, 
                        IS_USEFUL, IS_LOCK, LOCK_ID, GAP_X, GAP_Y, GAP_Z, LIMIT_X, LIMIT_Y) VALUES ('{0}', '{1}', '{2}', {3}, '{4}', '{5}', {6}, 
                        {7}, {8}, '{9}', {10}, {11}, {12}, {13}, {14})",
                       TBdev.Text, CBarea.Text, TBip.Text, TBport.Text, CBtype.Text, TBmark.Text, CBflag.Text.Substring(0, 1), 
                       CBuser.Text.Substring(0, 1), CBlock.Text.Substring(0, 1), TBlockid.Text, TBgapX.Text, TBgapY.Text, TBgapZ.Text, TBlimX.Text, TBlimY.Text);
                    CommonSQL.mysql.ExcuteSql(sqlinsert);
                }
                else
                {
                    String sqlupdate = String.Format(@"UPDATE wcs_config_device SET DEVICE = '{0}', AREA = '{1}', IP = '{2}', PORT = {3},  TYPE = '{4}', 
                        REMARK = '{5}', FLAG = {6}, IS_USEFUL = {7}, IS_LOCK = {8}, LOCK_ID = '{9}', 
                        GAP_X = {10}, GAP_Y = {11}, GAP_Z = {12}, LIMIT_X = {13}, LIMIT_Y = {14} 
                        WHERE DEVICE = '{15}'",
                        TBdev.Text, CBarea.Text, TBip.Text, TBport.Text, CBtype.Text, TBmark.Text, CBflag.Text.Substring(0, 1),
                        CBuser.Text.Substring(0, 1), CBlock.Text.Substring(0, 1), TBlockid.Text, 
                        TBgapX.Text, TBgapY.Text, TBgapZ.Text, TBlimX.Text, TBlimY.Text, _dev);
                    CommonSQL.mysql.ExcuteSql(sqlupdate);
                }

                Notice.Show("成功！", "成功", 3, MessageBoxIcon.Success);
                this.Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY"))
                {
                    Notice.Show("失败： 重复设备号！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("IP_UNIQUE"))
                {
                    Notice.Show("失败： 重复IP！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("PORT_UNIQUE"))
                {
                    Notice.Show("失败： 重复PORT！", "错误", 3, MessageBoxIcon.Error);
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
            if (string.IsNullOrWhiteSpace(TBdev.Text))
            {
                mes = "请填写设备号！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TBip.Text))
            {
                mes = "请填写IP！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TBport.Text))
            {
                mes = "请填写端口！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(CBtype.Text))
            {
                mes = "请选择设备类型！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(CBarea.Text))
            {
                mes = "请选择所属区域！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(CBflag.Text))
            {
                mes = "请选择特别属性！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TBgapX.Text) ||
                string.IsNullOrWhiteSpace(TBgapY.Text) ||
                string.IsNullOrWhiteSpace(TBgapZ.Text))
            {
                mes = "请补齐各坐标偏差！";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TBlimX.Text) ||
                string.IsNullOrWhiteSpace(TBlimY.Text))
            {
                mes = "请补齐各坐标允许误差范围！";
                return false;
            }

            mes = "";
            return true;
        }
    }
}
