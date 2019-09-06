using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC.SQL
{
    /// <summary>
    /// WCS与NDC 装卸货位置对应关系
    /// </summary>
    public class WCS_NDC_SITE
    {
        /// <summary>
        /// 类型：
        ///     装货区:loadsite
        ///     卸货区:unloadarea
        ///     卸货位置:unloadsite
        /// </summary>
        public string TYPE { set; get; }

        /// <summary>
        /// WCS 位置:唯一标识
        /// </summary>
        public string WCSSITE { set; get; }


        /// <summary>
        /// NDC 位置
        /// </summary>
        public string NDCSITE { set; get; }
    }
}
