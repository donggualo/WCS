using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;

namespace PubResourceManager
{
    public class MySQL
    {
        static readonly string conn = ConfigurationManager.AppSettings["MySqlConn"];

        /// <summary>
        /// 获取所有字段数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable SelectAll(string sql)
        {
            try
            {
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(sql, conn);
                DataSet dataSet = new DataSet();
                mySqlDataAdapter.Fill(dataSet, "Table");
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public void ExcuteSql(string sql)
        {
            try
            {
                MySqlConnection sqlcon = new MySqlConnection(conn);
                sqlcon.Open();
                MySqlCommand mySqlCommand = new MySqlCommand(sql, sqlcon);
                mySqlCommand.ExecuteNonQuery();
                sqlcon.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取对应table数据数目
        /// </summary>
        /// <param name="table"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public int GetCount(string table, string conditions)
        {
            try
            {
                string sql = string.Format(@"select count(*) COUNT from {0} where 1 = 1 {1}", table, string.IsNullOrEmpty(conditions.Trim()) ? "" : "and" + conditions);
                DataTable dt = SelectAll(sql);
                int count = Convert.ToInt32(dt.Rows[0]["COUNT"].ToString());
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
