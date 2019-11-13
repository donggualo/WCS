using ModuleManager.NDC;
using ModuleManager.NDC.SQL;
using ModuleManager.PUB;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NdcManager
{
    /// <summary>
    /// 用于读取和记录NDC设计的数据库信息
    /// </summary>
    internal class NDCSQLControl
    {
        MySQL mysql;
        public NDCSQLControl()
        {
            mysql = new MySQL();
        }

        private int Yes = 1, No = 0;

        /// <summary>
        /// 读取NDC设置的服务IP和IKEY
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="ikey">标识</param>
        public void ReadNDCServerAndIKEY(out string ip, out int port, out int ikey)
        {
            ip = "";
            port = 0;
            ikey = 0;

            if(CommonSQL.GetWcsParamValue(mysql, "NDC_SERVER_IP",out WCS_PARAM ipp)){
                ip = ipp.VALUE1;
            }

            if (CommonSQL.GetWcsParamValue(mysql, "NDC_SERVER_PORT", out WCS_PARAM portp)){
                port = int.Parse(portp.VALUE1);
            }

            if (CommonSQL.GetWcsParamValue(mysql, "NDC_TASK_IKEY", out WCS_PARAM ikp)){
                ikey = int.Parse(ikp.VALUE1);
            }
        }

        /// <summary>
        /// 更新IKEY值
        /// </summary>
        /// <param name="value"></param>
        public void UpdateIkeyValue(int value)
        {
            CommonSQL.UpdateWcsParamValue(mysql, "NDC_TASK_IKEY", value + "");
        }

        /// <summary>
        /// 读取WCS与NDC位置对应关系
        /// </summary>
        public void ReadWcsNdcSite(out Dictionary<string, string> loadsite,out Dictionary<string, string> unloadsite)
        {
            loadsite = new Dictionary<string, string>();
            unloadsite = new Dictionary<string, string>();
            DataTable dt = mysql.SelectAll("SELECT TYPE,WCSSITE,NDCSITE FROM WCS_NDC_SITE");
            if (CommonSQL.IsNoData(dt))
            {
                return;
            }
            List<WCS_NDC_SITE> list = dt.ToDataList<WCS_NDC_SITE>();
            foreach(WCS_NDC_SITE site in list)
            {
                if (site.TYPE.Equals("loadsite"))
                {
                    loadsite.Add(site.WCSSITE, site.NDCSITE);
                }
                else
                {
                    unloadsite.Add(site.WCSSITE, site.NDCSITE);
                }
            }
        }

        /// <summary>
        /// 读取未完成NDC任务信息
        /// </summary>
        public bool ReadUnFinishTask(out List<WCS_NDC_TASK> list)
        {
            list = new List<WCS_NDC_TASK>();
            string str = "SELECT ID,TASKID,CARRIERID,IKEY,NDCINDEX,LOADSITE,UNLOADSITE,REDIRECTSITE,NDCLOADSITE,NDCUNLOADSITE," +
                "NDCREDIRECTSITE,CREATETIME FROM WCS_NDC_TASK  WHERE FINISH = '{0}'";
            string sql = string.Format(@str, No);
            DataTable dt = mysql.SelectAll(sql);
            if (CommonSQL.IsNoData(dt))
            {
                return false;
            }
            list = dt.ToDataList<WCS_NDC_TASK>();
            return true;
        }

        public bool ReadTempTask(out List<WCS_NDC_TASK_TEMP> list)
        {
            list = new List<WCS_NDC_TASK_TEMP>();
            string sql = "SELECT ID,NDCINDEX,IKEY,CARRIERID FROM WCS_NDC_TASK_TEMP t";
            DataTable dt = mysql.SelectAll(sql);
            if (CommonSQL.IsNoData(dt))
            {
                return false;
            }
            list = dt.ToDataList<WCS_NDC_TASK_TEMP>();
            return true;
        }

        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool CheckExist(NDCItem i)
        {
            string sql = string.Format(@"select id from WCS_NDC_TASK where TASKID = '{0}'",i._mTask.TASKID);
            DataTable dt = mysql.SelectAll(sql);
            if (CommonSQL.IsNoData(dt))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新Task消息到数据库
        /// </summary>
        /// <param name="item"></param>
        public void UpdateNdcItem(NDCItem i)
        {
            string sqls = "update WCS_NDC_TASK t set IKEY = '{0}',NDCINDEX = '{1}',LOADSITE = '{2}',UNLOADSITE = '{3}'" +
                ",REDIRECTSITE = '{4}',NDCLOADSITE = '{5}',NDCUNLOADSITE = '{6}',NDCREDIRECTSITE = '{7}',HADLOAD = '{8}',HADUNLOAD = '{9}'" +
                ",CARRIERID = '{10}',FINISH = '{11}',PAUSE = '{12}', UPDATE_TIME = NOW() where TASKID = '{13}'";
            string sql = string.Format(@sqls, i._mTask.IKEY, i._mTask.NDCINDEX,
                   i._mTask.LOADSITE, i._mTask.UNLOADSITE, i._mTask.REDIRECTSITE,
                   i._mTask.NDCLOADSITE, i._mTask.NDCUNLOADSITE, i._mTask.NDCREDIRECTSITE,
                   i._mTask.HADLOAD ? 1 : 0, i._mTask.HADUNLOAD ? 1 : 0, i.CarrierId, i.IsFinish ? Yes:No,i._mTask.PAUSE ? Yes:No, i._mTask.TASKID);
            mysql.ExcuteSql(sql);
        }

        /// <summary>
        /// 将任务状态置为挂起/执行
        /// </summary>
        /// <param name="i"></param>
        /// <param name="v"></param>
        public void PauseNdcTask(NDCItem i,bool v)
        {
            string sql = string.Format(@"update WCS_NDC_TASK set PAUSE = '{0}' where TASKID = '{1}'", v ? Yes : No, i._mTask.TASKID);
            mysql.ExcuteSql(sql);
        }

        /// <summary>
        /// 插入数据到数据库
        /// </summary>
        /// <param name="i"></param>
        public void InsertNdcItem(NDCItem i)
        {
            string sqls = "INSERT INTO WCS_NDC_TASK(TASKID,IKEY,NDCINDEX,LOADSITE,UNLOADSITE," +
                "REDIRECTSITE,NDCLOADSITE,NDCUNLOADSITE,NDCREDIRECTSITE,HADLOAD,HADUNLOAD,CARRIERID,PAUSE,FINISH) " +
                "VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}','{11}','{12}','{13}')";

            string sql = string.Format(@sqls, i._mTask.TASKID, i._mTask.IKEY, i._mTask.NDCINDEX,
                   i._mTask.LOADSITE, i._mTask.UNLOADSITE, i._mTask.REDIRECTSITE,
                   i._mTask.NDCLOADSITE, i._mTask.NDCUNLOADSITE, i._mTask.NDCREDIRECTSITE,
                   i._mTask.HADLOAD ? 1 : 0, i._mTask.HADUNLOAD ? 1 : 0, i.CarrierId, No, No);
            mysql.ExcuteSql(sql);
        }

        /// <summary>
        /// 插入数据到数据库
        /// </summary>
        /// <param name="i"></param>
        public void InsertTempItem(NDCItem i)
        {
            string sqls = "INSERT INTO WCS_NDC_TASK_TEMP(NDCINDEX, IKEY, CARRIERID) VALUES('{0}', '{1}', '{2}')";
            string sql = string.Format(@sqls, i._mTask.NDCINDEX, i._mTask.IKEY, i.CarrierId);
            mysql.ExcuteSql(sql);
        }

        /// <summary>
        /// 清空临时数据
        /// </summary>
        public void DeleteTempItem()
        {
            mysql.ExcuteSql("DELETE from wcs_ndc_task_temp");
        }

    }
}
