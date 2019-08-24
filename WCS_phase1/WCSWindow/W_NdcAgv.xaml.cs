using Caliburn.Micro;
using Panuon.UI.Silver;
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
using WCS_phase1.Action;
using WCS_phase1.Hlpers;

namespace WCS_phase1.WCSWindow
{
    /// <summary>
    /// W_NdcAgv.xaml 的交互逻辑
    /// </summary>
    public partial class W_NdcAgv : UserControl
    {

        #region Property
        public IList<DataGridTaskModel> TaskDataList { get; set; }
        private int countItem = 0;
        
        #endregion

        public W_NdcAgv()
        {
            InitializeComponent();

            TaskDataList = new List<DataGridTaskModel>()
            {
                new DataGridTaskModel(){  TaskID=001,AgvName="01",LoadSite="C265",UnLoadSite="B20",RedirectSite="E2",HasLoad=true,HasUnLoad=false },
                new DataGridTaskModel(){  TaskID=002,AgvName="02",LoadSite="C225",UnLoadSite="B60",RedirectSite="E2",HasLoad=true },
                new DataGridTaskModel(){  TaskID=003,AgvName="03",LoadSite="C2s5",UnLoadSite="B26",HasLoad=true,HasUnLoad=false },
                new DataGridTaskModel(){  TaskID=004,AgvName="04",LoadSite="C2d5",UnLoadSite="B30",RedirectSite="E2",},
                new DataGridTaskModel(){  TaskID=005,AgvName="05",LoadSite="Ce65",UnLoadSite="B10",RedirectSite="E2",HasLoad=true,HasUnLoad=true },
            };
            DgCustom.ItemsSource = TaskDataList;

            new Thread(RefreshTaskList)
            {
                IsBackground = true
            }.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //TestDataList.Add(new DataGridTestModel() { Name = "Item:"+ countItem, IsEnabled = true, Score = countItem++, });
           // DgCustom.Items.Refresh();
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                var rs = MessageBoxX.Show("任务ID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("任务ID必须是整型数字");
                return;
            }

            if (!DataControl._mNDCControl.AddNDCTask(taskid, loadSite.Text, unloadSite.Text, out string result))
            {
                var rs = MessageBoxX.Show(result, "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
            }
        }

        private void LoadAgvBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                var rs = MessageBoxX.Show("任务ID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (!int.TryParse(agvName.Text, out int agvid))
            {
                var rs = MessageBoxX.Show("AGVID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("AGVID必须是整型数字");
                return;
            }
            if (!DataControl._mNDCControl.DoLoad(taskid, agvid, out string result))
            {
                var rs = MessageBoxX.Show(result, "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
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
                var rs = MessageBoxX.Show("任务ID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (!int.TryParse(agvName.Text, out int agvid))
            {
                var rs = MessageBoxX.Show("AGVID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("AGVID必须是整型数字");
                return;
            }
            if (!DataControl._mNDCControl.DoUnLoad(taskid, agvid, out string result))
            {
                var rs = MessageBoxX.Show(result, "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
            }
        }

        private void RedirectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(taskId.Text, out int taskid))
            {
                var rs = MessageBoxX.Show("任务ID必须是整型数字", "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
                //MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (DataControl._mNDCControl.DoReDerect(taskid, redirectArea.Text, out string result))
            {
                var rs = MessageBoxX.Show(result, "Error", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
                {
                    MessageBoxIcon = MessageBoxIcon.Error,
                    ThemeBrush = "#FF4C4C".ToColor().ToBrush(),
                });
            }
        }

        private void RefreshTaskList()
        {
            while (true)
            {
                Thread.Sleep(1000);
                lock (TaskDataList)
                {
                    TaskDataList.Clear();
                    TaskDataList.Concat(DataControl._mNDCControl.GetTaskDataList());
                    DgCustom.Items.Refresh();
                }
            }
        }

        private void NdcConnectCB_Click(object sender, RoutedEventArgs e)
        {
            if (ndcConnectCB.IsChecked!=null && (bool)ndcConnectCB.IsChecked)
            {
                DataControl._mNDCControl.DoConnectNDC();
            }
            else
            {
                DataControl._mNDCControl.DoDisConnectNDC();
            }

        }
    }


    public class DataGridTaskModel
    {
        [DataGridColumn("任务ID")]
        public int TaskID { get; set; }

        [DataGridColumn("IKey")]
        public int IKey { get; set; }

        [DataGridColumn("Order")]
        public int Order { get; set; }

        [DataGridColumn("AGV名称")]
        public string AgvName { get; set; }

        [DataGridColumn("接货点")]
        public string LoadSite { get; set; }

        [DataGridColumn("卸货点")]
        public string UnLoadSite { get; set; }

        [DataGridColumn("重定向")]
        public string RedirectSite { get; set; }

        [DataGridColumn("接货")]
        public bool HasLoad { get; set; }

        [DataGridColumn("接货")]
        public bool HasUnLoad { get; set; }
    }
}
