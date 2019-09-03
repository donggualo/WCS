using DataGridManager;
using DataGridManager.DataGrid.Models;
using Panuon.UI.Silver;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_NdcAgv.xaml 的交互逻辑
    /// </summary>
    public partial class W_NdcAgv : UserControl
    {

        #region Property

        NdcAgvDataGrid taskDataGrid;

        #endregion

        public W_NdcAgv()
        {
            InitializeComponent();

            taskDataGrid = new NdcAgvDataGrid();

            //DataContext = taskDataGrid;
            DgCustom.ItemsSource = taskDataGrid.NdcTaskDataList;
            DataControl._mNDCControl.TaskListUpdate += _mNDCControl_TaskListUpdate;
            DataControl._mNDCControl.TaskListDelete += _mNDCControl_TaskListDelete;
            DataControl._mNDCControl.NoticeRedirect += _mNDCControl_NoticeRedirect;
        }

        private void _mNDCControl_NoticeRedirect(NdcTaskModel model)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((System.Action)(() =>
                {
                    Notice.Show("任务ID:"+model.TaskID+"\nOrder:"+model.Order +"\nIkey:"+model.IKey+"\n需要重定向!!", "Notice", 10, MessageBoxIcon.Info);
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void _mNDCControl_TaskListDelete(NdcTaskModel model)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((System.Action)(() =>
                {
                    taskDataGrid.DeleteTask(model);
                    DgCustom.Items.Refresh();

                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void _mNDCControl_TaskListUpdate(NdcTaskModel model)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((System.Action)(() =>
                {
                    taskDataGrid.UpdateTaskInList(model);
                    DgCustom.Items.Refresh();

                }));
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                Notice.Show("任务ID必须是整型数字", "错误", 3, MessageBoxIcon.Error);
                return;
            }

            if (!DataControl._mNDCControl.AddNDCTask(taskid, loadSite.Text, unloadSite.Text, out string result))
            {
                Notice.Show(result, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void LoadAgvBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                var rs = MessageBoxX.Show("任务ID必须是整型数字", "Error", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (!int.TryParse(agvName.Text, out int agvid))
            {
                var rs = MessageBoxX.Show("AGVID必须是整型数字", "Error", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("AGVID必须是整型数字");
                return;
            }
            if (!DataControl._mNDCControl.DoLoad(taskid, agvid, out string result))
            {
                var rs = MessageBoxX.Show(result, "Error", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
            }
        }

        private void UnloadAgvBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                Notice.Show("任务ID必须是数字", "错误", 3, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(agvName.Text, out int agvid))
            {
                Notice.Show("AGVID必须是整型数字", "错误", 3, MessageBoxIcon.Error);
                return;
            }
            if (!DataControl._mNDCControl.DoUnLoad(taskid, agvid, out string result))
            {
                Notice.Show(result, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void RedirectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                Notice.Show("任务ID必须是整型数字", "错误", 3, MessageBoxIcon.Error);
                return;
            }

            int orderint = -1;
            if (order.Text != "" && !int.TryParse(order.Text,out orderint))
            {
                Notice.Show("Order必须是数字", "错误", 3, MessageBoxIcon.Error);
                return;
            }

            if (!DataControl._mNDCControl.DoReDerect(taskid, redirectArea.Text, out string result, orderint))
            {

                Notice.Show(result, "错误", 3, MessageBoxIcon.Error);
            }
        }

        private void NdcConnectCB_Click(object sender, RoutedEventArgs e)
        {
            if (ndcConnectCB.IsChecked != null && (bool)ndcConnectCB.IsChecked)
            {
                DataControl._mNDCControl.DoConnectNDC();
            }
            else
            {
                DataControl._mNDCControl.DoDisConnectNDC();
            }

        }

        private void DgCustom_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row != null)
            {

            }

            //DgCustom.Items.Refresh();
        }

        public void OnWindowClosing()
        {
            DataControl._mNDCControl.TaskListUpdate -= _mNDCControl_TaskListUpdate;
            DataControl._mNDCControl.TaskListDelete -= _mNDCControl_TaskListDelete;
            DataControl._mNDCControl.NoticeRedirect -= _mNDCControl_NoticeRedirect;
        }
    }

}
