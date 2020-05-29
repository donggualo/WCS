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

        /// <summary>
        /// 获取资讯
        /// </summary>
        private void GetInfo()
        {
            try
            {
                // 清空数据
                DGlog.ItemsSource = null;

                string sql = @"select FUNCTION_NAME , DATE_FORMAT(CREATION_TIME,'%Y/%m/%d %T') TIME , REMARK , VALUE1 , VALUE2 , VALUE3 , MESSAGE
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
