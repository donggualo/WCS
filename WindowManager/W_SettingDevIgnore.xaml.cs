using PubResourceManager;
using System.Windows;
using System.Windows.Controls;
using TaskManager;

namespace WindowManager
{
    /// <summary>
    /// W_SettingDevIgnore.xaml 的交互逻辑
    /// </summary>
    public partial class W_SettingDevIgnore : UserControl
    {
        public W_SettingDevIgnore()
        {
            InitializeComponent();
            Refresh();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void Refresh()
        {
            CheckerAGV.IsChecked = PublicParam.IsIgnoreAGV;
            CheckerFRT.IsChecked = PublicParam.IsIgnoreFRT;
            CheckerARF.IsChecked = PublicParam.IsIgnoreARF;
            CheckerRGV.IsChecked = PublicParam.IsIgnoreRGV;
            CheckerABC.IsChecked = PublicParam.IsIgnoreABC;
        }

        /// <summary>
        /// 是否无视AGV货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerAGV_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsIgnoreAGV = (bool)CheckerAGV.IsChecked;
        }

        /// <summary>
        /// 是否无视固定辊台货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerFRT_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsIgnoreFRT = (bool)CheckerFRT.IsChecked;
        }

        /// <summary>
        /// 是否无视摆渡车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerARF_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsIgnoreARF = (bool)CheckerARF.IsChecked;
        }

        /// <summary>
        /// 是否无视运输车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerRGV_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsIgnoreRGV = (bool)CheckerRGV.IsChecked;
        }

        /// <summary>
        /// 是否无视行车货物状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckerABC_Click(object sender, RoutedEventArgs e)
        {
            PublicParam.IsIgnoreABC = (bool)CheckerABC.IsChecked;
        }

    }
}
