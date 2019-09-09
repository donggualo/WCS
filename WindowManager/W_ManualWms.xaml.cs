using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManager;
using TaskManager.Functions;
using ModuleManager.WCS;
using WcsHttpManager;

namespace WindowManager
{
    /// <summary>
    /// W_ManualWms.xaml 的交互逻辑
    /// </summary>
    public partial class W_ManualWms : UserControl
    {
        public W_ManualWms()
        {
            InitializeComponent();

            // 选项框
            AddCombBoxForWMS("A01", CBfrt_P);
            AddCombBoxForWMS("B01", CBfrt_D);
        }

        // 限制仅输入数字
        private void InputNum(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }

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
                if (new ForWMSControl().WriteTaskToWCS(wms, out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败:" + result);
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
                string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(2, '0') + "-" + TBlocZ.Text.Trim().PadLeft(2, '0');
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
            string LOC = "C" + TBlocX.Text.Trim().PadLeft(3, '0') + "-" + TBlocY.Text.Trim().PadLeft(2, '0') + "-" + TBlocZ.Text.Trim().PadLeft(2, '0');

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
                if (new ForWMSControl().WriteTaskToWCS(wms, out string result))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！" + result);
                }
            }
            else
            {
                MessageBox.Show("无法请求WMS出库！");
            }
        }

    }
}
