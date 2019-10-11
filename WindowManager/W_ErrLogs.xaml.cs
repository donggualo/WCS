using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_ErrLogs.xaml 的交互逻辑
    /// </summary>
    public partial class W_ErrLogs : UserControl
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

                string sql = @"select FUNCTION_NAME '方法名', REMARK '说明', WCS_NO '参数1', ITEM_ID '参数2', MESSAGE '描述', CREATION_TIME '时间' from wcs_function_log order by CREATION_TIME desc";
                // 获取数据
                DGlog.ItemsSource = DataControl._mMySql.SelectAll(sql).DefaultView;
            }
            catch (Exception e)
            {
                Notice.Show(e.ToString(), "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            GetInfo();
        }
    }
}
