using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaskManager;
using ModuleManager.WCS;
using PubResourceManager;
using ModuleManager;

namespace WindowManager
{
    /// <summary>
    /// W_DevSetting.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingDevData : UserControl, ITabWin
    {
        public W_SettingDevData()
        {
            InitializeComponent();
            AddCombBoxForDEV();
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

        private void AddCombBoxForDEV()
        {
            try
            {
                // 搜索设备类型
                CBdev.Items.Add(" ");
                CBdev.Items.Add(DeviceType.固定辊台 + " : 固定辊台");
                CBdev.Items.Add(DeviceType.摆渡车 + " : 摆渡车");
                CBdev.Items.Add(DeviceType.运输车 + " : 运输车");
                CBdev.Items.Add(DeviceType.行车 + " : 行车");
                CBdev.SelectedIndex = 0;

                // 搜索设备区域
                CBarea.Items.Add(" ");
                CBarea.SelectedIndex = 0;
                String sql = "select distinct AREA from wcs_config_device";
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> areaList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                foreach (WCS_CONFIG_DEVICE area in areaList)
                {
                    CBarea.Items.Add(area.AREA);
                }

                // 所属设备类型
                CBtype.Items.Add("固定辊台");
                CBtype.Items.Add("摆渡车");
                CBtype.Items.Add("运输车");
                CBtype.Items.Add("行车");
            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshDev_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空数据
                DGdevice.ItemsSource = null;

                string sql = @"select DEVICE 设备号, IP, PORT, AREA 所属区域, REMARK 备注, 
             (case when TYPE = 'FRT' then '固定辊台'
			       when TYPE = 'ARF' then '摆渡车'
				   when TYPE = 'RGV' then '运输车'
                   when TYPE = 'ABC' then '行车'else '' end) 设备类型, 
			 (case when FLAG = 'L' then '锁定'
				   when FLAG = 'U' then '已占用1个辊台'
				   when FLAG = 'Y' then '空闲'
				   when FLAG = 'N' then '失效' else '' end) 状态, LOCK_WCS_NO 锁定清单号, CREATION_TIME 创建时间, UPDATE_TIME 更新时间
             from wcs_config_device where 1=1";
                if (!string.IsNullOrWhiteSpace(CBdev.Text))
                {
                    sql = sql + string.Format(" and TYPE = '{0}'", CBdev.Text.Substring(0, 3));
                }
                if (!string.IsNullOrWhiteSpace(CBarea.Text))
                {
                    sql = sql + string.Format(" and AREA = '{0}'", CBarea.Text.Substring(0, 3));
                }
                // 获取数据
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                DGdevice.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 重连设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReLinkDev_Click(object sender, EventArgs e)
        {
            try
            {
                DataControl._mTaskTools.LinkDevicesClient();
            }
            catch (Exception ex)
            {
                Notice.Show("重连失败：" + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取所选数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGdevice_DoubleClick(object sender, System.EventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }

                TBdevice.Text = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();
                TBip.Text = (DGdevice.SelectedItem as DataRowView)["IP"].ToString();
                TBport.Text = (DGdevice.SelectedItem as DataRowView)["PORT"].ToString();
                TBarea.Text = (DGdevice.SelectedItem as DataRowView)["所属区域"].ToString();
                TBmark.Text = (DGdevice.SelectedItem as DataRowView)["备注"].ToString();
                CBtype.Text = (DGdevice.SelectedItem as DataRowView)["设备类型"].ToString();
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
        private void UpdateDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    Notice.Show("请选中目标！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                string flag = (DGdevice.SelectedItem as DataRowView)["状态"].ToString();
                if (flag == "锁定" || flag == "已占用1个辊台")
                {
                    Notice.Show("设备使用中，暂无法修改！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();
                string ip = (DGdevice.SelectedItem as DataRowView)["IP"].ToString();
                string port = (DGdevice.SelectedItem as DataRowView)["PORT"].ToString();

                string deviceNew = TBdevice.Text.Trim();
                string ipNew = TBip.Text.Trim();
                string portNew = TBport.Text.Trim();
                string area = TBarea.Text.Trim();
                string remark = TBmark.Text.Trim();
                string type = CBtype.Text;
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

                if (string.IsNullOrEmpty(deviceNew) || string.IsNullOrEmpty(ipNew) || string.IsNullOrEmpty(portNew) ||
                    string.IsNullOrEmpty(area) || string.IsNullOrEmpty(type))
                {
                    Notice.Show("请填入明细！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                MessageBoxResult result = MessageBoxX.Show("确认修改设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlupdate = String.Format(@"update wcs_config_device set DEVICE = '{0}',IP = '{1}',PORT = '{2}',TYPE = '{3}', AREA = '{4}',REMARK = '{5}', UPDATE_TIME = NOW() 
                    where DEVICE = '{6}' and IP = '{7}' and PORT = '{8}'", deviceNew, ipNew, portNew, type, area, remark, device, ip, port);
                DataControl._mMySql.ExcuteSql(sqlupdate);

                Notice.Show("修改成功！", "成功", 3, MessageBoxIcon.Success);
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY"))
                {
                    Notice.Show("修改失败： 重复设备号！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("IP_UNIQUE"))
                {
                    Notice.Show("修改失败： 重复IP！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("PORT_UNIQUE"))
                {
                    Notice.Show("修改失败： 重复PORT！", "错误", 3, MessageBoxIcon.Error);
                }
                else
                {
                    Notice.Show("修改失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string device = TBdevice.Text.Trim();
                string ip = TBip.Text.Trim();
                string port = TBport.Text.Trim();
                string area = TBarea.Text.Trim();
                string remark = TBmark.Text.Trim();
                string type = CBtype.Text;
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

                if (string.IsNullOrEmpty(device) || string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) ||
                    string.IsNullOrEmpty(area) || string.IsNullOrEmpty(type))
                {
                    Notice.Show("请填入明细！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                MessageBoxResult result = MessageBoxX.Show("确认添加设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlinsert = String.Format(@"insert into wcs_config_device(DEVICE,IP,PORT,AREA,REMARK,TYPE,FLAG) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                    device, ip, port, area, remark, type, DeviceFlag.空闲);
                DataControl._mMySql.ExcuteSql(sqlinsert);

                Notice.Show("添加成功！", "成功", 3, MessageBoxIcon.Success);
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY"))
                {
                    Notice.Show("添加失败： 重复设备号！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("IP_UNIQUE"))
                {
                    Notice.Show("添加失败： 重复IP！", "错误", 3, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("PORT_UNIQUE"))
                {
                    Notice.Show("添加失败： 重复PORT！", "错误", 3, MessageBoxIcon.Error);
                }
                else
                {
                    Notice.Show("添加失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认删除设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"delete from wcs_config_device where DEVICE = '{0}'", device);
                DataControl._mMySql.ExcuteSql(sqldelete);

                Notice.Show("删除成功！", "成功", 3, MessageBoxIcon.Success);
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show("删除失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 失效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FailDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string flag = (DGdevice.SelectedItem as DataRowView)["状态"].ToString();
                if (flag != "空闲")
                {
                    Notice.Show("该设备暂无法失效！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认失效设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"update wcs_config_device set FLAG = '{1}' where DEVICE = '{0}'", device, DeviceFlag.失效);
                DataControl._mMySql.ExcuteSql(sqldelete);

                Notice.Show("失效成功！", "成功", 3, MessageBoxIcon.Success);
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show("失效失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 生效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkDev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string flag = (DGdevice.SelectedItem as DataRowView)["状态"].ToString();
                if (flag != "失效")
                {
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认生效设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"update wcs_config_device set FLAG = '{1}' where DEVICE = '{0}'", device, DeviceFlag.空闲);
                DataControl._mMySql.ExcuteSql(sqldelete);

                Notice.Show("生效成功！", "成功", 3, MessageBoxIcon.Success);
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show("生效失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导出Excel文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveToExcel_Click(object sender, EventArgs e)
        {
            DataControl._mStools.SaveToExcel(DGdevice);
        }

        /// <summary>
        /// 设置偏差值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetGap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["设备号"].ToString();
                string type = (DGdevice.SelectedItem as DataRowView)["设备类型"].ToString();

                if (type != "运输车" && type != "行车")
                {
                    Notice.Show("仅运输车&行车需要设定！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                W_SettingDevData_Gap _cmd = new W_SettingDevData_Gap(device, type);
                _cmd.ShowDialog();
            }
            catch (Exception ex)
            {
                Notice.Show("设置失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }
    }
}
