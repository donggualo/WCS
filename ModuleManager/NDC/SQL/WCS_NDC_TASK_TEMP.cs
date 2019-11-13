using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC.SQL
{
    public class WCS_NDC_TASK_TEMP
    {
        /// <summary>
        /// 数据库唯一标识
        /// </summary>
        public int ID { set; get; } = 0;

        /// <summary>
        /// NDC  任务标识
        /// </summary>
        public int IKEY { set; get; } = 0;

        /// <summary>
        /// NDC 任务序列号
        /// </summary>
        public int NDCINDEX { set; get; } = 0;


        /// <summary>
        /// 小车ID
        /// </summary>
        public int CARRIERID { set; get; } = 0;
    }
}
