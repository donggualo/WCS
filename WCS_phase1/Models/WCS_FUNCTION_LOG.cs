using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_phase1.Models
{
    /// <summary>
    /// 任务流程方法日志    WCS_FUNCTION_LOG
    /// </summary>
    class WCS_FUNCTION_LOG
    {
        /// <summary>
        /// 方法名
        /// </summary>
        public String FUNCTION_NAME { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public String REMARK { get; set; }

        /// <summary>
        /// WCS单号
        /// </summary>
        public String WCS_NO { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public String ITEM_ID { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public String RESULT { get; set; }

        /// <summary>
        /// 讯息
        /// </summary>
        public String MESSAGE { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATION_TIME { get; set; }
    }
}
