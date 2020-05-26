using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    /// <summary>
    /// WCS作业单 表体
    /// </summary>
    public class WCS_JOB_DETAIL
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// WCS作业单号
        /// </summary>
        public string JOB_ID { get; set; }

        /// WCS作业区域
        /// </summary>
        public string AREA { get; set; }

        /// <summary>
        /// WMS任务号
        /// </summary>
        public string TASK_ID { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int TASK_TYPE { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string DEV_TYPE { get; set; }

        /// <summary>
        /// 设备参考信息
        /// </summary>
        public int DEV_FLAG { get; set; }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DEVICE { get; set; }

        /// <summary>
        /// 接货数量
        /// </summary>
        /// 
        public int TAKE_NUM { get; set; }
        /// <summary>
        /// 接货点(X)
        /// </summary>
        public int TAKE_SITE_X { get; set; }
        /// <summary>
        /// 
        /// 接货点(Y)
        /// </summary>
        public int TAKE_SITE_Y { get; set; }

        /// <summary>
        /// 接货点(Z)
        /// </summary>
        public int TAKE_SITE_Z { get; set; }

        /// <summary>
        /// 来源对接设备(设备字头)
        /// </summary>
        public string DEV_FROM { get; set; }

        /// <summary>
        /// 目的对接设备(设备字头)
        /// </summary>
        public string DEV_TO { get; set; }

        /// <summary>
        /// 送货数量
        /// </summary>
        public int GIVE_NUM { get; set; }
        /// <summary>
        /// 送货点(X)
        /// </summary>
        public int GIVE_SITE_X { get; set; }

        /// <summary>
        /// 送货点(Y)
        /// </summary>
        public int GIVE_SITE_Y { get; set; }

        /// <summary>
        /// 送货点(Z)
        /// </summary>
        public int GIVE_SITE_Z { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int TASK_STATUS { get; set; }

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