using System;

namespace ModuleManager.NDC.SQL
{
    /// <summary>
    /// WCS,NDC的任务信息
    /// </summary>
    public class WCS_NDC_TASK
    {
        /// <summary>
        /// 数据库唯一标识
        /// </summary>
        public int ID { set; get; } = 0;

        /// <summary>
        /// 小车ID
        /// </summary>
        public int CARRIERID { set; get; } = 0;

        /// <summary>
        /// WCS 任务标识
        /// </summary>
        public int TASKID { set; get; } = 0;

        /// <summary>
        /// NDC  任务标识
        /// </summary>
        public int IKEY { set; get; } = 0;

        /// <summary>
        /// NDC 任务序列号
        /// </summary>
        public int NDCINDEX { set; get; } = -1;

        /// <summary>
        /// WCS 装货位置信息
        /// </summary>
        public string LOADSITE { set; get; } = "";

        /// <summary>
        /// WCS  卸货位置信息
        /// </summary>
        public string UNLOADSITE { set; get; } = "";
        /// <summary>
        /// WCS 重定位信息
        /// </summary>
        public string REDIRECTSITE { set; get; } = "";

        /// <summary>
        /// NDC 装货位置信息
        /// </summary>
        public string NDCLOADSITE { set; get; } = "";

        /// <summary>
        /// NDC 卸货位置数据
        /// </summary>
        public string NDCUNLOADSITE { set; get; } = "";

        /// <summary>
        /// NDC 重定位位置数据
        /// </summary>
        public string NDCREDIRECTSITE { set; get; } = "";

        /// <summary>
        /// 是否已经装货
        /// </summary>
        public bool HADLOAD { set; get; } = false;

        /// <summary>
        /// 是否已经卸货
        /// </summary>
        public bool HADUNLOAD { set; get; }= false;

        /// <summary>
        /// 是否挂起/执行
        /// </summary>
        public bool PAUSE { set; get; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATETIME { set; get; }
    }
}
