using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NDC8.ACINET.ACI;
using ToolManager;
using ModuleManager.NDC;
using ModuleManager.NDC.SQL;

namespace NdcManager
{
    public class NDCControl : NDCBase
    {
        #region 参数
        /// <summary>
        /// 运行开关
        /// </summary>
        private bool ControlRunning = true;

        private List<NDCItem> _items = new List<NDCItem>();
        private List<NDCItem> _initItems = new List<NDCItem>();
        /// <summary>
        /// 保存所有任务
        /// </summary>
        private List<NDCItem> Items
        {
            get
            {
                lock (_items)
                {
                    return _items;
                }
            }
        }

        private List<TempItem> tempList = new List<TempItem>();
        /// <summary>
        /// 临时任务列表
        /// </summary>
        private List<TempItem> TempList
        {
            get
            {
                lock (tempList)
                {
                    return tempList;
                }
            }
        }

        /// <summary>
        /// 装货点和卸货点NDC对应信息
        /// </summary>
        Dictionary<string, string> loadStaDic, unLoadStaDic;



        /// <summary>
        /// 检查任务状态线程
        /// </summary>
        Thread itemCheckThread;

        /// <summary>
        /// 重定向任务ID
        /// </summary>
        List<int> redirectItemList;

        /// <summary>
        /// 重定向任务ID
        /// </summary>
        private List<int> ReDirectList
        {
            get
            {
                lock (redirectItemList)
                {
                    return redirectItemList;
                }
            }
        }

        /// <summary>
        /// 装货任务ID列表
        /// </summary>
        List<int> loadItemList;

        /// <summary>
        /// 装货任务ID列表
        /// </summary>
        private List<int> LoadItemList
        {
            get
            {
                lock (loadItemList)
                {
                    return loadItemList;
                }
            }
        }

        /// <summary>
        /// 卸货任务ID列表
        /// </summary>
        List<int> unloadItemList;

        /// <summary>
        /// 卸货任务ID列表
        /// </summary>
        private List<int> UnLoadItemList
        {
            get
            {
                lock (unloadItemList)
                {
                    return unloadItemList;
                }
            }
        }
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造方法
        /// </summary>
        public NDCControl():base()
        {
            DoReadSQL();

            redirectItemList = new List<int>();
            loadItemList = new List<int>();
            unloadItemList = new List<int>();

            itemCheckThread = new Thread(CheckItemTask)
            {
                IsBackground = true
            };

            itemCheckThread.Start();
        }

        #endregion

        /// <summary>
        /// 关闭任务前保存数据
        /// </summary>
        public void Close()
        {
            DoDisConnectNDC();

            ControlRunning = false;

            if (Ikey >= 99) Ikey = 1;
            NDCSQLControl control = new NDCSQLControl();
            control.UpdateIkeyValue(Ikey);
            control.SaveUnFinishTask(Items, TempList);
        }

        /// <summary>
        /// 重新读取装货点，卸货点对应NDC实际使用信息
        /// </summary>
        public void DoReadSQL()
        {
            try
            {
                NDCSQLControl control = new NDCSQLControl();
                control.ReadWcsNdcSite(out loadStaDic, out unLoadStaDic);

                control.ReadNDCServerAndIKEY(out IPaddress, out Port, out Ikey);

                if (control.ReadUnFinishTask(out List<WCS_NDC_TASK> list))
                {
                    foreach (var i in list)
                    {
                        if (i.ORDERINDEX == 0)
                        {
                            TempItem item = TempList.Find(c => { return c.TaskID == i.TASKID || c.IKey == i.IKEY; });
                            if (item == null)
                            {
                                TempList.Add(new TempItem()
                                {
                                    TaskID = i.TASKID,
                                    IKey = i.IKEY,
                                    LoadSite = i.LOADSITE,
                                    UnloadSite = i.UNLOADSITE,
                                    RedirectSite = i.REDIRECTSITE,
                                    NdcLoadSite = i.NDCLOADSITE,
                                    NdcUnloadSite = i.NDCUNLOADSITE,
                                    NdcRedirectSite = i.NDCREDIRECTSITE
                                });
                            }
                        }
                        else
                        {
                            NDCItem item = Items.Find(c =>
                            {
                                return c._mTask.IKEY == i.IKEY || c._mTask.TASKID == i.TASKID
                                        || c._mTask.ORDERINDEX == i.ORDERINDEX;
                            });
                            if (item != null)
                            {
                                item._mTask = i;
                                item.CarrierId = i.CARRIERID!=0 ? i.CARRIERID:item.CarrierId;
                            }
                            else
                            {
                                NDCItem it = new NDCItem()
                                {
                                    _mTask = i
                                };
                                it.CarrierId = i.CARRIERID;
                                Items.Add(it);
                                CheckCanUpdateTaskList(it);
                            }
                        }


                    }

                    control.DelectUnFinishTaskAfter();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region NDCITEM

        /// <summary>
        /// 更新S消息
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateItem(Message_s message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c._mTask.ORDERINDEX == message.Index; });
                if (ndcItem == null)
                {
                    ndcItem = new NDCItem
                    {
                        CarrierId = message.CarrierNumber
                    };
                    ndcItem._mTask.ORDERINDEX = message.Index;
                    Items.Add(ndcItem);
                }
                ndcItem.SetSMessage(message);
                if (ndcItem.StatusInfo != "")
                {
                    Console.WriteLine(ndcItem.StatusInfo);
                    log.LOG(ndcItem.StatusInfo);
                }
                CheckMagic(ndcItem, message);

                CheckCanUpdateTaskList(ndcItem);

                ///通知并更新WCS
                //if(ndcItem.TaskID !=0 ) DataControl._mForAGVControl.SubmitAgvMagic(ndcItem.TaskID, ndcItem.CarrierId+"", ndcItem.Magic);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                log.LOG(e.Message);
            }

        }

        /// <summary>
        /// 更新B消息
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateItem(Message_b message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c._mTask.ORDERINDEX == message.Index; });
                if (ndcItem == null)
                {
                    ndcItem = new NDCItem();
                    ndcItem._mTask.IKEY = message.IKEY;
                    ndcItem._mTask.ORDERINDEX = message.Index;
                    Items.Add(ndcItem);
                }
                ndcItem.SetBMessage(message);
                if (ndcItem.TaskInfo != "")
                {
                    Console.WriteLine(ndcItem.TaskInfo);
                    log.LOG(ndcItem.TaskInfo);
                }
                CheckStatus(ndcItem, message);

                CheckCanUpdateTaskList(ndcItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                log.LOG(e.Message);
            }
        }

        /// <summary>
        /// 更新PLC结果信息
        /// </summary>
        /// <param name="message"></param>
        public override void UpdateItem(Message_vpil message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c.CarrierId == message.CarId; });
                if (ndcItem != null)
                {
                    ndcItem.SetVMessage(message);
                    CheckPlc(ndcItem, message);
                }
            }catch (Exception e)
            {
                Console.WriteLine(e.Message);
                log.LOG(e.Message);
            }
        }

        /// <summary>
        /// 检查任务状态
        /// </summary>
        /// <param name="status"></param>
        private void CheckStatus(NDCItem item, Message_b b)
        {
            switch (item.Status)
            {
                case 37: //小车已经分配
                    item.CarrierId = b.ParNo;
                    break;

                case 3://任务完成
                    item.IsFinish = true;
                    item.finishTime = DateTime.Now;
                    break;
                case 27://IKEY in use
                    TempItem temp = TempList.Find(c => { return c.IKey.Equals("" + item._mTask.IKEY); });
                    if (temp != null)
                    {
                        temp.IKey = Ikey++;
                        DoStartOrder(temp);
                    }
                    break;
            }
        }


        /// <summary>
        /// 检查任务进程
        /// </summary>
        /// <param name="index"></param>
        /// <param name="magic"></param>
        private void CheckMagic(NDCItem item, Message_s s)
        {
            int index = item._mTask.ORDERINDEX;
            switch (item.Magic)
            {
                case 2://确定生成任务
                    item._mTask.NDCLOADSITE = item.s.Magic2 + "";
                    item._mTask.NDCUNLOADSITE = item.s.Magic3 + "";
                    break;

                case 5: //小车到达接货点
                    if (item.DirectStatus != NDCItemStatus.HasDirectInfo)
                    {
                        item.DirectStatus = NDCItemStatus.CanRedirect;
                    }
                    if (!ReDirectList.Contains(item._mTask.ORDERINDEX))
                    {
                        ReDirectList.Add(item._mTask.ORDERINDEX);
                    }
                    //TODO 告诉WCS 车已经到达

                    //准备好装货了
                    item.PLCStatus = NDCPlcStatus.LoadReady;
                    break;
                case 6: //小车接货完成
                    if (item.DirectStatus == NDCItemStatus.CanRedirect || item.DirectStatus == NDCItemStatus.Init)
                    {
                        if (item.HadDirectInfo)
                        {
                            item.DirectStatus = NDCItemStatus.HasDirectInfo;
                        }
                        else
                        {
                            item.DirectStatus = NDCItemStatus.NeedRedirect;
                        }
                    }

                    if (!ReDirectList.Contains(item._mTask.ORDERINDEX))
                    {
                        ReDirectList.Add(item._mTask.ORDERINDEX);
                    }

                    //装货完成
                    item.PLCStatus = NDCPlcStatus.Loaded;
                    item._mTask.HADLOAD = true;

                    if(item.DirectStatus == NDCItemStatus.NeedRedirect)
                    {
                        NoticeRedirect?.Invoke(item);
                    }
                    break;

                case 254://重新定位成功
                    item.DirectStatus = NDCItemStatus.Redirected;
                    ReDirectList.Remove(item._mTask.ORDERINDEX);
                    break;

                case 8://到达卸货点

                    //准备好卸货了
                    item.PLCStatus = NDCPlcStatus.UnloadReady;
                    break;

                case 10://卸货完成

                    //卸货完成
                    item.PLCStatus = NDCPlcStatus.Unloaded;
                    item._mTask.HADUNLOAD = true;
                    break;
                case 11://任务完成
                    item.IsFinish = true;
                    item.finishTime = DateTime.Now;
                    break;
                default:
                    break;
            }
            GetTempInfo(item);
        }


        private void CheckPlc(NDCItem item, Message_vpil v)
        {
            Console.WriteLine("PLC:" + v.PlcLp1 + " Value:" + v.Value1);
            if (v.PlcLp1 == 29 && v.Value1 == 1)
            {
                //装货中
                item.PLCStatus = NDCPlcStatus.Loading;
                LoadItemList.Remove(item._mTask.ORDERINDEX);
                //通知WCS
                AGVDataUpdate?.Invoke(item._mTask.TASKID, item.CarrierId + "");
            }
            else if (v.PlcLp1 == 29 && v.Value1 == 2)
            {
                //卸货中
                item.PLCStatus = NDCPlcStatus.Unloading;
                UnLoadItemList.Remove(item._mTask.ORDERINDEX);
            }

            /*
            //switch (v.PlcLp1)
            //{
            //    case 1://装货中
            //        item.PLCStatus = NDCPlcStatus.Loading;
            //        LoadItemList.Remove(item.OrderIndex);
            //        DataControl._mForAGVControl.SubmitAgvLoading(item.TaskID, item.CarrierId + "");
            //        break;


            //    case 2://卸货中
            //        item.PLCStatus = NDCPlcStatus.Unloading;
            //        UnLoadItemList.Remove(item.OrderIndex);
            //        break;
            //}
            */
        }


        /// <summary>
        /// 绑定临时任务和最终任务信息
        /// </summary>
        /// <param name="item"></param>
        private void GetTempInfo(NDCItem item)
        {
            try
            {
                if (item._mTask.TASKID != 0)
                {
                    try
                    {
                        AGVMagicUpdate?.Invoke(item._mTask.TASKID, item.CarrierId + "", item.Magic);
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (item._mTask.NDCLOADSITE == null) return;
                TempItem tempItem = TempList.Find(c => { return c.NdcLoadSite == item._mTask.NDCLOADSITE; });
                if (tempItem != null)
                {
                    item._mTask.TASKID = tempItem.TaskID;
                    item._mTask.LOADSITE = tempItem.LoadSite;
                    item._mTask.UNLOADSITE = tempItem.UnloadSite;
                    item._mTask.REDIRECTSITE = tempItem.RedirectSite;
                    item._mTask.NDCREDIRECTSITE = tempItem.NdcRedirectSite;
                    TempList.Remove(tempItem);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        #endregion

        #region 线程操作

        /// <summary>
        /// 检查任务
        /// </summary>
        private void CheckItemTask()
        {
            try
            {
                while (ControlRunning)
                {
                    Thread.Sleep(3000);

                    CheckReDirect();

                    CheckLoadPlcStatus();

                    CheckUnLoadPlcStatus();

                    ClearFinishItem();

                    ClearEmptyItem();

                    RefreshInitGrid();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// //重定向任务
        /// </summary>
        private void CheckReDirect()
        {
            foreach (int index in ReDirectList)
            {
                NDCItem item = Items.Find(c => { return c._mTask.ORDERINDEX == index; });
                if (item != null && item.CanDirect())
                {
                    DoRedirect(item._mTask.ORDERINDEX, item._mTask.NDCREDIRECTSITE);
                }
            }
        }

        /// <summary>
        /// 检查是否需要卸货
        /// </summary>
        private void CheckLoadPlcStatus()
        {
            foreach (int index in LoadItemList)
            {
                NDCItem item = Items.Find(c => { return c._mTask.ORDERINDEX == index; });
                if (item != null && item.CanLoadPlc())
                {
                    DoLoad(item._mTask.ORDERINDEX, item.CarrierId);
                }
            }
        }

        /// <summary>
        /// 检查是否需要装货
        /// </summary>
        private void CheckUnLoadPlcStatus()
        {
            foreach (int index in UnLoadItemList)
            {
                NDCItem item = Items.Find(c => { return c._mTask.ORDERINDEX == index; });
                if (item != null && item.CanUnLoadPlc())
                {
                    DoUnLoad(item._mTask.ORDERINDEX, item.CarrierId);
                }
            }
        }

        /// <summary>
        /// 清除完成的任务信息
        /// </summary>
        private void ClearFinishItem()
        {
            List<NDCItem> items = Items.FindAll(c => { return c.IsFinish && c.CanDeleteFinish(); });
            foreach (var i in items)
            {
                Items.Remove(i);
                TaskGridDelete?.Invoke(i);
            }
        }

        /// <summary>
        /// 清除空信息
        /// </summary>
        private void ClearEmptyItem()
        {
            List<NDCItem> items = Items.FindAll(c => { return c._mTask.IKEY == 0 && c._mTask.TASKID ==0 && c._mTask.ORDERINDEX ==0; });
            foreach (var i in items)
            {
                Items.Remove(i);
            }
        }

        private void RefreshInitGrid()
        {
            if (_initItems.Count == 0) return;
            lock (_initItems)
            {
                foreach(var i in _initItems)
                {
                    CheckCanUpdateTaskList(i);
                }
            }
        }

        #endregion

        #region 对外方法

        /// <summary>
        /// 添加接货任务
        /// </summary>
        /// <param name="taskid">任务ID </param>
        /// <param name="loadstation">装货区域</param>
        /// <param name="unloadstation">卸货区域</param>
        /// <param name="result">失败原因</param>
        /// <returns></returns>
        public bool AddNDCTask(int taskid, string loadstation, string unloadstation, out string result)
        {
            if (!VCP9412.Instance.IsConnected)
            {
                result = "NDC服务未连接";
                return false;
            }

            if (!loadStaDic.TryGetValue(loadstation, out string ndcLoadsta))
            {
                result = "装货点未配置";
                return false;
            }

            if (!unLoadStaDic.TryGetValue(unloadstation, out string ndcUnloadsta))
            {
                result = "卸货点未配置";
                return false;
            }

            if (Items.Find(c => { return c._mTask.TASKID == taskid; }) != null)
            {
                result = "找到相同任务ID(" + taskid + ")任务，不能再次添加";
                return false;
            }

            if (TempList.Find(c => { return c.TaskID == taskid; }) != null)
            {
                result = "找到相同任务ID(" + taskid + ")任务，不能再次添加";
                return false;
            }


            TempItem item = new TempItem
            {
                Prio = "1",
                IKey = Ikey++,
                TaskID = taskid,
                LoadSite = loadstation,
                UnloadSite = unloadstation,
                NdcLoadSite = ndcLoadsta,
                NdcUnloadSite = ndcUnloadsta
            };
            TempList.Add(item);
            if (Ikey >= 99) Ikey = 1;
            DoStartOrder(item);

            //TaskListUpdate(new NdcTaskModel(item));//更新界面数据

            result = "";
            return true;
        }

        /// <summary>
        /// 根据任务ID,重新定位卸货地点
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="unloadstation"></param>
        /// <returns></returns>
        public bool DoReDerect(int taskid, string unloadstation, out string result,int order = -1)
        {
            NDCItem item = Items.Find(c =>
            {
                return c._mTask.TASKID == taskid && (order == -1 || c._mTask.ORDERINDEX == order);
            });

            if (item == null)
            {
                TempItem temp = TempList.Find(c => { return c.TaskID == taskid; });
                if (temp != null)
                {
                    if(unLoadStaDic.TryGetValue(unloadstation, out string ndcUnloadSta))
                    {
                        temp.RedirectSite = unloadstation;
                        temp.NdcRedirectSite = ndcUnloadSta;
                    }
                    else
                    {
                        result = "该区域的对应关系还没有";
                        return false;
                    }
                }
                else
                {
                    result = "并未找到任务ID为：" + taskid + "的任务";
                    return false;
                }
            }
            else
            {
                if (item.DirectStatus == NDCItemStatus.Redirected)
                {
                    result = "任务已经重定位了";
                    return false;
                }
                else
                {
                    if (unLoadStaDic.TryGetValue(unloadstation, out string ndcUnloadSta))
                    {
                        item._mTask.NDCREDIRECTSITE = ndcUnloadSta;
                        item._mTask.REDIRECTSITE = unloadstation;
                        if (item.DirectStatus == NDCItemStatus.NeedRedirect)
                        {
                            item.DirectStatus = NDCItemStatus.HasDirectInfo;
                        }
                        else if (item.DirectStatus == NDCItemStatus.CanRedirect || item.DirectStatus == NDCItemStatus.Init)
                        {
                            item.HadDirectInfo = true;
                        }
                    }
                    else
                    {
                        result = "该区域的对应关系还没有";
                        return false;
                    }
                }
                CheckCanUpdateTaskList(item);
            }

            result = "";
            return true;
        }

        /// <summary>
        /// 根据任务ID,进行装货操作
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="carid"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool DoLoad(int taskid, int carid, out string result)
        {
            if (taskid == 0 || carid == 0)
            {
                result = "任务ID,车ID不能为零";
                return false;
            }

            NDCItem item = Items.Find(c =>
            {
                return c._mTask.TASKID == taskid && c.CarrierId == carid;
            });

            if(item == null)
            {
                result = "找不到任务ID:" + taskid + ",小车:" + carid + "的任务.";
                return false;
            }


            if(item.PLCStatus == NDCPlcStatus.Loading)
            {
                LoadItemList.Remove(item._mTask.ORDERINDEX);
                //通知WCS
                AGVDataUpdate?.Invoke(item._mTask.TASKID, item.CarrierId + "");
                result = "小车已经启动辊台了";
                return true;
            }

            if (item.PLCStatus != NDCPlcStatus.LoadReady)
            {
                result = "小车未准备好接货";
                return false;
            }

            if (!LoadItemList.Contains(item._mTask.ORDERINDEX))
            {
                LoadItemList.Add(item._mTask.ORDERINDEX);
                result = "";
                return true;
            }

            result = taskid + "的装货已经请求过了";
            return false;
        }

        /// <summary>
        /// 根据任务ID,进行卸货操作
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="carid"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool DoUnLoad(int taskid, int carid, out string result)
        {
            if (taskid == 0 || carid == 0)
            {
                result = "任务ID,车ID不能为零";
                return false;
            }

            NDCItem item = Items.Find(c =>
            {
                return c._mTask.TASKID == taskid && c.CarrierId == carid;
            });

            if (item == null)
            {
                result = "找不到任务ID:" + taskid + ",小车:" + carid + "的任务.";
                return false;
            }

            if (item.PLCStatus != NDCPlcStatus.UnloadReady)
            {
                result = "小车未准备好卸货";
                return false;
            }

            if (!UnLoadItemList.Contains(item._mTask.ORDERINDEX))
            {
                UnLoadItemList.Add(item._mTask.ORDERINDEX);
                result = "";
                return true;
            }

            result = taskid + "的卸货请求已经请求过了";
            return false;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="index"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool DoCancelIndex(int index,out string result)
        {
            if (index == 0 || index == 0)
            {
                result = "任务ID,车ID不能为零";
                return false;
            }

            NDCItem item = Items.Find(c =>
            {
                return c._mTask.ORDERINDEX == index;
            });

            if (item == null)
            {
                result = "找不到任务Index:"+index+"任务.";
                return false;
            }

            DoDeleteOrder(index + "");
            result = "取消成功";
            return true;
        }

        #endregion


        #region 对外接口

        /// <summary>
        /// 句柄更新类型
        /// </summary>
        /// <param name="model"></param>
        public delegate void GridDataHandler(NDCItem model);

        /// <summary>
        /// 任务Grid数据更新句柄
        /// </summary>
        public event GridDataHandler TaskGridUpdate;
        /// <summary>
        /// 任务Grid数据删除句柄
        /// </summary>
        public event GridDataHandler TaskGridDelete;
        /// <summary>
        /// 界面展示需要重定向句柄
        /// </summary>
        public event GridDataHandler NoticeRedirect;

        //通知任务车辆信息
        //装货状态通知
        public delegate void AGVDataHandler(int taskid,string agvid);
        /// <summary>
        /// AGV 装货中 通知句柄
        /// </summary>
        public event AGVDataHandler AGVDataUpdate;

        //AGV Magic状态通知
        public delegate void AGVMagicHandler(int id, string agv, int magic);

        /// <summary>
        /// 更新AGV当前任务状态
        /// </summary>
        public event AGVMagicHandler AGVMagicUpdate;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ndcItem"></param>
        private void CheckCanUpdateTaskList(NDCItem ndcItem)
        {
            if (ndcItem._mTask.IKEY != 0 && ndcItem._mTask.ORDERINDEX != 0 && ndcItem._mTask.TASKID != 0)
            {
                if (TaskGridUpdate != null)
                {
                    TaskGridUpdate(ndcItem);
                }
                else
                {
                    if (_initItems.Contains(ndcItem)) return;
                    _initItems.Add(ndcItem);
                }
                
            }
        }
    }
}
