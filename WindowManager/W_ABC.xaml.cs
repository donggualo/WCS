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
using DataGridManager;
using DataGridManager.Models;

namespace WindowManager
{
    /// <summary>
    /// W_ABC.xaml 的交互逻辑
    /// </summary>
    public partial class W_ABC : UserControl
    {
        private AbcDataGrid grid;

        public W_ABC()
        {
            InitializeComponent();
            grid = new AbcDataGrid();

            DataContext = grid;
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            grid.UpdateDeviceList(new ABCDeviceModel());
        }
    }
}
