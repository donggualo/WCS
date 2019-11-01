using PubResourceManager;
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
using WindowManager;

namespace WCS_phase1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        W_SettingDevIgnore _DIS;
        W_TaskData _TD;
        W_SettingDevData _TEST;

        public MainWindow()
        {
            InitializeComponent();
            DataControl.Init();

            CheckTask.IsChecked = PublicParam.IsRunTaskLogic_I;
            CheckOrder.IsChecked = PublicParam.IsRunTaskOrder;
            CheckAGV.IsChecked = PublicParam.IsRunSendAGV;

            // 初始化功能界面
            _DIS = new W_SettingDevIgnore();
            _TD = new W_TaskData();
            _TEST = new W_SettingDevData();
        }

        #region WCS

        /// <summary>
        /// 是否执行任务生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTask_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunTaskLogic_I = (bool)CheckTask.IsChecked;
        }

        /// <summary>
        /// 是否执行指令发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOrder_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunTaskOrder = (bool)CheckOrder.IsChecked;
        }

        /// <summary>
        /// 是否运行AGV派送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAGV_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsRunSendAGV = (bool)CheckAGV.IsChecked;
        }

        /// <summary>
        /// 打开调试设置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDIS_Click(object sender, RoutedEventArgs e)
        {
            //DataControl._mStools.ShowWindow(_DIS);
        }

        /// <summary>
        /// 打开任务资讯界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTD_Click(object sender, RoutedEventArgs e)
        {
            //DataControl._mStools.ShowWindow(_TD);
        }

        /// <summary>
        /// 打开模拟测试界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTEST_Click(object sender, RoutedEventArgs e)
        {
            //DataControl._mStools.ShowWindow(_TEST);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataControl._mHttp.DoPost("http://10.9.31.101/wms.php");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DataControl._mNDCControl.DoConnectNDC();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ndcTB1.Text, out int taskid))
            {
                MessageBox.Show("任务ID必须是整型数字");
                return;
            }

            if (!DataControl._mNDCControl.AddNDCTask(taskid, ndcTB2.Text, ndcTB3.Text, out string result))
            {
                MessageBox.Show(result);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataControl.BeforeClose();
            System.Environment.Exit(0);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ndcTB1.Text, out int taskid))
            {
                MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (DataControl._mNDCControl.DoReDerect(taskid, ndcTB4.Text, out string result))
            {
                MessageBox.Show(result);
            }
        }
        /// <summary>
        /// 装货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ndcTB1.Text, out int taskid))
            {
                MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (!int.TryParse(agvId.Text, out int agvid))
            {
                MessageBox.Show("AGVID必须是整型数字");
                return;
            }
            if (!DataControl._mNDCControl.DoLoad(taskid, agvid, out string result))
            {
                MessageBox.Show(result);
            }
        }
        /// <summary>
        /// 卸货
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ndcTB1.Text, out int taskid))
            {
                MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (!int.TryParse(agvId.Text, out int agvid))
            {
                MessageBox.Show("AGVID必须是整型数字");
                return;
            }
            if (!DataControl._mNDCControl.DoUnLoad(taskid, agvid, out string result))
            {
                MessageBox.Show(result);
            }
        }
    }
}
