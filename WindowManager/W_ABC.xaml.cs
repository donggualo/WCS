using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ModuleManager.WCS;
using PubResourceManager;
using TaskManager;
using WindowManager.Datagrid;

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

            getABCNameList();

            new Thread(DoRefresh)
            {
                IsBackground = true
            }.Start();
        }

        private void getABCNameList()
        {
            List<WCS_CONFIG_DEVICE> list = CommonSQL.GetDeviceNameList(DataControl._mMySql, DeviceType.行车);
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
