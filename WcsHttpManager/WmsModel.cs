using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcsHttpManager
{
    /// <summary>
    /// 接收WMS返回的信息类
    /// </summary>
    [Serializable]
    public class WmsModel
    {       
        
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Task_UID { get; set; }
        
        /// <summary>
        /// 任务类型
        /// </summary>
        public WmsStatus Task_type { get; set; }
        
        /// <summary>
        /// 条形码
        /// </summary>
        public string Barcode { get; set; }
        
        /// <summary>
        /// 来源货位
        /// </summary>
        public string W_S_Loc { get; set; }
        
        /// <summary>
        /// 目标货位
        /// </summary>
        public string W_D_Loc { get; set; }

        public override string ToString()
        {
            return "任务ID : "+this.Task_UID+
                "\n任务类型 : "+WmsStatusZH.Get(Task_type)+
                "\n二维码 : " + Barcode+
                "\n来源货位 : " + W_S_Loc+
                "\n目标货位 : " + W_D_Loc;
        }
    }

    /// <summary>
    /// 接受WMS返回的坐标
    /// </summary>
    [Serializable]
    public class WmsLocation
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
        /// 货位坐标
        /// </summary>
        public List<Loction> D { get; set; }
    }

    /// <summary>
    /// 坐标内容
    /// </summary>
    [Serializable]
    public class Loction
    {
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
    }

}
