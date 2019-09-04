using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Functions;
using TaskManager.Models;

namespace TaskManager
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public class CommonSQL
    {
        public static List<WCS_CONFIG_DEVICE> GetDeviceNameList(string type)
        {
            List<WCS_CONFIG_DEVICE> list = new List<WCS_CONFIG_DEVICE>();
            string sql = string.Format(@"select DEVICE,AREA from wcs_config_device where TYPE = '{0}'", type);
            DataTable dt = DataControl._mMySql.SelectAll(sql);
            if (DataControl._mStools.IsNoData(dt))
            {
                return list;
            }
            list = dt.ToDataList<WCS_CONFIG_DEVICE>();
            return list;
        }
    }
}
