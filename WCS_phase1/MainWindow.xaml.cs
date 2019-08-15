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
using WCS_phase1.Action;
using WCS_phase1.Devices;
using WCS_phase1.Http;
namespace WCS_phase1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataControl.Init();
        }

        private void BtnTask_Click(object sender, RoutedEventArgs e)
        {

        }

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
            if(!int.TryParse(ndcTB1.Text,out int taskid))
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
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ndcTB1.Text, out int taskid))
            {
                MessageBox.Show("任务ID必须是整型数字");
                return;
            }
            if (DataControl._mNDCControl.DoReDerect(taskid, ndcTB4.Text,out string result))
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
            if(!DataControl._mNDCControl.DoLoad(taskid,agvid,out string result))
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
