using ModuleManager.PUB;
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
        /// <summary>
        /// 获取公共参数的内容
        /// </summary>
        /// <param name="mysql"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetWcsParamValue(MySQL mysql,string name,out WCS_PARAM param)
        {
            List<WCS_PARAM> list = new List<WCS_PARAM>();
            string sql = string.Format(@"select ID,NAME,INFO,VALUE1,VALUE2,VALUE3,VALUE4,VALUE5,VALUE6 from wcs_param where NAME = '{0}'", name);
            DataTable dt = mysql.SelectAll(sql);
            if (IsNoData(dt))
            {
                param = null;
                return false;
            }
            StringBuilder value = new StringBuilder();
            list = dt.ToDataList<WCS_PARAM>();

            param = list.Count == 0 ? null : list[0];
            return list.Count > 0;
        }


        public static void UpdateWcsParamValue(MySQL mysql,string name,string value)
        {
            string sql = string.Format(@"UPDATE wcs_param set VALUE1 = '{0}' where NAME = '{1}' ", value, name);
            mysql.ExcuteSql(sql);
        }



        public static List<WCS_CONFIG_DEVICE> GetDeviceNameList(MySQL mysql,string type)
        {
            List<WCS_CONFIG_DEVICE> list = new List<WCS_CONFIG_DEVICE>();
            string sql = string.Format(@"select DEVICE,AREA from wcs_config_device where FLAG <> 'N' and TYPE = '{0}'", type);
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
