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
    /// W_TaskData_CMD.xaml 的交互逻辑
    /// </summary>
    public partial class W_TaskData_CMD : Window
    {
        public int _TYPE;
        public string _DEV;
        /// <summary>
        /// 作业清单[0:新增；1:更新]
        /// </summary>
        /// <param name="wcsno"></param>
        /// <param name="type"></param>
        public W_TaskData_CMD(string wcsno, int type)
        {
            InitializeComponent();
            _TYPE = type;

            switch (_TYPE)
            {
                case 0:
                    CreateCMD();
                    break;
                case 1:
                    UpdateCMD(wcsno);
                    break;
                default:
                    Notice.Show("TYPE 不可识别！", "错误", 3, MessageBoxIcon.Error);
                    return;
            }
        }

        #region 下拉列表

        // 固定辊台
        private void AddCombBoxForFrt()
        {
            try
            {
                string sql = string.Format(@"select distinct DEVICE From wcs_config_device where TYPE = '{0}' and FLAG = '{1}' and AREA = '{2}'", DeviceType.固定辊台, DeviceFlag.空闲, "B01");
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 遍历执行入库任务
                foreach (WCS_CONFIG_DEVICE dev in devList)
                {
                    CBfrt.Items.Add(dev.DEVICE);
                }
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        // 清单状态
        private void AddCombBoxForStep()
        {
            try
            {
                CBstep.Items.Add(CommandStep.生成单号 + "—生成单号");
                CBstep.Items.Add(CommandStep.请求执行 + "—请求执行");
                CBstep.Items.Add(CommandStep.执行中 + "—执行中");
                CBstep.Items.Add(CommandStep.结束 + "—结束");
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        // 作业类型
        private void AddCombBoxForType()
        {
            try
            {
                CBtype.Items.Add(TaskType.入库 + "—入库");
                CBtype.Items.Add(TaskType.出库 + "—出库");
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        // 任务状态
        private void AddCombBoxForSite()
        {
            try
            {
                CBsite1.Items.Add(TaskSite.未执行 + "—未执行");
                CBsite1.Items.Add(TaskSite.任务中 + "—任务中");
                CBsite1.Items.Add(TaskSite.完成 + "—完成");
                CBsite1.Items.Add(TaskSite.失效 + "—失效");

                CBsite2.Items.Add(TaskSite.未执行 + "—未执行");
                CBsite2.Items.Add(TaskSite.任务中 + "—任务中");
                CBsite2.Items.Add(TaskSite.完成 + "—完成");
                CBsite2.Items.Add(TaskSite.失效 + "—失效");
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void CreateCMD()
        {
            AddCombBoxForFrt();
            AddCombBoxForType();

            // 锁定
            TBwcsno.IsEnabled = false;

            AddCombBoxForStep();
            CBstep.SelectedIndex = 0;
            CBstep.IsEnabled = false;

            AddCombBoxForSite();
            CBsite1.SelectedIndex = 0;
            CBsite2.SelectedIndex = 0;
            CBsite1.IsEnabled = false;
            CBsite2.IsEnabled = false;
        }

        private void UpdateCMD(string wcsno)
        {

        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
