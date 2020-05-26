using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    public class WCS_LOC_TEMP
    {
        /// <summary>
        /// 数据结构版本号
        /// </summary>
        public int AV { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int BN { get; set; }

        /// <summary>
        /// 总批号
        /// </summary>
        public int BC { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public int BI { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        public string D1 { get; set; }

        /// <summary>
        /// 行车轨道
        /// </summary>
        public string D2 { get; set; }

        /// <summary>
        /// 行车货位
        /// </summary>
        public string D3 { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATION_TIME { get; set; }

    }
}