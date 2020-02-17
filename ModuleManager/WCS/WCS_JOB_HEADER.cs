using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    /// <summary>
    /// WCS作业单 表头
    /// </summary>
    public class WCS_JOB_HEADER
    {
        /// <summary>
        /// WCS作业单号
        /// </summary>
        public string JOB_ID { get; set; }

        /// <summary>
        /// WCS作业区域
        /// </summary>
        public string AREA { get; set; }

        /// <summary>
        /// WCS作业固定辊台
        /// </summary>
        public string FRT { get; set; }

        /// <summary>
        /// WMS任务号1
        /// </summary>
        public string TASK_ID1 { get; set; }

        /// <summary>
        /// WMS任务号2
        /// </summary>
        public string TASK_ID2 { get; set; }

        /// <summary>
        /// 作业类型
        /// </summary>
        public int JOB_TYPE { get; set; }

        /// <summary>
        /// 作业状态
        /// </summary>
        public int JOB_STATUS { get; set; }

        /// <summary>
        /// 设备参考信息
        /// </summary>
        public int DEV_FLAG { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATION_TIME { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UPDATE_TIME { get; set; }
    }
}
