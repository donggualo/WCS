using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using TaskManager;
using TaskManager.Models;
using WindowManager.Datagrid;
using WindowManager.Datagrid.Models;

namespace WindowManager
{
    /// <summary>
    /// W_FRT.xaml 的交互逻辑
    /// </summary>
    public partial class W_FRT : UserControl
    {
        private FrtDataGrid grid;
        

        public W_FRT()
        {
            InitializeComponent();
            grid = new FrtDataGrid();

            DataContext = grid;

            getABCNameList();

            new Thread(DoRefresh)
            {
                IsBackground = true
            }.Start();
        }

        private void getABCNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(TaskManager.Models.DeviceType.固定辊台);
            foreach(var l in list)
            {
                grid.UpdateDeviceList(l.DEVICE,l.AREA);
            }
        }

        private void DoRefresh()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    Application.Current.Dispatcher.Invoke((System.Action)(() =>
                    {
                        grid.UpdateDeviceList();
                    }));
                }
                
            }catch(Exception e)
            {
                Console.WriteLine("更新终止："+e.Message);
            }
        }

        private void LocateBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnloadBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Relocate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TerminateBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
