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
        public static bool GetWcsParamValue(MySQL mysql, string name, out WCS_PARAM param)
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

        public static void UpdateWcsParamValue(MySQL mysql, string name, string value)
        {
            string sql = string.Format(@"UPDATE wcs_param set VALUE1 = '{0}' where NAME = '{1}' ", value, name);
            mysql.ExcuteSql(sql);
        }

        public static List<WCS_CONFIG_DEVICE> GetDeviceNameList(MySQL mysql, string type)
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


        #region [WCS]
        private static MySQL mysql = new MySQL();

        /// <summary>
        /// 获取作业表头
        /// </summary>
        public static List<WCS_JOB_HEADER> GetJobHeader()
        {
            try
            {
                string sql = string.Format(@"select * from wcs_job_header order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                List<WCS_JOB_HEADER> list = dt.ToDataList<WCS_JOB_HEADER>();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取作业表体
        /// </summary>
        public static List<WCS_JOB_DETAIL> GetJobDetail()
        {
            try
            {
                string sql = string.Format(@"select * from WCS_JOB_DETAIL order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                List<WCS_JOB_DETAIL> list = dt.ToDataList<WCS_JOB_DETAIL>();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最新可用的 作业表体ID
        /// </summary>
        /// <returns></returns>
        public static int GetNewJobDetailID()
        {
            try
            {
                string sql = string.Format(@"select IFNULL(MAX(ID),0) MAXID from wcs_job_detail");
                DataTable dt = mysql.SelectAll(sql);
                return Convert.ToInt32(dt.Rows[0]["MAXID"].ToString()) + 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS任务资讯
        /// </summary>
        public static List<WCS_WMS_TASK> GetTaskInfo()
        {
            try
            {
                string sql = string.Format(@"select * from wcs_wms_task order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                List<WCS_WMS_TASK> list = dt.ToDataList<WCS_WMS_TASK>();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WCS坐标资讯
        /// </summary>
        public static WCS_CONFIG_LOC GetLocInfo(string wmsLoc)
        {
            try
            {
                string sql = string.Format(@"select * From wcs_config_loc where WMS_LOC = '{0}'", wmsLoc);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                WCS_CONFIG_LOC loc = dt.ToDataEntity<WCS_CONFIG_LOC>();
                return loc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取摆渡车对接固定辊台坐标
        /// </summary>
        public static int GetArfByFrt(string frt)
        {
            try
            {
                int arf;
                string sql = string.Format(@"select distinct ARF_LOC from wcs_config_loc where FRT_LOC = '{0}'", frt);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    arf = 0;
                }
                else
                {
                    arf = Convert.ToInt32(dt.Rows[0]["ARF_LOC"].ToString() ?? "0");
                }
                return arf;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WCS区域资讯
        /// </summary>
        public static List<WCS_CONFIG_AREA> GetAreaInfo()
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_area");
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                List<WCS_CONFIG_AREA> list = dt.ToDataList<WCS_CONFIG_AREA>();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WCS设备资讯
        /// </summary>
        public static List<WCS_CONFIG_DEVICE> GetDevInfo(string type)
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_device where IS_USEFUL = 1 and TYPE = '{0}'", type);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return null;
                }
                List<WCS_CONFIG_DEVICE> list = dt.ToDataList<WCS_CONFIG_DEVICE>();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 写入表头数据
        /// </summary>
        public static void InsertJobHeader(string JOB_ID, string AREA, int JOB_TYPE, string TASK_ID1, string TASK_ID2, int DEV_FLAG, string FRT)
        {
            try
            {
                string sql = string.Format(@"insert into wcs_job_header(JOB_ID, AREA, JOB_TYPE, TASK_ID1, TASK_ID2, DEV_FLAG, FRT) 
                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}') ", JOB_ID, AREA, JOB_TYPE, TASK_ID1, TASK_ID2, DEV_FLAG, FRT);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 写入表体数据
        /// </summary>
        public static void InsertJobDetail(int ID, string JOB_ID, string AREA, int TASK_TYPE, string TASK_ID, string DEV_TYPE, int DEV_FLAG, string DEVICE,
            string DEV_FROM, string DEV_TO, int TAKE_NUM, int TAKE_SITE_X, int TAKE_SITE_Y, int TAKE_SITE_Z, int GIVE_NUM, int GIVE_SITE_X, int GIVE_SITE_Y, int GIVE_SITE_Z)
        {
            try
            {
                string sql = string.Format(@"insert into wcs_job_detail(ID, JOB_ID, AREA, TASK_TYPE, TASK_ID, DEV_TYPE,
										                                DEV_FLAG, DEVICE, DEV_FROM, DEV_TO,
                                                                        TAKE_NUM, TAKE_SITE_X, TAKE_SITE_Y, TAKE_SITE_Z,
													                    GIVE_NUM, GIVE_SITE_X, GIVE_SITE_Y, GIVE_SITE_Z)
                                                                 VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', 
                                                                        '{6}', '{7}', '{8}', '{9}', 
                                                                        '{10}', '{11}', '{12}', '{13}', 
                                                                        '{14}', '{15}', '{16}', '{17}')",
                       ID, JOB_ID, AREA, TASK_TYPE, TASK_ID, DEV_TYPE, DEV_FLAG, DEVICE, DEV_FROM, DEV_TO,
                       TAKE_NUM, TAKE_SITE_X, TAKE_SITE_Y, TAKE_SITE_Z, GIVE_NUM, GIVE_SITE_X, GIVE_SITE_Y, GIVE_SITE_Z);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 写入任务数据(先删后写)
        /// </summary>
        public static void InsertTaskInfo(string TASK_ID, int TASK_TYPE, string BARCODE, string WMS_LOC_FROM, string WMS_LOC_TO, string FRT)
        {
            try
            {
                string sql = string.Format(@"
             delete from wcs_wms_task where TASK_ID = '{0}';
             insert into wcs_wms_task(TASK_ID, TASK_TYPE, BARCODE, WMS_LOC_FROM, WMS_LOC_TO, FRT) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                    TASK_ID, TASK_TYPE, BARCODE, WMS_LOC_FROM, WMS_LOC_TO, FRT);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 更新表头状态
        /// </summary>
        public static void UpdateJobHeader(string jobid, int status)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_header set UPDATE_TIME = CURRENT_TIMESTAMP, JOB_STATUS = {1} where JOB_ID = '{0}'", jobid, status);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新表头任务号
        /// </summary>
        public static void UpdateJobHeader(string jobid, string wmsTask)
        {
            try
            {
                string sql = string.Format(@"select TASK_ID1,TASK_ID2 from wcs_job_header where JOB_ID = '{0}'", jobid);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt)) return;

                if (string.IsNullOrEmpty(dt.Rows[0]["TASK_ID1"].ToString()))
                {
                    sql = string.Format(@"update wcs_job_header set UPDATE_TIME = CURRENT_TIMESTAMP, TASK_ID1 = {1} where JOB_ID = '{0}'", jobid, wmsTask);
                }
                else if(string.IsNullOrEmpty(dt.Rows[0]["TASK_ID2"].ToString()))
                {
                    sql = string.Format(@"update wcs_job_header set UPDATE_TIME = CURRENT_TIMESTAMP, TASK_ID2 = {1} where JOB_ID = '{0}'", jobid, wmsTask);
                }
                else
                {
                    return;
                }
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新表体状态
        /// </summary>
        public static void UpdateJobDetail(int id, int status)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_detail set UPDATE_TIME = CURRENT_TIMESTAMP, TASK_STATUS = {1} where ID = {0}", id, status);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新表体设备信息
        /// </summary>
        public static void UpdateJobDetail(int id, string dev)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_detail set UPDATE_TIME = CURRENT_TIMESTAMP, DEVICE = '{1}' where ID = {0}", id, dev);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新表体坐标信息
        /// </summary>
        public static void UpdateJobDetail(int id, int takeX, int takeY, int takeZ, int giveX, int giveY, int giveZ)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_detail set UPDATE_TIME = CURRENT_TIMESTAMP, TAKE_SITE_X= {1}, TAKE_SITE_Y= {2},
TAKE_SITE_Z= {3}, GIVE_SITE_X= {4}, GIVE_SITE_Y= {5}, GIVE_SITE_Z= {6} where ID = {0}", id, takeX, takeY, takeZ, giveX, giveY, giveZ);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新接货坐标（摆渡车）
        /// </summary>
        public static void UpdateTakeSite(int id, int site)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_detail set UPDATE_TIME = CURRENT_TIMESTAMP, TAKE_SITE_X = {1} where ID = {0}", id, site);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新送货坐标（摆渡车）
        /// </summary>
        public static void UpdateGiveSite(int id, int site)
        {
            try
            {
                string sql = string.Format(@"update wcs_job_detail set UPDATE_TIME = CURRENT_TIMESTAMP, GIVE_SITE_X = {1} where ID = {0}", id, site);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        public static void UpdateTaskInfo(string taskid, int status)
        {
            try
            {
                string sql = string.Format(@"update wcs_wms_task set UPDATE_TIME = CURRENT_TIMESTAMP, TASK_STATUS = {1} where TASK_ID = '{0}'", taskid, status);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新设备锁定信息
        /// </summary>
        public static void UpdateDevInfo(string devname, string lockid, bool islock)
        {
            try
            {
                string sql = string.Format(@"update wcs_config_device set IS_LOCK = '{1}', LOCK_ID = '{2}' where DEVICE = '{0}'",
                    devname, islock ? 1 : 0, lockid);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 备份
        /// </summary>
        public static void Backup(string jobid, string remark = "")
        {
            try
            {
                string sql = string.Format(@"
                /*备份*/
                    insert into wcs_task_backup(JOB_ID, TASK_ID, TASK_TYPE, BARCODE, WMS_LOC_FROM, WMS_LOC_TO, REMARK)
                    select H.JOB_ID, W.TASK_ID, W.TASK_TYPE, W.BARCODE, W.WMS_LOC_FROM, W.WMS_LOC_TO, '{1}' REMARK 
                      from wcs_job_header H, wcs_wms_task W
                     where (H.TASK_ID1 = W.TASK_ID or H.TASK_ID2 = W.TASK_ID) and H.JOB_ID = '{0}';
                /*清任务*/
                    delete from wcs_wms_task where TASK_ID in
                        (select TASK_ID1 ti from wcs_job_header where JOB_ID = '{0}'
	                      union 
	                     select TASK_ID2 ti from wcs_job_header where JOB_ID = '{0}');
                /*清明细*/ 
                    delete from wcs_job_detail where JOB_ID = '{0}';
                /*清作业*/
                    delete from wcs_job_header where JOB_ID = '{0}';
                /*解设备*/
                    update wcs_config_device set IS_LOCK = 0, LOCK_ID = null where LOCK_ID = '{0}'", 
                    jobid, remark);

                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
