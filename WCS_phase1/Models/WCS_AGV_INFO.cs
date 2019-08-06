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
        public int ID { get; set; }

        /// <summary>
        /// WMS任务UID
        /// </summary>
        public String TASK_UID { get; set; }

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
        public String MAGIC { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATION_TIME { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UPDATE_TIME { get; set; }
    }

    /// <summary>
    ///  AGV 当前状态
    /// </summary>
    public class AGVMagic
    {
        public const int 任务生成 = 1;
        public const int 分配装货卸货点 = 2;
        public const int 前往装货点 = 3;
        public const int 到达装货点 = 4;
        public const int 准备装货 = 5;
        public const int 装货完成 = 6;
        public const int 到达卸货点 = 7;
        public const int 准备卸货 = 8;
        public const int 卸货完成 = 10;
        public const int 任务完成 = 11;
        public const int 重新定位卸货 = 254;
    }
}
