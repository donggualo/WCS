using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_phase1.Models
{
    /// <summary>
    /// AGV资讯   WCS_AGV_INFO
    /// </summary>
    class WCS_AGV_INFO
    {
        /// <summary>
        /// 唯一识别码
        /// </summary>
        public int IKEY { get; set; }

        /// <summary>
        /// AGV车号
        /// </summary>
        public String AGV { get; set; }

        /// <summary>
        /// 装货点
        /// </summary>
        public String PICKSTATION { get; set; }

        /// <summary>
        /// 卸货点
        /// </summary>
        public String DROPSTATION { get; set; }

        /// <summary>
        /// 是否结束
        /// </summary>
        public String ISOVER { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATION_TIME { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UPDATE_TIME { get; set; }
    }
}
