using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowManager
{
    /// <summary>
    /// 窗口常用方法
    /// </summary>
    public class WindowCommon
    {
        /// <summary>
        /// 弹框确认进行操作
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ConfirmAction(string msg)
        {
            var result = MessageBoxX.Show(msg, "警告", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                MessageBoxStyle = MessageBoxStyle.Classic,
                MessageBoxIcon = MessageBoxIcon.Warning
            });
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                return true;
            }
            return false;
        }
    }
}
