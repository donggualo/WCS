using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Models;
using System.Data;
using WCS_phase1.Functions;
using System.Configuration;
using WCS_phase1.Http;

namespace WCS_phase1.Action
{
    class ForWMSControl
    {
        /// <summary>
        /// 获取WMS资讯写入WCS数据库
        /// </summary>
        /// <param name="wms"></param>
        public bool WriteTaskToWCS(WmsModel wms)
        {
            try
            {
                String sql = String.Format(@"insert into wcs_task_info(TASK_UID, TASK_TYPE, BARCODE, W_S_LOC, W_D_LOC) values('{0}','{1}','{2}','{3}','{4}')",
                    wms.Task_UID, wms.Task_type.ToString(), wms.Barcode, wms.W_S_Loc, wms.W_D_Loc);
                DataControl._mMySql.ExcuteSql(sql);
                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("WriteTaskToWCS()", "WMS请求作业[任务ID，作业类型]", wms.Task_UID, wms.Task_type.ToString(), ex.ToString());
                return false;
            }
        }


        public void ScanCodeTask(string loc, string code)
        {
            try
            {
                WmsModel wms;
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where SITE = 'N' and TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.AGV搬运);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    // 无资讯则新增
                    // 呼叫WMS 请求入库资讯---区域
                    wms = DataControl._mHttp.DoBarcodeScanTask(loc, code);
                    // 写入数据库
                    WriteTaskToWCS(wms);
                }
                // 获取对应任务ID
                string taskuid = dt.Rows[0]["TASK_UID"].ToString();
                // 呼叫WMS 请求入库资讯---库位
                wms = DataControl._mHttp.DoReachStockinPosTask(loc, taskuid);
                // 更新任务资讯
                sql = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '{2}' where TASK_UID = '{3}'",
                    TaskType.入库, wms.W_S_Loc, wms.W_D_Loc, taskuid);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask()", "扫码任务[扫码位置,码数]", loc, code, ex.ToString());
            }
        }
    }
}
