using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.WCS
{
    /// <summary>
    /// 区域资讯 WCS_CONFIG_AREA
    /// </summary>
    public class WCS_CONFIG_AREA
    {
        /// <summary>
        /// 所属区域
        /// </summary>
        public string AREA { get; set; }

        /// <summary>
        /// 区域描述
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        /// 行车安全间距
        /// </summary>
        public int AWC_DIS_SAFE { get; set; }

        /// <summary>
        /// 行车取货运输车后安全高度
        /// </summary>
        public int AWC_DIS_TAKE { get; set; }

        /// <summary>
        /// 行车放货运输车后安全高度
        /// </summary>
        public int AWC_DIS_GIVE { get; set; }

        /// <summary>
        /// 运输车安全间距
        /// </summary>
        public int RGV_DIS_SAFE { get; set; }

        /// <summary>
        /// 运输车对接间距
        /// </summary>
        public int RGV_DIS_BUTT { get; set; }

        /// <summary>
        /// 运输车轨道中点
        /// </summary>
        public int RGV_P_CENTER { get; set; }

        /// <summary>
        /// 运输车对接摆渡车点位
        /// </summary>
        public int RGV_P_ARF { get; set; }

        /// <summary>
        /// 摆渡车安全间距
        /// </summary>
        public int ARF_DIS_SAFE { get; set; }

        /// <summary>
        /// 摆渡车对接运输车点位
        /// </summary>
        public int ARF_P_RGV { get; set; }

        /// <summary>
        /// 摆渡车待命点1(入库车)
        /// </summary>
        public int ARF_P_STAND1 { get; set; }

        /// <summary>
        /// 摆渡车待命点2(出库车)
        /// </summary>
        public int ARF_P_STAND2 { get; set; }

    }
}
