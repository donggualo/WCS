using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WCS_phase1.Action;
using WCS_phase1.Devices;
using WCS_phase1.Functions;
using WCS_phase1.Http;
using WCS_phase1.Models;

namespace WCS_phase1.WCSWindow
{
    /// <summary>
    /// W_TEST.xaml 的交互逻辑
    /// </summary>
    public partial class W_TEST : Window
    {
        public W_TEST()
        {
            InitializeComponent();

            // 选项框
            AddCombBoxForWMS("A01", CBfrt_P);
            AddCombBoxForWMS("B01", CBfrt_D);

            AddCombBoxForDEV();
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
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

        #region WMS

        /// <summary>
        /// Add 选项
        /// </summary>
        /// <param name="area"></param>
        /// <param name="box"></param>
        private void AddCombBoxForWMS(string area, System.Windows.Controls.ComboBox box)
        {
            try
            {
                string sql = string.Format(@"select distinct DEVICE from wcs_config_device where TYPE = 'FRT' and AREA = '{0}' and (FLAG = 'Y'
                                                 or LOCK_WCS_NO not in (select WCS_NO From wcs_command_master where TASK_UID_2 is not null))", area);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 遍历执行入库任务
                foreach (WCS_CONFIG_DEVICE dev in devList)
                {
                    box.Items.Add(dev.DEVICE);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
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

        /// <summary>
        /// 分配卸货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFRT_Click(object sender, RoutedEventArgs e)
        {
            string frtP = CBfrt_P.Text.Trim();
            string code = TBcode.Text.Trim();
            string frtD = CBfrt_D.Text.Trim();

            if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(frtD))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and BARCODE = '{0}'", code, TaskSite.完成);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (!DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("货物条码已存在任务！");
                    return;
                }
                // 无Task资讯则新增
                // 呼叫WMS 请求入库资讯---区域
                WmsModel wms = new WmsModel()
                {
                    Task_UID = "NW" + System.DateTime.Now.ToString("yyMMddHHmmss"),
                    Task_type = WmsStatus.StockInTask,
                    Barcode = code,
                    W_S_Loc = frtP,
                    W_D_Loc = frtD
                };
                // 写入数据库
                if (new ForWMSControl().WriteTaskToWCS(wms,out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！"+result);
                }
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask(frtP, code))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }

        /// <summary>
        /// 分配货位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLOC_Click(object sender, RoutedEventArgs e)
        {
            string frtP = CBfrt_P.Text.Trim();
            string code = TBcode.Text.Trim();
            string frtD = CBfrt_D.Text.Trim();

            if (string.IsNullOrEmpty(frtP) || string.IsNullOrEmpty(code))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(frtD))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
                {
                    MessageBox.Show("货位不能为空！", "Error");
                    return;
                }
                // 货位
                string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(3, '0') + "-" + TBlocZ.Text.Trim().PadLeft(3, '0');
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.入库);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("不存在任务Task资讯！", "Error");
                    return;
                }

                // 获取对应任务ID
                string taskuid = dt.Rows[0]["TASK_UID"].ToString();
                // 更新任务资讯
                sql = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '{2}' where TASK_UID = '{3}'",
                    TaskType.入库, frtD, LOC, taskuid);
                DataControl._mMySql.ExcuteSql(sql);

                // 对应 WCS 清单
                DataControl._mTaskTools.CreateCommandIn(taskuid, frtD);
                MessageBox.Show("完成！");
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask_Loc(frtP, code))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }

        /// <summary>
        /// 货位出库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOUT_Click(object sender, RoutedEventArgs e)
        {
            // 无用资讯
            CBfrt_P.Text = "";
            TBcode.Text = "";

            string frtD = CBfrt_D.Text.Trim();
            if (string.IsNullOrEmpty(frtD))
            {
                MessageBox.Show("卸货点不能为空！", "Error");
                return;
            }
            if (string.IsNullOrEmpty(TBlocX.Text.Trim()) && string.IsNullOrEmpty(TBlocY.Text.Trim()) && string.IsNullOrEmpty(TBlocZ.Text.Trim()))
            {
                MessageBox.Show("货位不能为空！", "Error");
                return;
            }
            // 货位
            string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(3, '0') + "-" + TBlocZ.Text.Trim().PadLeft(3, '0');

            if ((bool)CheckWMS.IsChecked)
            {
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and TASK_TYPE = '{2}' and W_S_LOC = '{0}'", LOC, TaskSite.完成, TaskType.出库);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (!DataControl._mStools.IsNoData(dt))
                {
                    MessageBox.Show("该货位已存在出库任务！");
                    return;
                }
                // 无Task资讯则新增
                // 呼叫WMS 请求入库资讯---区域
                WmsModel wms = new WmsModel()
                {
                    Task_UID = "NW" + System.DateTime.Now.ToString("yyMMddHHmmss"),
                    Task_type = WmsStatus.StockOutTask,
                    Barcode = "",
                    W_S_Loc = LOC,
                    W_D_Loc = frtD
                };
                // 写入数据库
                if (new ForWMSControl().WriteTaskToWCS(wms,out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！"+result);
                }
            }
            else
            {
                MessageBox.Show("无法请求WMS出库！");
            }
        }

        #endregion

        #region 设备控制

        /// <summary>
        /// 更新设备号内容
        /// </summary>
        private void ResetDev()
        {
            CBnum.Items.Clear();
            string type = CBdevtype.SelectedValue.ToString();
            string area = CBdevarea.SelectedValue.ToString();

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
                default:
                    break;
            }

            String sql = String.Format(@"select distinct DEVICE from wcs_config_device where TYPE = '{0}' and AREA = '{1}'", type, area);
            DataTable dt = DataControl._mMySql.SelectAll(sql);
            if (DataControl._mStools.IsNoData(dt))
            {
                return;
            }
            List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
            foreach (WCS_CONFIG_DEVICE dev in devList)
            {
                CBnum.Items.Add(dev.DEVICE);
            }
        }

        private void CBnum_DropDownOpened(object sender, EventArgs e)
        {
            ResetDev();
        }

        /// <summary>
        /// 获取当前设备资讯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetDevData_Click(object sender, EventArgs e)
        {
            try
            {
                string type = CBdevtype.Text;
                string dev = CBnum.Text.Trim();

                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(dev))
                {
                    LAdevice.Content = "— — — —";
                    LAactS.Content = "— — — —";
                    LAdevS.Content = "— — — —";
                    LAcmdS.Content = "— — — —";
                    LAtask.Content = "— — — —";
                    LAfinish.Content = "— — — —";
                    LAlocC.Content = "— — — —";
                    LAlocT.Content = "— — — —";
                    LAgoodsS.Content = "— — — —";
                    LAerr.Content = "— — — —";
                    LArollS.Content = "— — — —";
                    LArollD.Content = "— — — —";
                    return;
                }

                switch (type)
                {
                    case "固定辊台":
                        FRT frt = new FRT(dev);
                        break;
                    case "摆渡车":
                        ARF arf = new ARF(dev);
                        break;
                    case "运输车":
                        RGV rgv = new RGV(dev);
                        break;
                    case "行车":
                        ABC abc = new ABC(dev);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

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
                    CBdevarea.Items.Add(area.AREA);
                }

                // 所属设备类型
                CBtype.Items.Add("固定辊台");
                CBtype.Items.Add("摆渡车");
                CBtype.Items.Add("运输车");
                CBtype.Items.Add("行车");

                CBdevtype.Items.Add("固定辊台");
                CBdevtype.Items.Add("摆渡车");
                CBdevtype.Items.Add("运输车");
                CBdevtype.Items.Add("行车");

                CBdevtype.SelectedIndex = 0;
                CBdevarea.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        #region 设备设定

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
						 when TYPE = 'RGV' then '运输车' else '行车' end) 设备类型, 
			 (case when FLAG = 'L' then '锁定'
					   when FLAG = 'Y' then '空闲' else '未知' end) 状态, LOCK_WCS_NO 锁定清单号, CREATION_TIME 创建时间, UPDATE_TIME 更新时间
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
                    MessageBox.Show("请选中目标！", "提示");
                    return;
                }

                string flag = (DGdevice.SelectedItem as DataRowView)["状态"].ToString();
                if (flag == "锁定")
                {
                    MessageBox.Show("设备已锁定，暂无法修改！", "提示");
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

                if (string.IsNullOrEmpty(deviceNew) && string.IsNullOrEmpty(ipNew) && string.IsNullOrEmpty(portNew) &&
                    string.IsNullOrEmpty(area) && string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("请填入明细！", "提示");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("确认修改设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlupdate = String.Format(@"update wcs_config_device set DEVICE = '{0}',IP = '{1}',PORT = '{2}',TYPE = '{3}', AREA = '{4}',REMARK = '{5}', UPDATE_TIME = NOW() 
                    where DEVICE = '{6}' and IP = '{7}' and PORT = '{8}'", deviceNew, ipNew, portNew, type, area, remark, device, ip, port);
                DataControl._mMySql.ExcuteSql(sqlupdate);

                MessageBox.Show("修改成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("PRIMARY"))
                {
                    MessageBox.Show("添加失败： 重复设备号！", "Error");
                }
                else if (ex.ToString().Contains("IP_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复IP！", "Error");
                }
                else if (ex.ToString().Contains("PORT_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复PORT！", "Error");
                }
                else
                {
                    MessageBox.Show("添加失败： " + ex.ToString(), "Error");
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

                if (string.IsNullOrEmpty(device) && string.IsNullOrEmpty(ip) && string.IsNullOrEmpty(port) &&
                    string.IsNullOrEmpty(area) && string.IsNullOrEmpty(type))
                {
                    MessageBox.Show("请填入明细！", "提示");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("确认添加设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqlinsert = String.Format(@"insert into wcs_config_device(DEVICE,IP,PORT,AREA,REMARK,TYPE,FLAG) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','N')",
                    device, ip, port, area, remark, type);
                DataControl._mMySql.ExcuteSql(sqlinsert);

                MessageBox.Show("添加成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("PRIMARY"))
                {
                    MessageBox.Show("添加失败： 重复设备号！", "Error");
                }
                else if (ex.ToString().Contains("IP_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复IP！", "Error");
                }
                else if (ex.ToString().Contains("PORT_UNIQUE"))
                {
                    MessageBox.Show("添加失败： 重复PORT！", "Error");
                }
                else
                {
                    MessageBox.Show("添加失败： " + ex.ToString(), "Error");
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

                MessageBoxResult result = MessageBox.Show("确认删除设备号【" + device + "】的数据？！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                String sqldelete = String.Format(@"delete from wcs_config_device where DEVICE = '{0}'", device);
                DataControl._mMySql.ExcuteSql(sqldelete);

                MessageBox.Show("删除成功！", "提示");
                RefreshDev_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败： " + ex.ToString(), "Error");
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

        #endregion

    }

}
