using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager
{
    /// <summary>
    /// 在Tabcontrol里面的TabItem 在关闭之前执行的关闭操作
    /// </summary>
    public interface ITabWin
    {
        /// <summary>
        /// 关闭操作，关闭线程，断开监听等操作
        /// </summary>
        void Close();

        /// <summary>
        /// 更新内容
        /// </summary>
        /// <param name="data"></param>
        //void Update(string data);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="data"></param>
        //void Update(Object data);
    }
}
