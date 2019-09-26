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
    /// W_TaskData_Task.xaml 的交互逻辑
    /// </summary>
    public partial class W_TaskData_Task : Window
    {
        public int _TYPE;
        public string _WCSNO;
        public string _ID;
        public string _DEV;
        /// <summary>
        /// 细项任务设定[0:新增；1:更新]
        /// </summary>
        /// <param name="wcsNo"></param>
        /// <param name="type"></param>
        public W_TaskData_Task(string wcsNo, string id, int type)
        {
            InitializeComponent();

            _TYPE = type;
            _WCSNO = wcsNo;
            _ID = id;
            AddCombBox();

            switch (_TYPE)
            {
                case 0:
                    CreateTask();
                    break;
                case 1:
                    UpdateTask();
                    break;
                default:
                    Notice.Show("TYPE 不可识别！", "错误", 3, MessageBoxIcon.Error);
                    return;
            }

        }

        private void AddCombBox()
        {
            try
            {
                // 任务类型
                CBitemid.Items.Add(ItemId.固定辊台正向 + " - " + ItemId.GetItemIdName(ItemId.固定辊台正向));
                CBitemid.Items.Add(ItemId.固定辊台反向 + " - " + ItemId.GetItemIdName(ItemId.固定辊台反向));
                CBitemid.Items.Add(ItemId.摆渡车正向 + " - " + ItemId.GetItemIdName(ItemId.摆渡车正向));
                CBitemid.Items.Add(ItemId.摆渡车反向 + " - " + ItemId.GetItemIdName(ItemId.摆渡车反向));
                CBitemid.Items.Add(ItemId.运输车正向 + " - " + ItemId.GetItemIdName(ItemId.运输车正向));
                CBitemid.Items.Add(ItemId.运输车反向 + " - " + ItemId.GetItemIdName(ItemId.运输车反向));
                CBitemid.Items.Add(ItemId.行车取货 + " - " + ItemId.GetItemIdName(ItemId.行车取货));
                CBitemid.Items.Add(ItemId.行车放货 + " - " + ItemId.GetItemIdName(ItemId.行车放货));
                CBitemid.Items.Add(ItemId.摆渡车复位 + " - " + ItemId.GetItemIdName(ItemId.摆渡车复位));
                CBitemid.Items.Add(ItemId.摆渡车定位固定辊台 + " - " + ItemId.GetItemIdName(ItemId.摆渡车定位固定辊台));
                CBitemid.Items.Add(ItemId.摆渡车定位运输车对接 + " - " + ItemId.GetItemIdName(ItemId.摆渡车定位运输车对接));
                CBitemid.Items.Add(ItemId.运输车定位 + " - " + ItemId.GetItemIdName(ItemId.运输车定位));
                CBitemid.Items.Add(ItemId.运输车复位1 + " - " + ItemId.GetItemIdName(ItemId.运输车复位1));
                CBitemid.Items.Add(ItemId.运输车复位2 + " - " + ItemId.GetItemIdName(ItemId.运输车复位2));
                CBitemid.Items.Add(ItemId.运输车对接定位 + " - " + ItemId.GetItemIdName(ItemId.运输车对接定位));
                CBitemid.Items.Add(ItemId.行车轨道定位 + " - " + ItemId.GetItemIdName(ItemId.行车轨道定位));
                CBitemid.Items.Add(ItemId.行车库存定位 + " - " + ItemId.GetItemIdName(ItemId.行车库存定位));
                CBitemid.Items.Add(ItemId.行车复位 + " - " + ItemId.GetItemIdName(ItemId.行车复位));
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void AddCombBoxForDev(string device, string status)
        {
            try
            {
                CBdevice.Items.Add(device);
                CBstatus.Items.Add(status);

                string devtype = "";
                switch (CBitemid.Text.Substring(0, 3))
                {
                    case ItemId.固定辊台正向:
                    case ItemId.固定辊台反向:
                        devtype = DeviceType.固定辊台;
                        break;
                    case ItemId.摆渡车定位固定辊台:
                    case ItemId.摆渡车定位运输车对接:
                    case ItemId.摆渡车复位:
                    case ItemId.摆渡车正向:
                    case ItemId.摆渡车反向:
                        devtype = DeviceType.摆渡车;
                        break;
                    case ItemId.运输车定位:
                    case ItemId.运输车复位1:
                    case ItemId.运输车复位2:
                    case ItemId.运输车对接定位:
                    case ItemId.运输车正向:
                    case ItemId.运输车反向:
                        devtype = DeviceType.运输车;
                        break;
                    case ItemId.行车轨道定位:
                    case ItemId.行车库存定位:
                    case ItemId.行车复位:
                    case ItemId.行车取货:
                    case ItemId.行车放货:
                        devtype = DeviceType.行车;
                        break;
                }
                // 可用设备
                String sql = String.Format(@"select distinct DEVICE from wcs_config_device where FLAG = '{1}' and TYPE = '{0}'", devtype, DeviceFlag.空闲);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                foreach (WCS_CONFIG_DEVICE dev in devList)
                {
                    CBdevice.Items.Add(dev.DEVICE);
                }
                CBdevice.SelectedIndex = 0;

                // 作业状态
                CBstatus.Items.Add(ItemStatus.不可执行 + " - 不可执行");
                CBstatus.Items.Add(ItemStatus.请求执行 + " - 请求执行");
                CBstatus.Items.Add(ItemStatus.任务中 + " - 任务中");
                CBstatus.Items.Add(ItemStatus.失效 + " - 失效");
                CBstatus.Items.Add(ItemStatus.交接中 + " - 交接中");
                CBstatus.Items.Add(ItemStatus.出现异常 + " - 出现异常");
                CBstatus.Items.Add(ItemStatus.完成任务 + " - 完成任务");
                CBstatus.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void CreateTask()
        {
            // 锁定
            TBwcsno.Text = _WCSNO;
            TBwcsno.IsEnabled = false;
            CBdevice.IsEnabled = false;
            CBstatus.Text = ItemStatus.不可执行 + " - 不可执行";
            CBstatus.IsEnabled = false;
            BTNuse.Content = "新增";
        }

        private void UpdateTask()
        {
            String sql = String.Format(@"select * From wcs_task_item where id = '{0}'", _ID);
            DataTable dt = DataControl._mMySql.SelectAll(sql);
            if (DataControl._mStools.IsNoData(dt))
            {
                return;
            }
            WCS_TASK_ITEM item = dt.ToDataEntity<WCS_TASK_ITEM>();
            // 锁定
            TBwcsno.Text = item.WCS_NO;
            TBlocfrom.Text = item.LOC_FROM;
            TBlocto.Text = item.LOC_TO;
            CBitemid.Text = item.ITEM_ID + " - " + ItemId.GetItemIdName(item.ITEM_ID);
            TBwcsno.IsEnabled = false;
            CBitemid.IsEnabled = false;
            AddCombBoxForDev(item.DEVICE, item.STATUS);
            BTNuse.Content = "更新";
            _DEV = item.DEVICE;
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

                string sql = "";
                switch (_TYPE)
                {
                    case 0:
                        if (string.IsNullOrEmpty(TBlocfrom.Text.Trim()) || string.IsNullOrEmpty(TBlocto.Text.Trim()) || string.IsNullOrEmpty(CBitemid.Text.Trim()))
                        {
                            Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                            return;
                        }
                        sql = string.Format(@"insert into wcs_task_item(WCS_NO,ITEM_ID,LOC_FROM,LOC_TO) VALUES('{0}','{1}','{2}','{3}')",
                            _WCSNO, CBitemid.Text.Trim().Substring(0, 3), TBlocfrom.Text.Trim(), TBlocto.Text.Trim());
                        break;
                    case 1:
                        if (string.IsNullOrEmpty(TBlocfrom.Text.Trim()) || string.IsNullOrEmpty(TBlocto.Text.Trim()) || string.IsNullOrEmpty(CBdevice.Text.Trim()) || string.IsNullOrEmpty(CBstatus.Text.Trim()))
                        {
                            Notice.Show("请将资讯填写完整！", "错误", 3, MessageBoxIcon.Error);
                            return;
                        }
                        sql = string.Format(@"update wcs_task_item set LOC_FROM = '{1}',LOC_TO = '{2}',DEVICE = '{3}',STATUS = '{4}' where ID = '{0}'",
                            _ID, TBlocfrom.Text.Trim(), TBlocto.Text.Trim(), CBdevice.Text.Trim(), CBstatus.Text.Trim().Substring(0, 1));

                        // 解锁设备数据状态
                        DataControl._mTaskTools.DeviceUnLock(_DEV);
                        // 锁定设备
                        DataControl._mTaskTools.DeviceLock(_WCSNO, CBdevice.Text.Trim());
                        break;
                    default:
                        Notice.Show("TYPE 不可识别！", "错误", 3, MessageBoxIcon.Error);
                        return;
                }

                if (!string.IsNullOrWhiteSpace(sql))
                {
                    DataControl._mMySql.ExcuteSql(sql);
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
