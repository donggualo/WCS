using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    /// <summary>
    /// 设备偏差参数    WCS_CONFIG_DEV_GAP
    /// </summary>
    public class WCS_CONFIG_DEV_GAP
    {
        /// <summary>
        /// 设备
        /// </summary>
        public String DEVICE { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public String TYPE { get; set; }

        /// <summary>
        /// X轴差
        /// </summary>
        public int GAP_X { get; set; }

        /// <summary>
        /// Y轴差
        /// </summary>
        public int GAP_Y { get; set; }

        /// <summary>
        /// Z轴差
        /// </summary>
        public int GAP_Z { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATION_TIME { get; set; }

    }
}
