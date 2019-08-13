using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_phase1.Models
{
    /// <summary>
    /// WCS指令资讯     WCS_TASK_ITEM
    /// </summary>
    public class WCS_TASK_ITEM
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// WCS单号
        /// </summary>
        public String WCS_NO { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public String ITEM_ID { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        public String DEVICE { get; set; }

        /// <summary>
        /// 启动位置
        /// </summary>
        public String LOC_FROM { get; set; }

        /// <summary>
        /// 目的位置
        /// </summary>
        public String LOC_TO { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public String STATUS { get; set; }

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
    /// Item作业ID
    /// </summary>
    public class ItemId
    {
        public const String 固定辊台正向 = "111";
        public const String 固定辊台反向 = "112";
        public const String 摆渡车正向 = "113";
        public const String 摆渡车反向 = "114";
        public const String 运输车正向 = "115";
        public const String 运输车反向 = "116";
        public const String 行车取货 = "117";
        public const String 行车放货 = "118";

        public const String 摆渡车复位 = "011";
        public const String 摆渡车定位固定辊台 = "012";
        public const String 摆渡车定位运输车对接 = "013";

        public const String 运输车定位 = "021";
        public const String 运输车复位1 = "022";    // 摆渡车对接待命点
        public const String 运输车复位2 = "023";    // 运输车对接待命点
        public const String 运输车对接定位 = "024";

        public const String 行车轨道定位 = "031";
        public const String 行车库存定位 = "032";
        public const String 行车复位 = "033";

        /// <summary>
        /// 获取Item Id 对应的中文意义
        /// </summary>
        /// <param name="item_id"></param>
        /// <returns></returns>
        public static String GetItemIdName(String item_id)
        {
            String name = "";
            switch (item_id)
            {
                case 固定辊台正向:
                    name = "固定辊台正向滚动";
                    break;
                case 固定辊台反向:
                    name = "固定辊台反向滚动";
                    break;
                case 摆渡车正向:
                    name = "摆渡车辊台正向滚动";
                    break;
                case 摆渡车反向:
                    name = "摆渡车辊台反向滚动";
                    break;
                case 运输车正向:
                    name = "运输车辊台正向滚动";
                    break;
                case 运输车反向:
                    name = "运输车辊台反向滚动";
                    break;
                case 行车取货:
                    name = "行车取货";
                    break;
                case 行车放货:
                    name = "行车放货";
                    break;
                case 摆渡车定位固定辊台:
                    name = "摆渡车移至与固定辊台对接";
                    break;
                case 摆渡车定位运输车对接:
                    name = "摆渡车移至与运输车对接";
                    break;
                case 摆渡车复位:
                    name = "摆渡车复位";
                    break;
                case 运输车定位:
                    name = "运输车定位";
                    break;
                case 运输车复位1:
                    name = "运输车移至靠外待命点";
                    break;
                case 运输车复位2:
                    name = "运输车移至靠内待命点";
                    break;
                case 运输车对接定位:
                    name = "运输车[外]移至与运输车[内]对接";
                    break;
                case 行车轨道定位:
                    name = "行车定位至运输轨道坐标";
                    break;
                case 行车库存定位:
                    name = "行车定位至库存货位坐标";
                    break;
                case 行车复位:
                    name = "行车复位待命点";
                    break;
            }
            return name;
        }
    }

    /// <summary>
    /// Item状态
    /// </summary>
    public class ItemStatus
    {
        public const String 不可执行 = "N";
        public const String 请求执行 = "Q";
        public const String 任务中 = "W";
        public const String 失效 = "X";
        public const String 交接中 = "R";
        public const String 出现异常 = "E";
        public const String 完成任务 = "Y";
    }

    /// <summary>
    /// Item列名
    /// </summary>
    public class ItemColumnName
    {
        public const String 设备编号 = "DEVICE";
        public const String 来源位置 = "LOC_FROM";
        public const String 作业状态 = "STATUS";
    }
}
