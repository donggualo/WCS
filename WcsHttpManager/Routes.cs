using MHttpServer;
using MHttpServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WcsHttpManager
{
    public class Routes
    {
        public delegate bool WmsModelHandler(WmsModel model,out string result);
        public event WmsModelHandler WmsModelAdd;


        public List<Route> GET
        {
            get
            {
                return new List<Route>()
                {
                    /*
                     * urlRegex 正则表达式
                     * 后面带问号+加任何字符  \?(.*)
                     */

                    //空链接.帮助界面
                    new Route()
                    {
                        Callable = HomeIndex,
                        UrlRegex = "^\\/$",
                        Method = "GET"
                    },
                    //处理：WMS向WCS发送的出库任务
                    new Route()
                    {
                        Callable = StockOutHandle,
                        UrlRegex = "^\\/StockOutHandle$",
                        Method = "GET"
                    }
                    //处理：WMS向WCS发送的移库任务
                    ,new Route()
                    {
                        Callable = StockMoveHandle,
                        UrlRegex = "^\\/StockMoveHandle$",
                        Method = "GET"
                    }
                    //处理：WMS向WCS发送的盘点任务
                    ,new Route()
                    {
                        Callable = StockCheckHandle,
                        UrlRegex = "^\\/StockCheckHandle$",
                        Method = "GET"
                    }
                };

            }
        }

        /// <summary>
        /// 出库任务
        /// 处理：WMS向WCS发送的出库任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpResponse StockOutHandle(HttpRequest request)
        {
            try { 
                if (request.Content != null)
                {
                    WmsModel model = JsonConvert.DeserializeObject<WmsModel>(request.Content);
                    HttpResponse response = CheckWmsModel(model,WmsStatus.StockOutTask,true);
                    if (response != null) return response;
                    if (WmsModelAdd(model, out string result))
                    {
                        return FailResponse(result);
                    }
                    return OkResponse();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return EmptyMssage();
        }

        /// <summary>
        /// 移库任务
        /// 处理：WMS向WCS发送的移库任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpResponse StockMoveHandle(HttpRequest request)
        {
            try
            {
                if (request.Content != null)
                {
                    WmsModel model = JsonConvert.DeserializeObject<WmsModel>(request.Content);
                    HttpResponse response = CheckWmsModel(model,WmsStatus.StockMoveTask,true);
                    if (response != null) return response;
                    if (WmsModelAdd(model, out string result))
                    {
                        return FailResponse(result);
                    }
                    return OkResponse();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return EmptyMssage();

        }

        /// <summary>
        /// 盘点任务
        /// 处理：WMS向WCS发送的盘点任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpResponse StockCheckHandle(HttpRequest request)
        {
            try
            {
                if (request.Content != null)
                {
                    WmsModel model = JsonConvert.DeserializeObject<WmsModel>(request.Content);
                    HttpResponse response = CheckWmsModel(model,WmsStatus.StockCheckTask,false);
                    if (response != null) return response;
                    if (WmsModelAdd(model, out string result))
                    {
                        return FailResponse(result);
                    }
                    return OkResponse();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return EmptyMssage();
        }

        /// <summary>
        /// 检查Wms请求过来的信息是否完整
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tasktype"></param>
        /// <param name="checkWdloc"></param>
        /// <returns></returns>
        private HttpResponse CheckWmsModel(WmsModel model,WmsStatus tasktype,bool checkWdloc)
        {
            string msg = "";

            //是否检查目标货位是否为空
            if (checkWdloc)
            {
                if (model.W_D_Loc == null || model.W_D_Loc.Length == 0)
                {
                    msg = "W_D_Loc can't be empty";
                }
            }

            //任务ID不能为空
            if (model.Task_UID == null || model.Task_UID.Length == 0)
            {
                msg = "Task_UID can't be empty";
            }
            //任务类型不能为空
            else if(model.Task_type == WmsStatus.Empty)
            {
                msg = "Task_type can't be empty";
            }
            //货位条形码不能为空
            else if(model.Barcode == null || model.Barcode.Length == 0)
            {
                msg = "Barcode can't be empty";
            }
            //源货位不能为空
            else if(model.W_S_Loc == null || model.W_S_Loc.Length == 0)
            {
                msg = "W_S_Loc can't be empty";
            }

            if (msg.Length != 0)
            {
                return new HttpResponse()
                {
                    ContentAsUTF8 = msg,
                    ReasonPhrase = "OK",
                    StatusCode = "200"
                };
            }

            return null;
        }

        private HttpResponse EmptyMssage()
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = "can't get any msg",
                ReasonPhrase = "OK",
                StatusCode = "200"
            };
        }

        private HttpResponse FailResponse(string msg = "Fail")
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = msg,
                ReasonPhrase = "OK",
                StatusCode = "200"
            };
        }

        private HttpResponse OkResponse(string msg = "OK")
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = msg,
                ReasonPhrase = "OK",
                StatusCode = "200"
            };
        }

        private HttpResponse HomeIndex(HttpRequest request)
        {
            return HttpBuilder.NotFound();
            //string value = "";
            //if (request.Headers.ContainsKey("WMS_DATA"))
            //{
            //    value = request.Headers["WMS_DATA"];
            //}

            //return new HttpResponse()
            //{
            //    ContentAsUTF8 = "{\"data\":\""+ value
            //        +"\",\"name\":\"kyle\",\"age\":18,\"friend\":[{\"name\":\"matt\",\"sex\":\"man\"},{\"name\":\"butt\",\"sex\":\"man\"}]}",
            //    ReasonPhrase = "OK",
            //    StatusCode = "200"
            //};
        }
    }
}
