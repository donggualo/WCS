using ModuleManager.PUB;
using Newtonsoft.Json;
using PubResourceManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WcsHttpManager
{
    /// <summary>
    /// 处理请求WMS和返回的结果
    /// </summary>
    public class HttpControl
    {
        public string wcsUrl = "";
        public string serverName = "WMS";
        public string CommandEnd = "END";

        public HttpControl()
        {
            //wcsUrl = "http://10.9.31.66/wms.php";

            ReadWmsServerUrl();
        }

        private void ReadWmsServerUrl()
        {
            if(CommonSQL.GetWcsParamValue("WMS_SERVER_URL", out WCS_PARAM param))
            {
                wcsUrl = param.VALUE1;
            }
            else
            {
                Console.WriteLine("在wcs_param表找不到(WMS_SERVER_URL)WMS服务配置内容");
            }
        }

        #region 入库任务

        /// <summary>
        /// 进仓扫码
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public WmsModel DoBarcodeScanTask(string from,string barcode)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockInTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.StockInTask+",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.Barcode + "=" + barcode + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return JsonConvert.DeserializeObject<WmsModel>(result);
        }

        /// <summary>
        /// 进仓到达位置
        /// </summary>
        /// <returns></returns>
        public WmsModel DoReachStockinPosTask(string from,string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockInTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.SiteArrived + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return JsonConvert.DeserializeObject<WmsModel>(result);
        }


        /// <summary>
        /// 进仓完成状态
        /// </summary>
        /// <param name="from"></param>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public string DoStockInFinishTask(string from,string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockInTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskFinish + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        #endregion

        #region 出仓任务

        /// <summary>
        /// 出仓完成
        /// </summary>
        /// <param name="from"></param>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public string DoStockOutFinishTask(string from, string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockOutTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskFinish + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        /// <summary>
        /// 出仓异常状态/暂停
        /// </summary>
        /// <param name="from"></param>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public WmsModel DoStockOutErrorTask(string from, string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockOutTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskSuspend + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return JsonConvert.DeserializeObject<WmsModel>(result);
        }

        #endregion

        #region 移仓任务

        /// <summary>
        /// 移仓完成
        /// </summary>
        /// <param name="from"></param>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public string DoStockMoveFinishTask(string from, string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockMoveTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskFinish + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        #endregion

        #region 盘点任务

        /// <summary>
        /// 请求对应位置的盘点任务
        /// </summary>
        /// <param name="from"></param>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public string DoRequestStockCheckTask(string from)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockCheckTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.StockCheckTask + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        /// <summary>
        /// 盘点任务完成
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string DoStockCheckFinishTask(string from,string taskuid)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockCheckTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskFinish + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }


        /// <summary>
        /// 盘点任务完成
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string DoStockCheckErrorTask(string from)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockCheckTask + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.TaskFinish + ",");
            url.Append(WmsParam.From + "=" + from + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        #endregion

        #region 取消&异常

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string DoCancelTask(WmsStatus ws, string taskuid)
        {
            string type;
            switch (ws)
            {
                case WmsStatus.StockInTask:
                    type = WmsParam.GetAllStockInTask;
                    break;
                case WmsStatus.StockOutTask:
                    type = WmsParam.GetAllStockOutTask;
                    break;
                case WmsStatus.StockMoveTask:
                    type = WmsParam.GetAllStockMoveTask;
                    break;
                case WmsStatus.StockCheckTask:
                    type = WmsParam.GetAllStockCheckTask;
                    break;
                default:
                    type = WmsParam.GetAllUnFinishTask;
                    break;
            }
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(type + ",");
            url.Append(WmsParam.Status + "=" + (int)WmsStatus.Cancel + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }

        /// <summary>
        /// 反馈异常
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string DoShowError(WmsStatus ws, string taskuid, int errNo, string msg)
        {
            string type;
            switch (ws)
            {
                case WmsStatus.StockInTask:
                    type = WmsParam.GetAllStockInTask;
                    break;
                case WmsStatus.StockOutTask:
                    type = WmsParam.GetAllStockOutTask;
                    break;
                case WmsStatus.StockMoveTask:
                    type = WmsParam.GetAllStockMoveTask;
                    break;
                case WmsStatus.StockCheckTask:
                    type = WmsParam.GetAllStockCheckTask;
                    break;
                default:
                    type = WmsParam.GetAllUnFinishTask;
                    break;
            }
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(type + ",");
            url.Append(WmsParam.TaskUID + "=" + taskuid + ",");
            url.Append(WmsParam.ErrorCode + "=" + errNo + ",");
            url.Append(WmsParam.TextMessage + "=" + msg + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString());
            return result;
        }
        #endregion

        #region 库位坐标

        /// <summary>
        /// 申请货位坐标
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public WmsLocation DoLocationData(int version, int lot, string area)
        {
            StringBuilder url = new StringBuilder();
            url.Append(wcsUrl + "/" + serverName + "/");
            url.Append(WmsParam.GetAllStockLocation + ",");
            url.Append(WmsParam.DataVersion + "=" + version + ",");
            url.Append(WmsParam.DataLot + "=" + lot + ",");
            url.Append(WmsParam.From + "=" + area + ",");
            url.Append(WmsParam.DateTime + "=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ",");
            url.Append(CommandEnd);

            Console.WriteLine(url.ToString());
            string result = DoPost(url.ToString(), WmsParam.GetAllStockLocation);
            return JsonConvert.DeserializeObject<WmsLocation>(result);
        }

        #endregion

        #region 网络请求逻辑

        /// <summary>
        /// 网络请求并返回结果
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public string DoPost(string url, string key = null)
        {
            try
            {
                //定义request并设置request的路径
                WebRequest request = WebRequest.Create(url);

                //定义请求的方式
                request.Method = "POST";


                //定义response为前面的request响应
                WebResponse response = request.GetResponse();

                //获取相应的状态代码
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                //定义response字符流
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();//读取所有

                if (string.IsNullOrEmpty(key) && key != WmsParam.GetAllStockLocation)
                {
                    Console.WriteLine(responseFromServer);
                }

                //关闭资源
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "";
        }


        #endregion

    }
}
