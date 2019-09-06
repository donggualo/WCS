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
    /// W_WcsNdcSite.xaml 的交互逻辑
    /// </summary>
    public partial class W_WcsNdcSite : UserControl
    {
        public W_WcsNdcSite()
        {
            InitializeComponent();
            RefreshSite();
        }


        private void SiteDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddSite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshSite()
        {
            try
            {
                // 清空数据
                SiteDG.ItemsSource = null;

                string sql = @"select (case when TYPE = 'loadsite' then '装货区'"+
                    "when TYPE = 'unloadarea' then '卸货区' else '卸货点' end) 类型,"+
                    "WCSSITE WCS站点,NDCSITE NDC站点 from wcs_ndc_site";
                
                // 获取数据
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                SiteDG.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void EditSite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteSite_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
