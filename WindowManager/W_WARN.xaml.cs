using System.Collections.Generic;
using System.Windows;

namespace WindowManager
{
    /// <summary>
    /// W_WARN.xaml 的交互逻辑
    /// </summary>
    public partial class W_WARN : Window
    {
        public W_WARN()
        {
            InitializeComponent();
        }

        // 重写OnClosing（防止窗口关闭无法再开Bug）
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public void UpdateData(List<DevError> e)
        {
            DGwarn.ItemsSource = null;
            DGwarn.ItemsSource = e;
        }
    }
}
