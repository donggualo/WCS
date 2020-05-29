using ModuleManager;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using ADS = WcsManager.Administartor;

namespace WindowManager
{
    /// <summary>
    /// W_SettingLocation.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingLocation : UserControl, ITabWin
    {
        public W_SettingLocation()
        {
            InitializeComponent();
        }

        // 设置时间格式
        private void DataGrid_TimeFormat(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
            {
                (e.Column as DataGridTextColumn).IsReadOnly = true;
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }

        /// <summary>
        /// 获取资讯
        /// </summary>
        private void GetInfo()
        {
            try
            {
                // 清空数据
                DGloc.ItemsSource = null;

                string sql = @"select WMS_LOC 'WMS位置', FRT_LOC '固定辊台号', ARF_LOC '摆渡车定位', RGV_LOC_1 '运输车定位(1#辊台)', RGV_LOC_2 '运输车定位(2#辊台)', 
                                AWC_LOC_TRACK '行车轨道定位', AWC_LOC_STOCK '行车库存定位', DATE_FORMAT(CREATION_TIME,'%Y/%m/%d %T') '时间' from wcs_config_loc order by WMS_LOC";
                // 获取数据
                DGloc.ItemsSource = CommonSQL.mysql.SelectAll(sql).DefaultView;
            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            GetTempData();
        }

        DataTable _Data;
        private void BtnExport_Click(object sender, EventArgs e)
        {
            //打开文件对话框
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog
            {
                //过滤exl文件
                Filter = @"Excel文件 (*.xls; *.xlsx)|*.xls; *.xlsx|All Files (*.*)|*.*"
            };

            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = fd.FileName;//文件名
                try
                {
                    // Excel --> DataTable
                    _Data = new DataTable();
                    _Data = CommonSQL.GetExcelData(fileName);
                    if (CommonSQL.IsNoData(_Data))
                    {
                        Notice.Show("空数据！", "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
                    }

                    // 清空数据
                    DGloc.ItemsSource = null;
                    // 获取数据
                    DGloc.ItemsSource = _Data.DefaultView;

                    // 保存按钮
                    BtnInsertDB.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Notice.Show(ex.Message, "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
                }
            }
        }

        private void BtnInsertDB_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBoxX.Show("确认清空后重新载入数据？！", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // 清空数据
                CommonSQL.mysql.ExcuteSql("truncate table wcs_config_loc");

                // DataTable --> DB
                string mes = CommonSQL.mysql.InsertDB("wcs_config_loc", _Data);

                // 保存按钮
                BtnInsertDB.IsEnabled = false;

                // 刷新
                GetInfo();

                Notice.Show(mes, "提示", 3, Panuon.UI.Silver.MessageBoxIcon.Info);
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
            }
        }

        public void Close()
        {

        }

        private void GetTempData()
        {
            try
            {
                // 清空数据
                DGloc.ItemsSource = null;

                string sql = @"select AV '数据结构版本号', BN '总记录数', BC '总批号', BI '批号',
       D1 '货位编码', D2 '行车轨道定位坐标', D3 '行车货位定位坐标', DATE_FORMAT(CREATION_TIME,'%Y/%m/%d %T') '创建时间' from wcs_loc_temp";
                // 获取数据
                DGloc.ItemsSource = CommonSQL.mysql.SelectAll(sql).DefaultView;
            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
            }
        }

        private void BtnGetData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ADS.UpdateLocation(out string res))
                {
                    // 保存按钮
                    //BtnUpLoc.IsEnabled = true;
                    GetTempData();
                    Notice.Show("获取数据完成！", "成功", 3, MessageBoxIcon.Success);
                }
                else
                {
                    Notice.Show(res, "错误", 3, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Notice.Show(ex.Message, "错误", 3, Panuon.UI.Silver.MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
