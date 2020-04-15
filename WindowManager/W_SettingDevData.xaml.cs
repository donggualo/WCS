using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
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

        private void AddCombBoxForDEV()
        {
            try
            {
                // 搜索设备类型
                CBtype.Items.Add(" ");
                CBtype.Items.Add(DeviceType.固定辊台 + " : 固定辊台");
                CBtype.Items.Add(DeviceType.摆渡车 + " : 摆渡车");
                CBtype.Items.Add(DeviceType.运输车 + " : 运输车");
                CBtype.Items.Add(DeviceType.行车 + " : 行车");
                CBtype.Items.Add(DeviceType.包装线辊台 + " : 包装线辊台");
                CBtype.SelectedIndex = 0;

                // 搜索设备区域
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
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空数据
                DGdevice.ItemsSource = null;

                string sql = @"select DEVICE 设备号, AREA 所属区域, IP, PORT 端口, 
       (case when TYPE = 'FRT' then '固定辊台'
			       when TYPE = 'ARF' then '摆渡车'
				     when TYPE = 'RGV' then '运输车'
				     when TYPE = 'AWC' then '行车'
             when TYPE = 'PKL' then '包装线辊台'else '' end) 设备类型, REMARK 备注,
			 (case when FLAG = 1 and TYPE = 'FRT' then '仅负责入库'
						 when FLAG = 2 and TYPE = 'FRT' then '仅负责出库'
						 when FLAG = 1 and TYPE = 'ARF' then '仅负责入库'
						 when FLAG = 2 and TYPE = 'ARF' then '仅负责出库'
						 when FLAG = 1 and TYPE = 'RGV' then '靠近入库口'
						 when FLAG = 2 and TYPE = 'RGV' then '远离入库口'
						 when FLAG = 1 and TYPE = 'AWC' then '靠近入库口'
						 when FLAG = 2 and TYPE = 'AWC' then '远离入库口' else '' end) 特别属性,
			 (case when IS_USEFUL = 0 then '失效'
						 when IS_USEFUL = 1 then '可用' else '' end) 使用状态,
			 (case when IS_LOCK = 0 then '空闲'
						 when IS_LOCK = 1 then '锁定' else '' end) 工作状态, LOCK_ID 锁定单号,
			 GAP_X X轴坐标偏差, GAP_Y Y轴坐标偏差, GAP_Z Z轴坐标偏差,
			 LIMIT_X X轴坐标允许误差范围, LIMIT_Y Y轴坐标允许误差范围
  from wcs_config_device where 1=1";
                if (!string.IsNullOrWhiteSpace(CBtype.Text))
                {
                    sql = sql + string.Format(" and TYPE = '{0}'", CBtype.Text.Substring(0, 3));
                }
                if (!string.IsNullOrWhiteSpace(CBarea.Text))
                {
                    sql = sql + string.Format(" and AREA = '{0}'", CBarea.Text);
                }
                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
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
                //DataControl._mTaskTools.LinkDevicesClient();
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

                if ((DGdevice.SelectedItem as DataRowView)["工作状态"].ToString() == "锁定")
                {
                    Notice.Show("设备锁定中，暂无法修改！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (string.IsNullOrWhiteSpace((DGdevice.SelectedItem as DataRowView)["工作状态"].ToString()))
                {
                    Notice.Show("设备号异常，无法修改！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                W_SettingDevDetail wd = new W_SettingDevDetail(
                    (DGdevice.SelectedItem as DataRowView)["设备号"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["所属区域"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["IP"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["端口"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["设备类型"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["备注"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["特别属性"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["使用状态"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["工作状态"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["锁定单号"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["X轴坐标偏差"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["Y轴坐标偏差"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["Z轴坐标偏差"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["X轴坐标允许误差范围"].ToString(),
                    (DGdevice.SelectedItem as DataRowView)["Y轴坐标允许误差范围"].ToString());
                wd.ShowDialog();
                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDev_Click(object sender, RoutedEventArgs e)
        {
            W_SettingDevDetail wd = new W_SettingDevDetail();
            wd.ShowDialog();
            Refresh_Click(sender, (EventArgs)e);
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
                CommonSQL.mysql.ExcuteSql(sqldelete);

                Notice.Show("删除成功！", "成功", 3, MessageBoxIcon.Success);
                Refresh_Click(sender, (EventArgs)e);
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
                string flag = (DGdevice.SelectedItem as DataRowView)["使用状态"].ToString();
                if (flag != "可用")
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

                String sqldelete = String.Format(@"update wcs_config_device set IS_USEFUL = {1} where DEVICE = '{0}'", device, 0);
                CommonSQL.mysql.ExcuteSql(sqldelete);

                Notice.Show("失效成功！", "成功", 3, MessageBoxIcon.Success);
                Refresh_Click(sender, (EventArgs)e);
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
                string flag = (DGdevice.SelectedItem as DataRowView)["使用状态"].ToString();
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

                String sqldelete = String.Format(@"update wcs_config_device set IS_USEFUL = {1} where DEVICE = '{0}'", device, 1);
                CommonSQL.mysql.ExcuteSql(sqldelete);

                Notice.Show("生效成功！", "成功", 3, MessageBoxIcon.Success);
                Refresh_Click(sender, (EventArgs)e);
            }
            catch (Exception ex)
            {
                Notice.Show("生效失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

    }
}
