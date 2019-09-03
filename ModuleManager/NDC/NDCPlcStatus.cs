using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC
{
    /// <summary>
    /// PLC状态
    /// </summary>
    public enum NDCPlcStatus
    {
        /// <summary>
        /// 装货未准备好
        /// </summary>
        LoadUnReady = 0,

        /// <summary>
        /// 装货准备好了
        /// </summary>
        LoadReady = 1,

        /// <summary>
        /// 装货中
        /// </summary>
        Loading = 2,

        /// <summary>
        /// 装货完成 
        /// </summary>
        Loaded = 3,

        /// <summary>
        /// 卸货未准备好
        /// </summary>
        UnloadUnReady = 4,

        /// <summary>
        /// 卸货准备好
        /// </summary>
        UnloadReady = 5,

        /// <summary>
        /// 卸货中
        /// </summary>
        Unloading = 6,

        /// <summary>
        /// 卸货完成
        /// </summary>
        Unloaded = 7
    }

}
