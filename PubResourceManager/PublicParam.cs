using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubResourceManager
{
    /// <summary>
    /// 公共参数
    /// </summary>
    public class PublicParam
    {
        #region 设定

        /// <summary>
        /// 是否运行生成出入库任务逻辑
        /// </summary>
        public static bool IsRunTaskLogic_I = false;
        public static bool IsRunTaskLogic_O = false;

        /// <summary>
        /// 是否运行任务指令发送
        /// </summary>
        public static bool IsRunTaskOrder = false;

        /// <summary>
        /// 是否运行AGV派送
        /// </summary>
        public static bool IsRunSendAGV = false;


        /// <summary>
        /// 是否无视AGV货物状态
        /// </summary>
        public static bool IsIgnoreAGV = false;

        /// <summary>
        /// 是否无视固定辊台货物状态
        /// </summary>
        public static bool IsIgnoreFRT = false;

        /// <summary>
        /// 是否无视摆渡车货物状态
        /// </summary>
        public static bool IsIgnoreARF = false;

        /// <summary>
        /// 是否无视运输车货物状态
        /// </summary>
        public static bool IsIgnoreRGV = false;

        /// <summary>
        /// 是否无视行车货物状态
        /// </summary>
        public static bool IsIgnoreABC = false;

        #endregion
    }
}
