using System.Windows;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_DevIgnoreSetting.xaml 的交互逻辑
    /// </summary>
    public partial class W_DevIgnoreSetting : Window
    {
        public W_DevIgnoreSetting()
        {
            InitializeComponent();
            Refresh();
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void Refresh()
        {
            CheckerAGV.IsChecked = DataControl.IsIgnoreAGV;
            CheckerFRT.IsChecked = DataControl.IsIgnoreFRT;
            CheckerARF.IsChecked = DataControl.IsIgnoreARF;
            CheckerRGV.IsChecked = DataControl.IsIgnoreRGV;
            CheckerABC.IsChecked = DataControl.IsIgnoreABC;
        }

        /// <summary>
        /// 是否无视AGV货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerAGV_Click(object sender, RoutedEventArgs e)
        {
            DataControl.IsIgnoreAGV = (bool)CheckerAGV.IsChecked;
        }

        /// <summary>
        /// 是否无视固定辊台货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerFRT_Click(object sender, RoutedEventArgs e)
        {
            DataControl.IsIgnoreFRT = (bool)CheckerFRT.IsChecked;
        }

        /// <summary>
        /// 是否无视摆渡车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerARF_Click(object sender, RoutedEventArgs e)
        {
            DataControl.IsIgnoreARF = (bool)CheckerARF.IsChecked;
        }

        /// <summary>
        /// 是否无视运输车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerRGV_Click(object sender, RoutedEventArgs e)
        {
            DataControl.IsIgnoreRGV = (bool)CheckerRGV.IsChecked;
        }

        /// <summary>
        /// 是否无视行车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerABC_Click(object sender, RoutedEventArgs e)
        {
            DataControl.IsIgnoreABC = (bool)CheckerABC.IsChecked;
        }
    }
}
