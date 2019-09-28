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
        public int _NUM;
        public string _WCSNO;
        public string _FRT;

        /// <summary>
        /// 作业清单[(type)0:新增；1:更新][(num)1:单托；2:双托]
        /// </summary>
        /// <param name="wcsno"></param>
        /// <param name="type"></param>
        /// <param name="num"></param>
        public W_TaskData_CMD(string wcsno, int type, int num)
        {
            InitializeComponent();

            _WCSNO = wcsno;
            _TYPE = type;
            _NUM = num;

            switch (_TYPE)
            {
                case 0:
                    CreateCMD();
                    break;
                case 1:
                    UpdateCMD();
                    break;
                default:
                    Notice.Show("TYPE 不可识别！", "错误", 3, MessageBoxIcon.Error);
                    this.Close();
                    break;
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

            if (_NUM == 1) // 单托
            {
                TBlocfrom2.IsEnabled = false;
                TBlocto2.IsEnabled = false;
                TBcode2.IsEnabled = false;
                CBsite2.IsEnabled = false;
                CBsite2.Items.Add("");
            }

            AddCombBoxForSite();
            CBsite1.SelectedIndex = 0;
            CBsite2.SelectedIndex = 0;
            CBsite1.IsEnabled = false;
            CBsite2.IsEnabled = false;

            TBtaskid1.IsEnabled = false;
            TBtaskid2.IsEnabled = false;
        }

        private void UpdateCMD()
        {
            String sql = String.Format(@"select * from wcs_command_v where WCS_NO = '{0}'", _WCSNO);
            DataTable dt = DataControl._mMySql.SelectAll(sql);
            if (DataControl._mStools.IsNoData(dt))
            {
                return;
            }
            WCS_COMMAND_V cmd = dt.ToDataEntity<WCS_COMMAND_V>();

            // 锁定
            TBwcsno.Text = cmd.WCS_NO;

            TBtaskid1.Text = cmd.TASK_UID_1;
            TBcode1.Text = cmd.CODE_1;
            TBlocfrom1.Text = cmd.LOC_FROM_1;
            TBlocto1.Text = cmd.LOC_TO_1;
            CBsite1.Items.Add(cmd.SITE_1);

            if (string.IsNullOrEmpty(cmd.TASK_UID_2)) //单托
            {
                TBlocfrom2.IsEnabled = false;
                TBlocto2.IsEnabled = false;
                CBsite2.IsEnabled = false;
                CBsite2.Items.Add("");
            }
            else
            {
                TBtaskid2.Text = cmd.TASK_UID_2;
                TBcode2.Text = cmd.CODE_2;
                TBlocfrom2.Text = cmd.LOC_FROM_2;
                TBlocto2.Text = cmd.LOC_TO_2;
                CBsite2.Items.Add(cmd.SITE_2);
            }

            CBfrt.Items.Add(cmd.FRT);
            CBstep.Items.Add(cmd.STEP);
            CBtype.Items.Add(cmd.TASK_TYPE + (cmd.TASK_TYPE == TaskType.入库 ? "—入库" : "—出库"));

            AddCombBoxForSite();
            AddCombBoxForFrt();
            AddCombBoxForStep();
            AddCombBoxForType();
            CBsite1.SelectedIndex = 0;
            CBsite2.SelectedIndex = 0;
            CBfrt.SelectedIndex = 0;
            CBstep.SelectedIndex = 0;
            CBtype.SelectedIndex = 0;

            TBwcsno.IsEnabled = false;
            TBtaskid1.IsEnabled = false;
            TBcode1.IsEnabled = false;
            TBtaskid2.IsEnabled = false;
            TBcode2.IsEnabled = false;
            CBtype.IsEnabled = false;

            _FRT = cmd.FRT;
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
                MessageBoxResult result = MessageBoxX.Show("确认使用此数据内容？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                string sql;
                switch (_TYPE)
                {
                    case 0:
                        if (string.IsNullOrEmpty(CBfrt.Text.Trim()) || string.IsNullOrEmpty(CBtype.Text))
                        {
                            Notice.Show("固定辊台与作业类型不能为空！", "错误", 3, MessageBoxIcon.Error);
                            return;
                        }
                        // 生成单号
                        TBwcsno.Text = (CBtype.Text.Substring(0, 1) == TaskType.入库 ? "I" : "O") + System.DateTime.Now.ToString("yyMMddHHmmss");

                        // 单托
                        TBtaskid1.Text = "T1" + System.DateTime.Now.ToString("yyMMddHHmmss");
                        if (string.IsNullOrEmpty(TBlocfrom1.Text.Trim()) || string.IsNullOrEmpty(TBlocto1.Text.Trim()) || string.IsNullOrEmpty(TBcode1.Text.Trim()))
                        {
                            Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                            return;
                        }
                        sql = string.Format(@"insert into wcs_task_info(TASK_UID, TASK_TYPE, BARCODE, W_S_LOC, W_D_LOC) values('{0}','{1}','{2}','{3}','{4}');",
                            TBtaskid1.Text, CBtype.Text.Substring(0, 1), TBcode1.Text.Trim(), TBlocfrom1.Text.Trim(), TBlocto1.Text.Trim());

                        // 双托
                        if (_NUM == 2)
                        {
                            TBtaskid2.Text = "T2" + System.DateTime.Now.ToString("yyMMddHHmmss");

                            if (string.IsNullOrEmpty(TBlocfrom2.Text.Trim()) || string.IsNullOrEmpty(TBlocto2.Text.Trim()) || string.IsNullOrEmpty(TBcode2.Text.Trim()))
                            {
                                Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                                return;
                            }

                            sql += string.Format(@"insert into wcs_task_info(TASK_UID, TASK_TYPE, BARCODE, W_S_LOC, W_D_LOC) values('{0}','{1}','{2}','{3}','{4}');",
                                TBtaskid2.Text, CBtype.Text.Substring(0, 1), TBcode2.Text.Trim(), TBlocfrom2.Text.Trim(), TBlocto2.Text.Trim());
                        }

                        sql += string.Format(@"insert into wcs_command_master(WCS_NO,FRT,TASK_UID_1,TASK_UID_2) values('{0}','{1}','{2}','{3}')",
                            TBwcsno.Text, CBfrt.Text.Trim(), TBtaskid1.Text, TBtaskid2.Text);

                        DataControl._mMySql.ExcuteSql(sql);

                        // 锁定设备
                        DataControl._mTaskTools.DeviceLock(TBwcsno.Text, CBfrt.Text.Trim());

                        MessageBox.Show(string.Format(@"生成清单成功：清单号【{0}】；1#任务【{1}】；2#任务【{2}】", TBwcsno.Text, TBtaskid1.Text, TBtaskid2.Text));

                        break;
                    case 1:
                        if (string.IsNullOrEmpty(CBfrt.Text.Trim()) || string.IsNullOrEmpty(TBlocfrom1.Text.Trim()) || string.IsNullOrEmpty(CBsite1.Text.Trim()) ||
                            string.IsNullOrEmpty(TBlocto1.Text.Trim()) || string.IsNullOrEmpty(TBcode1.Text.Trim()) || string.IsNullOrEmpty(CBstep.Text.Trim()))
                        {
                            Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                            return;
                        }

                        sql = string.Format(@"update wcs_task_info set W_S_LOC = '{1}', W_D_LOC = '{2}', SITE = '{3}' where TASK_UID = '{0}';",
                                TBtaskid1.Text, TBlocfrom1.Text.Trim(), TBlocto1.Text.Trim(), CBsite1.Text.Substring(0, 1));

                        if (!string.IsNullOrEmpty(TBtaskid2.Text))
                        {
                            if (string.IsNullOrEmpty(TBlocfrom2.Text.Trim()) || string.IsNullOrEmpty(TBlocto2.Text.Trim()) ||
                                string.IsNullOrEmpty(TBcode2.Text.Trim()) || string.IsNullOrEmpty(CBsite1.Text.Substring(0, 1)))
                            {
                                Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                                return;
                            }
                            sql += string.Format(@"update wcs_task_info set UPDATE_TIME = NOW(), W_S_LOC = '{1}', W_D_LOC = '{2}', SITE = '{3}' where TASK_UID = '{0}';",
                                TBtaskid2.Text, TBlocfrom2.Text.Trim(), TBlocto2.Text.Trim(), CBsite2.Text.Substring(0, 1));
                        }

                        sql += string.Format(@"update wcs_command_master set UPDATE_TIME = NOW(), FRT = '{1}', STEP = '{2}' where WCS_NO = '{0}'",
                            TBwcsno.Text, CBfrt.Text.Trim(), CBstep.Text.Substring(0, 1));

                        DataControl._mMySql.ExcuteSql(sql);

                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(_FRT);
                        // 锁定设备
                        DataControl._mTaskTools.DeviceLock(TBwcsno.Text, CBfrt.Text.Trim());

                        break;
                    default:
                        Notice.Show("TYPE 不可识别！", "错误", 3, MessageBoxIcon.Error);
                        this.Close();
                        break;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Notice.Show(ex.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }
    }
}
