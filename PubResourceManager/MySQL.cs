using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Text;

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
                string sql = string.Format(@"select count(*) COUNT from {0} where 1 = 1 {1}", table, string.IsNullOrEmpty(conditions.Trim()) ? "" : "and " + conditions);
                DataTable dt = SelectAll(sql);
                int count = Convert.ToInt32(dt.Rows[0]["COUNT"].ToString());
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// (DataTable)插入数据库表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string InsertDB(string tableName, DataTable data)
        {
            try
            {
                string result = string.Empty;

                if (string.IsNullOrEmpty(tableName))
                {
                    result = "操作失败：请确认需插入的表名！";
                }
                if (data == null || data.Rows.Count == 0)
                {
                    result = "操作失败：无数据！";
                }

                // 构建INSERT语句
                StringBuilder sql = new StringBuilder();

                sql.Append("INSERT INTO " + tableName + "(");
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    sql.Append(data.Columns[i].ColumnName.Trim() + ",");
                }
                sql.Remove(sql.ToString().LastIndexOf(','), 1);
                sql.Append(") VALUES ");
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sql.Append("(");
                    for (int j = 0; j < data.Columns.Count; j++)
                    {
                        sql.Append("'" + data.Rows[i][j].ToString().Trim() + "',");
                    }
                    sql.Remove(sql.ToString().LastIndexOf(','), 1);
                    sql.Append("),");
                }
                sql.Remove(sql.ToString().LastIndexOf(','), 1);
                sql.Append(";");

                int res = -1;

                using (MySqlConnection sqlcon = new MySqlConnection(conn))
                {
                    sqlcon.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql.ToString(), sqlcon))
                    {
                        try
                        {
                            res = cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            res = -1;
                            // Unknown column 'names' in 'field list' 
                            result = "操作失败！" + ex.Message.Replace("Unknown column", "未知列").Replace("in 'field list'", "存在字段集合中！");
                        }
                    }
                    sqlcon.Close();
                }
                if (res > 0)
                {
                    result = string.Format("数据插入结束...共{0}条记录！", res);
                }

                return result;
            }
            catch (Exception ex)
            {
                return "操作失败！" + ex.Message;
            }
        }
    }
}
