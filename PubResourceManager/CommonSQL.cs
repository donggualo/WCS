using ModuleManager.PUB;
using ModuleManager.WCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace PubResourceManager
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public class CommonSQL
    {
        public static MySQL mysql = new MySQL();

        /// <summary>
        /// 判断DataTable是否无数据
        /// </summary>
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

        #region NDC

        /// <summary>
        /// 获取公共参数的内容
        /// </summary>
        /// <param name="mysql"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetWcsParamValue(string name, out WCS_PARAM param)
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

        public static void UpdateWcsParamValue(string name, string value)
        {
            string sql = string.Format(@"UPDATE wcs_param set VALUE1 = '{0}' where NAME = '{1}' ", value, name);
            mysql.ExcuteSql(sql);
        }

        #endregion

        #region [WCS]

        /// <summary>
        /// 获取作业表头
        /// </summary>
        public static List<WCS_JOB_HEADER> GetJobHeader()
        {
            try
            {
                List<WCS_JOB_HEADER> list = new List<WCS_JOB_HEADER>();
                string sql = string.Format(@"select * from wcs_job_header order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_JOB_HEADER>();
                }
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
                List<WCS_JOB_DETAIL> list = new List<WCS_JOB_DETAIL>();
                string sql = string.Format(@"select * from WCS_JOB_DETAIL order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_JOB_DETAIL>();
                }
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
                List<WCS_WMS_TASK> list = new List<WCS_WMS_TASK>();
                string sql = string.Format(@"select * from wcs_wms_task order by CREATION_TIME");
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_WMS_TASK>();
                }
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
                List<WCS_CONFIG_AREA> list = new List<WCS_CONFIG_AREA>();
                string sql = string.Format(@"select * from wcs_config_area");
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_CONFIG_AREA>();
                }
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
                List<WCS_CONFIG_DEVICE> list = new List<WCS_CONFIG_DEVICE>();
                string sql = string.Format(@"select * from wcs_config_device where TYPE = '{0}'", type);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_CONFIG_DEVICE>();
                }
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
                int num = mysql.GetCount("wcs_wms_task", string.Format(@" TASK_ID = '{0}'", TASK_ID));
                if (num == 0)
                {
                    string sql = string.Format(@"
             insert into wcs_wms_task(TASK_ID, TASK_TYPE, BARCODE, WMS_LOC_FROM, WMS_LOC_TO, FRT) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                        TASK_ID, TASK_TYPE, BARCODE, WMS_LOC_FROM, WMS_LOC_TO, FRT);

                    mysql.ExcuteSql(sql);
                }
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
                else if (string.IsNullOrEmpty(dt.Rows[0]["TASK_ID2"].ToString()))
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
                string sql = string.Format(@"update wcs_config_device set IS_LOCK = '{1}', LOCK_ID1 = '{2}' where DEVICE = '{0}'",
                    devname, islock ? 1 : 0, lockid);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新设备使用信息
        /// </summary>
        public static void UpdateDevInfo(string devname, bool isuseful)
        {
            try
            {
                string sql = string.Format(@"update wcs_config_device set IS_USEFUL = '{1}' where DEVICE = '{0}'",
                    devname, isuseful ? 1 : 0);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新设备使用属性
        /// </summary>
        public static void UpdateDevInfo(string devname, int flag)
        {
            try
            {
                string sql = string.Format(@"update wcs_config_device set FLAG = '{1}' where DEVICE = '{0}'",
                    devname, flag);
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

        /// <summary>
        /// 异常记录
        /// </summary>
        public static void LogErr(string fnc, string remark, string err, string v1 = "", string v2 = "", string v3 = "")
        {
            try
            {
                string sql = string.Format(@"insert into wcs_function_log(FUNCTION_NAME, REMARK, MESSAGE, VALUE1, VALUE2, VALUE3)
		 values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                    fnc, remark, err, v1, v2, v3);

                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region New

        /// <summary>
        /// 更新设备锁定信息
        /// </summary>
        public static void UpdateDevInfo(int tt, string devname, string lockid1, string lockid2, bool islock)
        {
            try
            {
                string sql = string.Format(@"update wcs_config_device set TASK_TYPE = {1}, IS_LOCK = '{2}', LOCK_ID1 = '{3}', LOCK_ID2 = '{4}' 
                    where DEVICE = '{0}'",
                    devname, tt, islock ? 1 : 0, lockid1, lockid2);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WCS坐标资讯
        /// </summary>
        public static WCS_CONFIG_LOC GetWcsLocByTask(string lockid)
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_loc where WMS_LOC = (
       select case when TASK_TYPE = 2 then WMS_LOC_FROM else WMS_LOC_TO end from wcs_wms_task where TASK_ID = '{0}')", lockid);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return new WCS_CONFIG_LOC();
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
        /// 获取WCS坐标资讯
        /// </summary>
        public static WCS_CONFIG_LOC GetWcsLoc(string wmsLoc)
        {
            try
            {
                string sql = string.Format(@"select * from wcs_config_loc where WMS_LOC = '{0}'", wmsLoc);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    return new WCS_CONFIG_LOC();
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
        /// 获取WCS坐标资讯
        /// </summary>
        public static bool UpdateWcsLoc(string WMS_LOC, string RGV_LOC_1, string RGV_LOC_2, string AWC_LOC_TRACK, string AWC_LOC_STOCK)
        {
            try
            {
                if (string.IsNullOrEmpty(WMS_LOC))
                {
                    return false;
                }

                string sql = string.Format(@"update wcs_config_loc set RGV_LOC_1 = '{1}', RGV_LOC_2 = '{2}', 
                        AWC_LOC_TRACK = '{3}', AWC_LOC_STOCK = '{4}', CREATION_TIME = NOW() where WMS_LOC = '{0}'",
                            WMS_LOC, RGV_LOC_1, RGV_LOC_2, AWC_LOC_TRACK, AWC_LOC_STOCK);
                mysql.ExcuteSql(sql);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取坐标
        /// </summary>
        /// <param name="lockid"></param>
        /// <returns></returns>
        public static string GetWcsLocByName(string lockid, string name)
        {
            try
            {
                string loc = "";
                if (!string.IsNullOrEmpty(lockid))
                {
                    string sql = string.Format(@"select * from wcs_config_loc where WMS_LOC = (
       select case when TASK_TYPE = 2 then WMS_LOC_FROM else WMS_LOC_TO end from wcs_wms_task where TASK_ID = '{0}')", lockid);
                    DataTable dt = mysql.SelectAll(sql);
                    if (!IsNoData(dt))
                    {
                        loc = dt.Rows[0][name].ToString();
                    }
                }
                return loc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最新流水号
        /// </summary>
        /// <returns></returns>
        public static string GetSN(string value, out string box)
        {
            try
            {
                string sn = "";
                box = "001";
                string sql = string.Format(@"select VALUE2,VALUE3 from wcs_param where NAME = 'WMS_CODE_SN' and VALUE1 = '{0}'", value);
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    mysql.ExcuteSql(string.Format(@"update wcs_param set VALUE1 = '{0}', VALUE2 = 2, VALUE3 = 2 where NAME = 'WMS_CODE_SN'", value));
                    sn = "001";
                    box = "001";
                }
                else
                {
                    sn = dt.Rows[0]["VALUE2"].ToString().PadLeft(3, '0');
                    sql = string.Format(@"update wcs_param set VALUE2 = (VALUE2 + 1) where NAME = 'WMS_CODE_SN'", value);

                    //box = dt.Rows[0]["VALUE3"].ToString().PadLeft(3, '0');
                    //if (box == "060")
                    //{
                    //    sql = string.Format(@"update wcs_param set VALUE2 = (VALUE2 + 1),VALUE3 = 1 where NAME = 'WMS_CODE_SN'", value);
                    //}
                    //else
                    //{
                    //    sql = string.Format(@"update wcs_param set VALUE2 = (VALUE2 + 1),VALUE3 = (VALUE3 + 1) where NAME = 'WMS_CODE_SN'", value);
                    //}

                    mysql.ExcuteSql(sql);
                }
                return sn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取最新流水号
        /// </summary>
        /// <returns></returns>
        public static int GetAGVid()
        {
            try
            {
                int id = 0;
                string sql = string.Format(@"select VALUE2 from wcs_param where NAME = 'WCS_AGV_ID'");
                DataTable dt = mysql.SelectAll(sql);
                if (IsNoData(dt))
                {
                    mysql.ExcuteSql(string.Format(@"update wcs_param set VALUE2 = 2 where NAME = 'WCS_AGV_ID'"));
                    id = 1;
                }
                else
                {
                    id = Convert.ToInt32(dt.Rows[0]["VALUE2"].ToString());
                    int next = 1;
                    if (id < 65535)
                    {
                        next = id + 1;
                    }
                    mysql.ExcuteSql(string.Format(@"update wcs_param set VALUE2 = '{0}' where NAME = 'WCS_AGV_ID'", next));
                }
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS初始化目的区域
        /// </summary>
        /// <returns></returns>
        public static string GetToArea(string code, out string taskid)
        {
            try
            {
                string area = "";
                taskid = "";
                string sql = string.Format(@"select TASK_ID, WMS_LOC_TO, TASK_STATUS from wcs_wms_task where TASK_TYPE = 1 and BARCODE = '{0}'", code);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    taskid = dt.Rows[0]["TASK_ID"].ToString();
                    area = dt.Rows[0]["WMS_LOC_TO"].ToString();
                    string sta = dt.Rows[0]["TASK_STATUS"].ToString();
                    if (int.Parse(sta) > 1) // 已分配过库位
                    {
                        throw new Exception(string.Format(@"包装线辊台当前二维码[{0}]重复！", code));
                    }
                }
                return area;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否超时入库任务
        /// </summary>
        /// <returns></returns>
        public static bool IsTimeOut(string tid)
        {
            try
            {
                bool res = false;
                string sql = string.Format(@"select 1 from wcs_wms_task where TASK_ID = '{0}' and
     TRUNCATE((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(UPDATE_TIME))/60,0) >= 30", tid);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS待分配超时入库任务
        /// </summary>
        /// <returns></returns>
        public static string GetTimeOut(string area)
        {
            try
            {
                string tid = "";
                string sql = string.Format(@"select TASK_ID from wcs_wms_task where TASK_TYPE = 1 and TASK_STATUS = 1 and FRT = '{0}' and
     TRUNCATE((UNIX_TIMESTAMP(NOW())-UNIX_TIMESTAMP(UPDATE_TIME))/60,0) >= 30 order by UPDATE_TIME limit 1", area);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    tid = dt.Rows[0]["TASK_ID"].ToString();
                }
                return tid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否存在wms入库任务
        /// </summary>
        /// <returns></returns>
        public static bool IsExistsInTask(string code)
        {
            try
            {
                bool res = false;
                string sql = string.Format(@"select 1 from wcs_wms_task where TASK_TYPE = 1 and TASK_STATUS = 2 and BARCODE = '{0}'", code);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    // 已分配过
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS待分配入库任务
        /// </summary>
        /// <returns></returns>
        public static bool GetInTask(string code, out string tid)
        {
            try
            {
                bool res = false;
                tid = "";
                string sql = string.Format(@"select TASK_ID from wcs_wms_task where TASK_TYPE = 1 and TASK_STATUS <> 3 and BARCODE = '{0}'", code);
                DataTable dt = mysql.SelectAll(sql);
                if (dt != null && dt.Rows.Count != 0)
                {
                    tid = dt.Rows[0]["TASK_ID"].ToString();
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS待分配入库任务(2个)
        /// </summary>
        /// <returns></returns>
        public static bool GetInTaskDouble(string area, out string tid1, out string tid2)
        {
            try
            {
                bool res = false;
                tid1 = "";
                tid2 = "";
                string sql = string.Format(@"select TASK_ID from wcs_wms_task where TASK_TYPE = 1 and TASK_STATUS = 1 and 
                    WMS_LOC_TO = '{0}' order by UPDATE_TIME limit 2", area);
                DataTable dt = mysql.SelectAll(sql);
                if (dt != null && dt.Rows.Count == 2)
                {
                    tid1 = dt.Rows[0]["TASK_ID"].ToString();
                    tid2 = dt.Rows[1]["TASK_ID"].ToString();
                    res = true;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS待执行入库任务(1-2个)
        /// </summary>
        /// <returns></returns>
        public static List<WCS_WMS_TASK> GetInTaskInfo(string area)
        {
            try
            {
                List<WCS_WMS_TASK> list = new List<WCS_WMS_TASK>();
                string sql = string.Format(@"select * from wcs_wms_task where WMS_LOC_FROM = '{0}' and TASK_TYPE = 1 and TASK_STATUS = 2 
                    and TASK_ID not in 
                        (select LOCK_ID1 donoID from wcs_config_device where TYPE in ('AWC','FRT') and LOCK_ID1 <>'' or LOCK_ID1 <> null
                        UNION
                        select LOCK_ID2 donoID from wcs_config_device where TYPE in ('AWC','FRT') and LOCK_ID2 <>'' or LOCK_ID2 <> null) 
                        order by UPDATE_TIME LIMIT 2", area);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_WMS_TASK>();
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取WMS待执行出库任务(1-4个)
        /// </summary>
        /// <returns></returns>
        public static List<WCS_WMS_TASK> GetOutTaskInfo(string area)
        {
            try
            {
                List<WCS_WMS_TASK> list = new List<WCS_WMS_TASK>();
                string sql = string.Format(@"select * from wcs_wms_task where WMS_LOC_TO = '{0}' and TASK_TYPE = 2 and TASK_STATUS = 0 
                    order by CREATION_TIME limit 4", area);
                DataTable dt = mysql.SelectAll(sql);
                if (!IsNoData(dt))
                {
                    list = dt.ToDataList<WCS_WMS_TASK>();
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新WMS任务状态
        /// </summary>
        public static void UpdateWms(string taskid, int status, string from = "", string to = "")
        {
            try
            {
                string sql = string.Format(@"update wcs_wms_task set UPDATE_TIME = CURRENT_TIMESTAMP, TASK_STATUS = {0}", status);
                if (!string.IsNullOrEmpty(from))
                {
                    sql = string.Format(@"{0}, WMS_LOC_FROM = '{1}'", sql, from);
                }
                if (!string.IsNullOrEmpty(to))
                {
                    sql = string.Format(@"{0}, WMS_LOC_TO = '{1}'", sql, to);
                }
                sql = string.Format(@"{0} where TASK_ID = '{1}'", sql, taskid);
                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删任务数据
        /// </summary>
        public static void DeleteWms(string TASK_ID)
        {
            try
            {
                string sql = string.Format(@"
             delete from wcs_wms_task where TASK_ID = '{0}'", TASK_ID);

                mysql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取公共参数的内容
        /// </summary>
        /// <param name="mysql"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetWcsParam(string name, out List<WCS_PARAM> info)
        {
            info = new List<WCS_PARAM>();
            string sql = string.Format(@"select * from wcs_param where NAME = '{0}'", name);
            DataTable dt = mysql.SelectAll(sql);
            if (IsNoData(dt))
            {
                return false;
            }
            info = dt.ToDataList<WCS_PARAM>();
            return true;
        }

        #endregion

        #region Excel

        /// <summary>
        /// 导入Excel文件转为 DataTable
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataTable GetExcelData(string path)
        {
            try
            {
                //连接语句，读取文件路劲
                string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + "Extended Properties=Excel 12.0;";
                //查询Excel表名，默认是Sheet1
                string strExcel = "select * from [Sheet1$]";

                OleDbConnection ole = new OleDbConnection(strConn);
                ole.Open(); //打开连接
                DataTable dt = new DataTable();
                OleDbDataAdapter odp = new OleDbDataAdapter(strExcel, strConn);
                odp.Fill(dt);
                ole.Close();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
