using ModuleManager;
using Panuon.UI.Silver;
using PubResourceManager;
using System;
using System.Windows.Controls;

namespace WindowManager
{
    /// <summary>
    /// W_ErrLogs.xaml 的交互逻辑
    /// </summary>
    public partial class W_ErrLogs : UserControl, ITabWin
    {
        public W_ErrLogs()
        {
            InitializeComponent();
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

        /// <summary>
        /// 获取资讯
        /// </summary>
        private void GetInfo()
        {
            try
            {
                // 清空数据
                DGlog.ItemsSource = null;

                string sql = @"select FUNCTION_NAME '方法名', CREATION_TIME '时间' , REMARK '说明', VALUE1 '参数1', VALUE2 '参数2', VALUE3 '参数3', MESSAGE '描述'
                    from wcs_function_log order by CREATION_TIME desc";
                // 获取数据
                DGlog.ItemsSource = CommonSQL.mysql.SelectAll(sql).DefaultView;
            }
            catch (Exception e)
            {
                Notice.Show(e.Message, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            GetInfo();
        }

        public void Close()
        {
            
        }
    }
}
