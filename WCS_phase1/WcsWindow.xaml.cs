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
using System.Windows.Shapes;
using Panuon.UI.Silver;
using WCS_phase1.Action;
using WCS_phase1.WCSWindow;

namespace WCS_phase1
{
    /// <summary>
    /// WcsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WcsWindow : WindowX
    {
        public WcsWindow()
        {
            InitializeComponent();
            DataControl.Init();
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            TreeViewItem item = sender as TreeViewItem;
            if ("Home".Equals(item.Tag))
            {
                wcsTabControl.SelectedIndex = 0;
                return;
            }
            TabItem tabItem = new TabItem();
            tabItem.Header = item.Header;
            tabItem.Content = new W_NdcAgv();
            wcsTabControl.Items.Add(tabItem);
            wcsTabControl.SelectedIndex = wcsTabControl.Items.Count-1;
        }

        private void WcsTabControl_Removed(object sender, RoutedPropertyChangedEventArgs<TabItem> e)
        {
            wcsTabControl.SelectedIndex = 0;//wcsTabControl.Items.Count - 1;
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataControl.BeforeClose();
            System.Environment.Exit(0);
        }
    }
}
