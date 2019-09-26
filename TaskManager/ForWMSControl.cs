using ModuleManager.WCS;
using System;
using System.Data;
using WcsHttpManager;

namespace TaskManager
{
    public class ForWMSControl
    {
        /// <summary>
        /// 获取WMS资讯写入WCS数据库
        /// </summary>
        /// <param name="wms"></param>
        public bool WriteTaskToWCS(WmsModel wms, out string result)
        {
            try
            {
                String sql = String.Format(@"insert into wcs_task_info(TASK_UID, TASK_TYPE, BARCODE, W_S_LOC, W_D_LOC) values('{0}','{1}','{2}','{3}','{4}')",
                    wms.Task_UID, wms.Task_type.GetHashCode(), wms.Barcode, wms.W_S_Loc, wms.W_D_Loc);
                DataControl._mMySql.ExcuteSql(sql);
                result = "";
                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("WriteTaskToWCS()", "WMS请求作业[任务ID，作业类型]", wms.Task_UID, wms.Task_type.ToString(), ex.ToString());
                result = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 扫码任务(包装线)
        /// </summary>
        /// <param name="frt"></param>
        /// <param name="code"></param>
        public bool ScanCodeTask_P(string frt, string code)
        {
            try
            {
                // 获取Task资讯
                String sql = String.Format(@"select * from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.AGV搬运);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (!DataControl._mStools.IsNoData(dt))
                {
                    // 存在Task资讯则略过
                    return false;
                }
                // 无Task资讯则新增
                // 呼叫WMS 请求入库资讯---区域
                WmsModel wms = DataControl._mHttp.DoBarcodeScanTask(DataControl._mTaskTools.GetArea(frt), code);
                wms.Task_type = WmsStatus.Empty;
                // 写入数据库
                return WriteTaskToWCS(wms, out string result);
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask_P()", "扫码任务(包装线)[扫码位置,码数]", frt, code, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 扫码任务(卸货区)
        /// </summary>
        /// <param name="frt"></param>
        /// <param name="code"></param>
        public bool ScanCodeTask_D(string frt, string code)
        {
            try
            {
                // 辊台对应区域
                String area = DataControl._mTaskTools.GetArea(frt);

                // 获取Task资讯(第1次扫码)
                String sql1 = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.AGV搬运);
                DataTable dt1 = DataControl._mMySql.SelectAll(sql1);
                if (DataControl._mStools.IsNoData(dt1)) // 不满足第1次扫码条件
                {
                    string errmes;
                    // 获取Task资讯(重复扫码)
                    String sql2 = String.Format(@"select * from wcs_task_info where BARCODE = '{0}' and W_S_LOC = '{1}' and TASK_TYPE = '{2}'", code, area, TaskType.入库);
                    DataTable dt2 = DataControl._mMySql.SelectAll(sql2);
                    if (DataControl._mStools.IsNoData(dt2)) 
                    {
                        // 不存在Task
                        errmes = string.Format(@"固定辊台[{0}]承载货物编号[{1}]：不存在WMS入库任务ID！", frt, code);

                        // LOG
                        DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask_D()", "扫码任务(卸货区)[扫码位置,码数]", frt, code, errmes.ToString());
                        return false;
                    }
                    else
                    {
                        // 存在Task
                        errmes = string.Format(@"固定辊台[{0}]承载货物编号[{1}]：重复货物码！", frt, code);

                        // LOG
                        DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask_D()", "扫码任务(卸货区)[扫码位置,码数]", frt, code, errmes.ToString());
                        return false;
                    }
                }
                else // 满足第1次扫码条件
                {
                    // 更新Task
                    String sqlupdate = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '' where TASK_TYPE = '{2}' and BARCODE = '{3}'", 
                        TaskType.入库, area, TaskType.AGV搬运, code);
                    DataControl._mMySql.ExcuteSql(sqlupdate);

                    // 获取对应任务ID
                    string taskuid = dt1.Rows[0]["TASK_UID"].ToString();
                    // 记录WCS入库清单
                    DataControl._mTaskTools.CreateCommandIn(taskuid, frt);
                }
                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask_D()", "扫码任务(卸货区)[扫码位置,码数]", frt, code, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 单托库位
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool DoInTask_One(WCS_COMMAND_V cmd)
        {
            try
            {
                // 呼叫WMS 请求入库资讯---单托库位
                WmsModel wms = DataControl._mHttp.DoReachStockinPosTask(cmd.LOC_FROM_1, cmd.TASK_UID_1);
                // 更新任务资讯
                String sql = String.Format(@"update wcs_task_info set UPDATE_TIME = NOW(), W_D_LOC = '{0}' where TASK_UID = '{1}';
                                             update wcs_command_master set UPDATE_TIME = NOW(), STEP = {2} where WCS_NO = {3}",
                    wms.W_D_Loc, cmd.TASK_UID_1, CommandStep.请求执行, cmd.WCS_NO);
                DataControl._mMySql.ExcuteSql(sql);

                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("DoInTask_One()", "请求WMS分配库位[WCS入库清单号]", cmd.WCS_NO, "", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 双托库位
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool DoInTask_Two(WCS_COMMAND_V cmd)
        {
            try
            {
                // 呼叫WMS 请求入库资讯---双托库位

                // 更新任务资讯

                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("DoInTask_Two()", "请求WMS分配库位[WCS入库清单号]", cmd.WCS_NO, "", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 扫码任务--分配库位
        /// </summary>
        /// <param name="frt"></param>
        /// <param name="code"></param>
        public bool ScanCodeTask_Loc(string frt, string code)
        {
            try
            {
                // 获取Task资讯
                String sql = String.Format(@"select TASK_UID from wcs_task_info where TASK_TYPE = '{1}' and BARCODE = '{0}'", code, TaskType.AGV搬运);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    string errmes = string.Format(@"固定辊台[{0}]承载货物编号[{1}]：不存在WMS入库任务ID！", frt, code);
                    // LOG
                    DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask()", "扫码任务-分配库位[扫码位置,码数]", frt, code, errmes.ToString());
                    return false;
                }

                // 获取对应任务ID
                string taskuid = dt.Rows[0]["TASK_UID"].ToString();
                // 呼叫WMS 请求入库资讯---库位
                WmsModel wms = DataControl._mHttp.DoReachStockinPosTask(DataControl._mTaskTools.GetArea(frt), taskuid);
                // 更新任务资讯
                sql = String.Format(@"update WCS_TASK_INFO set UPDATE_TIME = NOW(), TASK_TYPE = '{0}', W_S_LOC = '{1}', W_D_LOC = '{2}' where TASK_UID = '{3}'",
                    TaskType.入库, wms.W_S_Loc, wms.W_D_Loc, taskuid);
                DataControl._mMySql.ExcuteSql(sql);

                // 对应 WCS 清单
                DataControl._mTaskTools.CreateCommandIn(taskuid, frt);

                return true;
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("ScanCodeTask()", "扫码任务-分配库位[扫码位置,码数]", frt, code, ex.ToString());
                return false;
            }
        }
    }
}
