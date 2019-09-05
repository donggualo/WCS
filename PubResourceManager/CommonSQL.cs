using ModuleManager.WCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubResourceManager
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public class CommonSQL
    {
        public static List<WCS_CONFIG_DEVICE> GetDeviceNameList(MySQL mysql,string type)
        {
            List<WCS_CONFIG_DEVICE> list = new List<WCS_CONFIG_DEVICE>();
            string sql = string.Format(@"select DEVICE,AREA from wcs_config_device where TYPE = '{0}'", type);
            DataTable dt = mysql.SelectAll(sql);
            if (IsNoData(dt))
            {
                return list;
            }
            list = dt.ToDataList<WCS_CONFIG_DEVICE>();
            return list;
        }

        /// <summary>
        /// 判断DataTable是否无数据
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsNoData(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
