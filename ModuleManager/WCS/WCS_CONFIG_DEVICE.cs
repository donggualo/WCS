using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ModuleManager.WCS
{
    /// <summary>
    /// 设备配置资讯    WCS_CONFIG_DEVICE
    /// </summary>
    public class WCS_CONFIG_DEVICE
    {
        /// <summary>
        /// 设备名
        /// </summary>
        public string DEVICE { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string TYPE { get; set; }

        /// <summary>
        /// 所属区域
        /// </summary>
        public string AREA { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int PORT { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        /// 是否使用
        /// </summary>
        public int IS_UREFUL { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public int IS_LOCK { get; set; }

        /// <summary>
        /// 锁定单号
        /// </summary>
        public string LOCK_ID { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public int FLAG { get; set; }

        /// <summary>
        /// X轴偏差
        /// </summary>
        public int GAP_X { get; set; }

        /// <summary>
        /// Y轴偏差
        /// </summary>
        public int GAP_Y { get; set; }

        /// <summary>
        /// Z轴偏差
        /// </summary>
        public int GAP_Z { get; set; }

        /// <summary>
        /// X轴误差范围
        /// </summary>
        public int LIMIT_X { get; set; }

        /// <summary>sss
        /// Y轴误差范围
        /// </summary>
        public int LIMIT_Y { get; set; }

    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public class DeviceType
    {
        public const string 行车 = "AWC";
        public const string 运输车 = "RGV";
        public const string 摆渡车 = "ARF";
        public const string 固定辊台 = "FRT";
        public const string 包装线辊台 = "PKL";
        public const string AGV = "AGV";

        public static string GetDevTypeName(string dev)
        {
            string name = "";
            switch (dev)
            {
                case DeviceType.AGV:
                    name = "AGV";
                    break;
                case DeviceType.行车:
                    name = "行车";
                    break;
                case DeviceType.运输车:
                    name = "运输车";
                    break;
                case DeviceType.摆渡车:
                    name = "摆渡车";
                    break;
                case DeviceType.固定辊台:
                    name = "固定辊台";
                    break;
                case DeviceType.包装线辊台:
                    name = "包装线辊台";
                    break;
                default:
                    break;
            }
            return name;
        }
    }

}
