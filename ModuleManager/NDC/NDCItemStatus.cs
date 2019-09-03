using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC
{
    public enum NDCItemStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        Init = 0,

        /// <summary>
        /// 走到第四步 就可以重定位
        /// </summary>
        CanRedirect = 1,

        /// <summary>
        /// 已经有需重定位数据
        /// </summary>
        HasDirectInfo = 2,

        /// <summary>
        /// 已经到第六步了  还没定位
        /// </summary>
        NeedRedirect = 3,

        /// <summary>
        /// 已经重新定位
        /// </summary>
        Redirected = 4,
    }
}
