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
        /// 入库作业执行开关
        /// </summary>
        public static bool IsDoJobIn = false;

        /// <summary>
        /// 出库作业执行开关
        /// </summary>
        public static bool IsDoJobOut = false;

        /// <summary>
        /// 设备任务执行开关
        /// </summary>
        public static bool IsDoTask = false;

        /// <summary>
        /// Agv搬运执行开关
        /// </summary>
        public static bool IsDoJobAGV = false;

        /// <summary>
        /// Agv初始卸货区域
        /// </summary>
        public static string AgvUnLoadArea = "FRTinit";



        /// <summary>
        /// 设备修改重启开关
        /// </summary>
        public static bool IsRe = false;


        #endregion
    }
}
