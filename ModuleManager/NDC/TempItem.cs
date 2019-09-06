using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC
{
    /// <summary>
    /// 暂时保存任务信息
    /// </summary>
    public class TempItem
    {
        public int IKey;
        public string Prio;
        public int TaskID;
        public string LoadSite;
        public string UnloadSite;
        public string RedirectSite;

        public string NdcLoadSite;
        public string NdcUnloadSite;
        public string NdcRedirectSite;

        public DateTime addTime = DateTime.Now;

        /// <summary>
        /// 能否重新添加
        /// </summary>
        /// <returns></returns>
        public bool CanReAdd()
        {
            return addTime.Subtract(DateTime.Now).TotalSeconds > 20;
        }
    }
}
