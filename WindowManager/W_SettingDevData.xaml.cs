using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ModuleManager.WCS;
using PubResourceManager;
using ModuleManager;

using ADS = WcsManager.Administartor;

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

                string sql = @"select DEVICE, AREA, IP, PORT, REMARK,
        (case when TYPE = 'FRT' then '固定辊台'
			  when TYPE = 'ARF' then '摆渡车'
			  when TYPE = 'RGV' then '运输车'
			  when TYPE = 'AWC' then '行车'
              when TYPE = 'PKL' then '包装线辊台'else '' end) DEV_TYPE,
	    (case when FLAG = 1 and TYPE = 'FRT' then '仅负责入库'
			  when FLAG = 2 and TYPE = 'FRT' then '仅负责出库'
			  when FLAG = 1 and TYPE = 'ARF' then '仅负责入库'
			  when FLAG = 2 and TYPE = 'ARF' then '仅负责出库'
			  when FLAG = 1 and TYPE = 'RGV' then '靠近入库口'
			  when FLAG = 2 and TYPE = 'RGV' then '远离入库口'
			  when FLAG = 1 and TYPE = 'AWC' then '靠近入库口'
			  when FLAG = 2 and TYPE = 'AWC' then '远离入库口' else '' end) DEV_DUTY,
	    (case when IS_USEFUL = 0 then '停用'
			  when IS_USEFUL = 1 then '启用' else '' end) DEV_USEFUL,
		(case when IS_LOCK = 0 then '解锁'
			  when IS_LOCK = 1 then '锁定' else '' end) DEV_WORK, LOCK_ID1, LOCK_ID2,
			 GAP_X, GAP_Y, GAP_Z, LIMIT_X, LIMIT_Y 
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
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDev_Click(object sender, RoutedEventArgs e)
        {
            W_SettingDevDetail wd = new W_SettingDevDetail();
            wd.ShowDialog();
            TBmark.Visibility = Visibility.Visible;
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
                string device = (DGdevice.SelectedItem as DataRowView)["DEVICE"].ToString();

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
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string flag = (DGdevice.SelectedItem as DataRowView)["DEV_USEFUL"].ToString();
                if (flag != "启用")
                {
                    Notice.Show("该设备暂无法失效！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["DEVICE"].ToString();
                string devtype = (DGdevice.SelectedItem as DataRowView)["DEV_TYPE"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认失效设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                string res = UpdateUseful(devtype, device, false);
                if (string.IsNullOrEmpty(res))
                {
                    PublicParam.IsRe = true;
                    Notice.Show("失效成功！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    Notice.Show("失效失败： " + res, "错误", 3, MessageBoxIcon.Error);
                }

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
                if (PublicParam.IsDoTask)
                {
                    Notice.Show("请先关闭顶部[设备运作]！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (DGdevice.SelectedItem == null)
                {
                    return;
                }
                string flag = (DGdevice.SelectedItem as DataRowView)["DEV_USEFUL"].ToString();
                if (flag != "停用")
                {
                    return;
                }
                string device = (DGdevice.SelectedItem as DataRowView)["DEVICE"].ToString();
                string devtype = (DGdevice.SelectedItem as DataRowView)["DEV_TYPE"].ToString();

                MessageBoxResult result = MessageBoxX.Show("确认生效设备号【" + device + "】的数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                string res = UpdateUseful(devtype, device, true);
                if (string.IsNullOrEmpty(res))
                {
                    PublicParam.IsRe = true;
                    Notice.Show("生效成功！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    Notice.Show("生效失败： " + res, "错误", 3, MessageBoxIcon.Error);
                }

                Refresh_Click(sender, (EventArgs)e);
            }
            catch (Exception ex)
            {
                Notice.Show("生效失败： " + ex.Message, "错误", 3, MessageBoxIcon.Error);
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
                    return;
                }
                DataRowView dr = DGdevice.SelectedItem as DataRowView;

                if (dr["DEV_WORK"].ToString().Equals("锁定"))
                {
                    Notice.Show("设备锁定中，暂无法修改！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dr["DEV_WORK"].ToString()))
                {
                    Notice.Show("设备异常，无法修改！", "提示", 3, MessageBoxIcon.Info);
                    return;
                }

                W_SettingDevDetail wd = new W_SettingDevDetail(
                    dr["DEVICE"].ToString(),
                    dr["AREA"].ToString(),
                    dr["IP"].ToString(),
                    dr["PORT"].ToString(),
                    dr["DEV_TYPE"].ToString(),
                    dr["REMARK"].ToString(),
                    dr["DEV_DUTY"].ToString(),
                    dr["DEV_USEFUL"].ToString(),
                    dr["DEV_WORK"].ToString(),
                    dr["LOCK_ID"].ToString(),
                    dr["GAP_X"].ToString(),
                    dr["GAP_Y"].ToString(),
                    dr["GAP_Z"].ToString(),
                    dr["LIMIT_X"].ToString(),
                    dr["LIMIT_Y"].ToString());
                wd.ShowDialog();
                TBmark.Visibility = Visibility.Visible;
                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string UpdateUseful(string dtype, string dname, bool useful)
        {
            try
            {
                string res = "";
                switch (dtype)
                {
                    case "包装线辊台":
                        ADS.mPkl.devices.Find(c => c.devName.Equals(dname)).UpdateUseufl(useful);
                        break;
                    case "固定辊台":
                        ADS.mFrt.devices.Find(c => c.devName.Equals(dname)).UpdateUseufl(useful);
                        break;
                    case "摆渡车":
                        ADS.mArf.devices.Find(c => c.devName.Equals(dname)).UpdateUseufl(useful);
                        break;
                    case "运输车":
                        ADS.mRgv.devices.Find(c => c.devName.Equals(dname)).UpdateUseufl(useful);
                        break;
                    case "行车":
                        ADS.mAwc.devices.Find(c => c.devName.Equals(dname)).UpdateUseufl(useful);
                        break;
                    default:
                        res = "无法识别类型！";
                        break;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
