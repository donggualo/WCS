using System;
using System.Data;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.OleDb;
using Microsoft.Win32;

namespace TaskManager.Functions
{
    public class SimpleTools
    {
        /// <summary>
        /// 判断DataTable是否无数据
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool IsNoData(DataTable dt)
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

        #region Byte

        /// <summary>
        /// int 转 byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }

        /// <summary>
        /// byte[] 转 int
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int BytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8)
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24));
            return value;
        }

        /// <summary>
        /// byte[] 转 String
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public string BytetToString(byte[] byteArray)
        {
            var str = new System.Text.StringBuilder();
            for (int i = 0; i < byteArray.Length; i++)
            {
                str.Append(String.Format("{0:X} ", byteArray[i]));//var拼接
            }
            string s = str.ToString();
            return s;
        }
        #endregion

        #region AppConfig

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueByKey(string key)
        {
            ConfigurationManager.RefreshSection("appSettings");
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 修改配置值
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="value"></param>
        public void ModifyAppSettings(string strKey, string value)
        {
            //获得配置文件的全路径  
            var assemblyConfigFile = Assembly.GetEntryAssembly().Location;
            var appDomainConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ChangeConfiguration(strKey, value, assemblyConfigFile);
            ModifyAppConfig(strKey, value, appDomainConfigFile);
        }
        private void ModifyAppConfig(string strKey, string value, string configPath)
        {
            var doc = new XmlDocument();
            doc.Load(configPath);

            //找出名称为“add”的所有元素  
            var nodes = doc.GetElementsByTagName("add");
            for (int i = 0; i < nodes.Count; i++)
            {
                //获得将当前元素的key属性  
                var xmlAttributeCollection = nodes[i].Attributes;
                if (xmlAttributeCollection != null)
                {
                    var att = xmlAttributeCollection["key"];
                    if (att == null) continue;
                    //根据元素的第一个属性来判断当前的元素是不是目标元素  
                    if (att.Value != strKey) continue;
                    //对目标元素中的第二个属性赋值  
                    att = xmlAttributeCollection["value"];
                    att.Value = value;
                }
                break;
            }

            //保存上面的修改  
            doc.Save(configPath);
            ConfigurationManager.RefreshSection("appSettings");
        }
        public void ChangeConfiguration(string key, string value, string path)
        {
            var config = ConfigurationManager.OpenExeConfiguration(path);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion

        #region Window

        /// <summary>
        /// 打开唯一窗口
        /// </summary>
        /// <param name="w"></param>
        public void ShowWindow(Window w)
        {
            if (w == null || w.IsVisible == false)
            {
                w.Show();
            }
            else
            {
                w.Activate();
                w.WindowState = System.Windows.WindowState.Normal;
            }
        }

        #endregion

        #region Excel

        /// <summary>
        /// (DataGrid)导出为Excel文件
        /// </summary>
        /// <param name="dg"></param>
        public void SaveToExcel(System.Windows.Controls.DataGrid dg)
        {
            string fileName = "";
            string saveFileName = "";
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                DefaultExt = "xlsx",
                Filter = "Excel 文件|*.xlsx",
                FileName = fileName
            };
            saveDialog.ShowDialog();
            saveFileName = saveDialog.FileName;
            if (saveFileName.IndexOf(":") < 0) return;  //被点了取消
            Excel.Application xlApp = new Excel.Application();
            if (xlApp == null)
            {
                System.Windows.MessageBox.Show("无法创建Excel对象，您可能未安装Excel");
                return;
            }
            Excel.Workbooks workbooks = xlApp.Workbooks;
            Excel.Workbook workbook = workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1]; //取得sheet1

            //写入行
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dg.Columns[i].Header;
            }
            for (int r = 0; r < dg.Items.Count; r++)
            {
                for (int i = 0; i < dg.Columns.Count; i++)
                {
                    //if ((dg.Columns[i].GetCellContent(dg.Items[r]).ToString() == "System.Windows.Controls.ContentPresenter"))
                    //{
                    //    continue; // 按钮控件类型
                    //}

                    worksheet.Cells[r + 2, i + 1] = (dg.Columns[i].GetCellContent(dg.Items[r]) as TextBlock).Text;   //读取DataGrid某一行某一列的信息内容
                }
                System.Windows.Forms.Application.DoEvents();
            }
            worksheet.Columns.EntireColumn.AutoFit();
            System.Windows.MessageBox.Show(fileName + "保存成功");
            if (saveFileName != "")
            {
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(saveFileName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("导出文件可能正在被打断!" + ex.Message);
                }
            }
            xlApp.Quit();
            GC.Collect();
        }

        public DataTable GetExcelData(string path)
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
