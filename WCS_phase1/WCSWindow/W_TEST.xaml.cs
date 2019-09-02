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
            AddCombBox("A01", CBfrt_P);
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// Add 选项
        /// </summary>
        /// <param name="area"></param>
        /// <param name="box"></param>
        private void AddCombBox(string area, System.Windows.Controls.ComboBox box)
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_device where TYPE = 'FRT' and AREA = '{0}' and (FLAG = 'Y'
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

        private void CheckWMS_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CheckWMS.IsChecked)
            {
                CBfrt_D.IsEnabled = true;
                TBlocX.IsEnabled = true;
                TBlocY.IsEnabled = true;
                TBlocZ.IsEnabled = true;
                AddCombBox("B01", CBfrt_D);
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
        /// 分配卸货点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFRT_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CBfrt_P.Text) || string.IsNullOrEmpty(TBcode.Text))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(CBfrt_D.Text))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where SITE <> '{1}' and BARCODE = '{0}'", TBcode.Text, TaskSite.完成);
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
                    Task_UID = "Test" + System.DateTime.Now.ToString("MMddHHmmss"),
                    Task_type = WmsStatus.StockInTask,
                    Barcode = TBcode.Text,
                    W_S_Loc = CBfrt_P.Text,
                    W_D_Loc = CBfrt_D.Text
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
                if (new ForWMSControl().ScanCodeTask(CBfrt_P.Text, TBcode.Text))
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
            if (string.IsNullOrEmpty(CBfrt_P.Text) || string.IsNullOrEmpty(TBcode.Text))
            {
                MessageBox.Show("包装线辊台设备号 / 货物条码 不能为空！", "Error");
                return;
            }

            if ((bool)CheckWMS.IsChecked)
            {
                if (string.IsNullOrEmpty(CBfrt_D.Text))
                {
                    MessageBox.Show("卸货点不能为空！", "Error");
                    return;
                }
                if (string.IsNullOrEmpty(TBlocX.Text) && string.IsNullOrEmpty(TBlocY.Text) && string.IsNullOrEmpty(TBlocZ.Text))
                {
                    MessageBox.Show("货位不能为空！", "Error");
                    return;
                }
                string locX = TBlocX.Text.PadLeft(3, '0');
                string locY = TBlocY.Text.PadLeft(3, '0');
                string locZ = TBlocZ.Text.PadLeft(3, '0');
                string LOC = "C" + locX + "-" + locX + "-" + locZ;
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", TBcode.Text, TaskType.入库);
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
                    TaskType.入库, CBfrt_D.Text, LOC, taskuid);
                DataControl._mMySql.ExcuteSql(sql);

                // 对应 WCS 清单
                DataControl._mTaskTools.CreateCommandIn(taskuid, CBfrt_D.Text);
                MessageBox.Show("完成！");
            }
            else
            {
                if (new ForWMSControl().ScanCodeTask_Loc(CBfrt_P.Text, TBcode.Text))
                {
                    MessageBox.Show("完成！");
                }
                else
                {
                    MessageBox.Show("失败！");
                }
            }
        }
    }

}
