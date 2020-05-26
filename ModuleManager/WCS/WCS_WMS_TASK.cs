using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    /// <summary>
    /// WMS任务资讯
    /// </summary>
    public class WCS_WMS_TASK
    {
        /// <summary>
        /// WMS任务号
        /// </summary>
        public string TASK_ID { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int TASK_TYPE { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int TASK_STATUS { get; set; }

        /// <summary>
        /// 货物贴码
        /// </summary>
        public string BARCODE { get; set; }

        /// <summary>
        /// 入库固定辊台
        /// </summary>
        public string FRT { get; set; }

        /// <summary>
        /// WMS来源货位
        /// </summary>
        public string WMS_LOC_FROM { get; set; }

        /// <summary>
        /// WMS目的货位
        /// </summary>
        public string WMS_LOC_TO { get; set; }

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
