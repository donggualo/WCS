using Module;
using ModuleManager;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using WcsHttpManager;
using ADS = WcsManager.Administartor;

namespace WindowManager
{
    /// <summary>
    /// W_WmsTaskData.xaml 的交互逻辑
    /// </summary>
    public partial class W_WmsTaskData : UserControl, ITabWin
    {
        public W_WmsTaskData()
        {
            InitializeComponent();
            AddCombBox();
        }

        public void Close()
        {

        }

        /// <summary>
        /// 时间格式
        /// </summary>
        private void DGtask_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }

        private void AddCombBox()
        {
            try
            {
                // 搜索任务类型
                CBtype.Items.Add(" ");
                CBtype.Items.Add((int)TaskTypeEnum.入库 + ":" + TaskTypeEnum.入库);
                CBtype.Items.Add((int)TaskTypeEnum.出库 + ":" + TaskTypeEnum.出库);
                CBtype.SelectedIndex = 0;

            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 清空数据
                DGtask.ItemsSource = null;

                string sql = @"select TASK_ID 任务号, 
       CONCAT(TASK_TYPE) 任务类型, 
			 CONCAT(TASK_STATUS) 任务状态, 
			 BARCODE 货物二维码, 
			 FRT 作业辊台, 
			 WMS_LOC_FROM 来源, 
			 WMS_LOC_TO 目的, 
			 DATE_FORMAT(CREATION_TIME,'%Y/%m/%d %T') 创建时间, 
			 DATE_FORMAT(UPDATE_TIME,'%Y/%m/%d %T') 更新时间
  from wcs_wms_task where 1=1";
                if (!string.IsNullOrWhiteSpace(CBtype.Text))
                {
                    sql = sql + string.Format(" and TASK_TYPE = '{0}'", CBtype.Text.Substring(0, 1));
                }

                sql = sql + " order by CREATION_TIME desc, UPDATE_TIME desc";
                // 获取数据
                DataTable dt = CommonSQL.mysql.SelectAll(sql);
                DGtask.ItemsSource = dt.DefaultView;

                // 中文释义
                foreach (DataRowView dr in DGtask.ItemsSource)
                {
                    dr.Row[1] = (TaskTypeEnum)Convert.ToInt32(dr.Row[1]);
                    dr.Row[2] = (WmsTaskStatus)Convert.ToInt32(dr.Row[2]);
                }

            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }


        private void TEST_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 请求WMS入库任务
                //WmsModel wms = ADS.mHttp.DoBarcodeScanTask("A01", ADS.GetGoodsCode(DateTime.Now.ToString("yyMMdd")));
                WmsModel wms = ADS.mHttp.DoBarcodeScanTask("A01", "@20200906001|Afpz|B10*22|Dw|Ea1|F001|G2440*0600");
                if (wms != null && !string.IsNullOrEmpty(wms.Task_UID))
                {
                    CommonSQL.InsertTaskInfo(wms.Task_UID, (int)wms.Task_type, wms.Barcode, wms.W_S_Loc, wms.W_D_Loc, "");
                    CommonSQL.UpdateWms(wms.Task_UID, (int)WmsTaskStatus.待分配);
                }

                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void FIN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGtask.SelectedItem == null)
                {
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否确认完成任务！！"))
                {
                    return;
                }

                string id = (DGtask.SelectedItem as DataRowView)["任务号"].ToString();
                string f = (DGtask.SelectedItem as DataRowView)["来源"].ToString();
                string t = (DGtask.SelectedItem as DataRowView)["目的"].ToString();
                string type = (DGtask.SelectedItem as DataRowView)["任务类型"].ToString();

                // 完成任务
                string mes = null;
                switch (type)
                {
                    case "入库":
                        mes = ADS.mHttp.DoStockInFinishTask(t, id);
                        break;
                    case "出库":
                        mes = ADS.mHttp.DoStockOutFinishTask(f, id);
                        break;
                    default:
                        break;
                }

                if (mes.Contains("OK"))
                {
                    CommonSQL.UpdateWms(id, (int)WmsTaskStatus.完成);
                    Notice.Show("成功！", "完成", 3, MessageBoxIcon.Success);
                }
                else
                {
                    Notice.Show(mes, "失败", 3, MessageBoxIcon.Warning);
                }

                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void GetBin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGtask.SelectedItem == null)
                {
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否请求库位分配！！"))
                {
                    return;
                }

                string id = (DGtask.SelectedItem as DataRowView)["任务号"].ToString();
                string status = (DGtask.SelectedItem as DataRowView)["任务状态"].ToString();
                string type = (DGtask.SelectedItem as DataRowView)["任务类型"].ToString();
                if ( type == "入库")
                {
                    // 请求WMS入库分配
                    if (ADS.AssignInSite("B01", id))
                    {
                        Notice.Show("完成！请刷新~", "分配", 3, MessageBoxIcon.Success);
                    }
                    else
                    {
                        Notice.Show("失败！", "失败", 3, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    Notice.Show("该任务不符合分配条件！", "提示", 3, MessageBoxIcon.Info);
                }

                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DGtask.SelectedItem == null)
                {
                    return;
                }

                if (!WindowCommon.ConfirmAction("是否确认删除任务！！"))
                {
                    return;
                }

                string id = (DGtask.SelectedItem as DataRowView)["任务号"].ToString();
                string type = (DGtask.SelectedItem as DataRowView)["任务类型"].ToString();

                // 取消任务
                string mes = null;
                switch (type)
                {
                    case "入库":
                        Notice.Show("入库任务不允许撤销！", "提示", 3, MessageBoxIcon.Info);
                        return;
                        //mes = ADS.mHttp.DoCancelTask(WmsStatus.StockInTask, id);
                        //break;
                    case "出库":
                        // 行车判断
                        if (ADS.mAwc.IsCanDelOut(id))
                        {
                            mes = ADS.mHttp.DoCancelTask(WmsStatus.StockOutTask, id);
                        }
                        break;
                    default:
                        break;
                }

                CommonSQL.DeleteWms(id);
                Notice.Show("成功！", "删除任务", 3, MessageBoxIcon.Success);

                Refresh_Click(sender, e);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }
    }
}
