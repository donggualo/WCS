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
            DataTable dt = mysql.SelectAll("SELECT ID,TASKID,IKEY,ORDERINDEX,LOADSITE,UNLOADSITE,REDIRECTSITE,NDCLOADSITE,NDCUNLOADSITE,NDCREDIRECTSITE,CREATETIME FROM WCS_NDC_TASK");
            if (CommonSQL.IsNoData(dt))
            {
                return false;
            }
            list = dt.ToDataList<WCS_NDC_TASK>();
            return true;
        }

        public void DelectUnFinishTaskAfter()
        {
            string sql = string.Format(@"delete from wcs_ndc_task");
            mysql.ExcuteSql(sql);
        }

        public void SaveUnFinishTask(List<NDCItem> list,List<TempItem> tlist)
        {
            string sql1 = "INSERT INTO WCS_NDC_TASK(TASKID,IKEY,ORDERINDEX,LOADSITE,UNLOADSITE," +
                "REDIRECTSITE,NDCLOADSITE,NDCUNLOADSITE,NDCREDIRECTSITE,HADLOAD,HADUNLOAD,CREATETIME) " +
                "VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}')";

            string sql2 = "INSERT INTO WCS_NDC_TASK(TASKID,IKEY,LOADSITE,UNLOADSITE,REDIRECTSITE," +
                "NDCLOADSITE,NDCUNLOADSITE,NDCREDIRECTSITE,CREATETIME) " +
                "VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')";

            foreach (var i in list)
            {
                if (i.IsFinish) continue;
                string sql = string.Format(@sql1, i._mTask.TASKID,i._mTask.IKEY,i._mTask.ORDERINDEX,
                    i._mTask.LOADSITE,i._mTask.UNLOADSITE,i._mTask.REDIRECTSITE,
                    i._mTask.NDCLOADSITE,i._mTask.NDCUNLOADSITE,i._mTask.NDCREDIRECTSITE,
                    i._mTask.HADLOAD?1:0,i._mTask.HADUNLOAD?1:0,DateTime.Now);
                mysql.ExcuteSql(sql);
            }

            foreach(var i in tlist)
            {
                string sql = string.Format(@sql2, i.TaskID, i.IKey, i.LoadSite,i.UnloadSite,i.RedirectSite,
                    i.NdcLoadSite, i.NdcUnloadSite,i.NdcRedirectSite, DateTime.Now);
                mysql.ExcuteSql(sql);
            }
        }
    }
}
