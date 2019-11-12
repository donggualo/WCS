using ModuleManager.NDC;
using ModuleManager.NDC.SQL;
using NDC8.ACINET.ACI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NdcManager
{
    /// <summary>
    /// 处理NDC消息更新类
    /// </summary>
    public abstract class NDCDataHelper : NDCBase
    {


        #region [参数/构造函数]

        /// <summary>
        /// 运行开关
        /// </summary>
        internal bool ControlRunning = true;

        /// <summary>
        /// 装货点和卸货点NDC对应信息
        /// </summary>
        internal Dictionary<string, string> loadStaDic, unLoadStaDic;

        private readonly List<NDCItem> _items = new List<NDCItem>();
        internal List<NDCItem> _initItems = new List<NDCItem>();
        private List<TempItem> tempList = new List<TempItem>();

        private List<int> redirectItemList;
        private List<int> loadItemList;
        private List<int> unloadItemList;
        private int CarWashIndex = 9999;

        /// <summary>
        /// 保存所有任务
        /// </summary>
        internal List<NDCItem> Items
        {
            get
            {
                lock (_items)
                {
                    return _items;
                }
            }
        }

        /// <summary>
        /// 临时任务列表
        /// </summary>
        internal List<TempItem> TempList
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
        /// 重定向任务ID
        /// </summary>
        internal List<int> ReDirectList
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
        internal List<int> LoadItemList
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
        internal List<int> UnLoadItemList
        {
            get
            {
                lock (unloadItemList)
                {
                    return unloadItemList;
                }
            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public NDCDataHelper() : base()
        {

            redirectItemList = new List<int>();
            loadItemList = new List<int>();
            unloadItemList = new List<int>();

            DoReadSQL();

            new Thread(CheckItemTask)
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// 关闭时需要的操作
        /// </summary>
        public void DoCloseNDCDataHelper()
        {
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
                        if (i.INDEX == 0)
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
                                        || c._mTask.INDEX == i.INDEX;
                            });
                            if (item != null)
                            {
                                item._mTask = i;
                                item.CarrierId = i.CARRIERID != 0 ? i.CARRIERID : item.CarrierId;
                            }
                            else
                            {
                                NDCItem it = new NDCItem()
                                {
                                    _mTask = i
                                };
                                it.CarrierId = i.CARRIERID;
                                Items.Add(it);
                                _NoticeUpdate(it);
                            }
                        }

                    }

                    control.DelectUnFinishTaskAfter();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region [作业线程：检查是否需要重定向，开始卸货/装货]

        /// <summary>
        /// 检查任务/
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
            }
            catch (Exception e)
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
                NDCItem item = Items.Find(c => { return c._mTask.INDEX == index; });
                if (item != null && item.CanDirect())
                {
                    DoRedirect(item._mTask.INDEX, item._mTask.NDCREDIRECTSITE);
                }else if(item != null && item.Magic == 32 && item._mTask.REDIRECTSITE == "")
                {
                    _NoticeRedirect(item);
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
                NDCItem item = Items.Find(c => { return c._mTask.INDEX == index; });
                if (item != null && item.CanLoadPlc())
                {
                    DoLoad(item._mTask.INDEX, item.CarrierId);
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
                NDCItem item = Items.Find(c => { return c._mTask.INDEX == index; });
                if (item != null && item.CanUnLoadPlc())
                {
                    DoUnLoad(item._mTask.INDEX, item.CarrierId);
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
                _NoticeDelete(i);
            }
        }

        /// <summary>
        /// 清除空信息
        /// </summary>
        private void ClearEmptyItem()
        {
            List<NDCItem> items = Items.FindAll(c => { return c._mTask.IKEY == 0 && c._mTask.TASKID == 0 && c._mTask.INDEX == 0; });
            foreach (var i in items)
            {
                Items.Remove(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshInitGrid()
        {
            if (_initItems.Count == 0) return;
            lock (_initItems)
            {
                foreach (var i in _initItems)
                {
                    _NoticeUpdate(i);
                }
            }
        }
        #endregion


        #region NDCITEM

        /// <summary>
        /// 更新S消息
        /// </summary>
        /// <param name="message"></param>
        internal override void UpdateItem(Message_s message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c._mTask.INDEX == message.Index; });
                if (ndcItem == null && message.Magic != 32)//32  Carwash sends this request
                {
                    ndcItem = new NDCItem
                    {
                        CarrierId = message.CarrierNumber
                    };
                    ndcItem._mTask.INDEX = message.Index;
                    Items.Add(ndcItem);
                }else if(ndcItem == null && message.Magic == 32)//32  Carwash sends this request
                {
                    ndcItem = new NDCItem
                    {
                        CarrierId = message.Magic2
                    };
                    ndcItem._mTask.INDEX = message.Index;
                    ndcItem._mTask.IKEY = CarWashIndex;
                    ndcItem._mTask.TASKID = CarWashIndex;
                    CarWashIndex--;
                    Items.Add(ndcItem);
                }
                ndcItem.SetSMessage(message);
                if (ndcItem.StatusInfo != "")
                {
                    Console.WriteLine(ndcItem.StatusInfo);
                    log.LOG(ndcItem.StatusInfo);
                }
                CheckMagic(ndcItem, message);

                _NoticeUpdate(ndcItem);

                ///通知并更新WCS
                //if(ndcItem.TaskID !=0 ) DataControl._mForAGVControl.SubmitAgvMagic(ndcItem.TaskID, ndcItem.CarrierId+"", ndcItem.Magic);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                log.LOG(e.Message);
            }

        }

        /// <summary>
        /// 更新B消息
        /// </summary>
        /// <param name="message"></param>
        internal override void UpdateItem(Message_b message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c._mTask.INDEX == message.Index; });
                if (ndcItem == null)
                {
                    ndcItem = new NDCItem();
                    ndcItem._mTask.IKEY = message.IKEY;
                    ndcItem._mTask.INDEX = message.Index;
                    Items.Add(ndcItem);
                }
                ndcItem.SetBMessage(message);
                if (ndcItem.TaskInfo != "")
                {
                    Console.WriteLine(ndcItem.TaskInfo);
                    log.LOG(ndcItem.TaskInfo);
                }
                CheckStatus(ndcItem, message);

                _NoticeUpdate(ndcItem);
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
        internal override void UpdateItem(Message_vpil message)
        {
            try
            {
                NDCItem ndcItem = Items.Find(c => { return c.CarrierId == message.CarId; });
                if (ndcItem != null)
                {
                    ndcItem.SetVMessage(message);
                    CheckPlc(ndcItem, message);
                }
            }
            catch (Exception e)
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
            int index = item._mTask.INDEX;
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
                    if (!ReDirectList.Contains(item._mTask.INDEX))
                    {
                        ReDirectList.Add(item._mTask.INDEX);
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

                    if (!ReDirectList.Contains(item._mTask.INDEX))
                    {
                        ReDirectList.Add(item._mTask.INDEX);
                    }

                    //装货完成
                    item.PLCStatus = NDCPlcStatus.Loaded;
                    item._mTask.HADLOAD = true;

                    if (item.DirectStatus == NDCItemStatus.NeedRedirect)
                    {
                        _NoticeRedirect(item);
                    }
                    break;

                case 254://重新定位成功
                    item.DirectStatus = NDCItemStatus.Redirected;
                    ReDirectList.Remove(item._mTask.INDEX);
                    break;

                case 9://到达卸货点

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
                case 32://小车重启任务
                    //如果车有站点信息 则直接重定向
                    if(item._mTask.NDCREDIRECTSITE != null && item._mTask.NDCREDIRECTSITE != "")
                    {
                        item.HadDirectInfo = true;
                        item.DirectStatus = NDCItemStatus.HasDirectInfo;
                        if (!ReDirectList.Contains(item._mTask.INDEX))
                        {
                            ReDirectList.Add(item._mTask.INDEX);
                        }
                    }
                    else
                    {
                        _NoticeRedirect(item);
                    }
                    break;
                default:
                    break;
            }
            MatchTempInfo(item);
        }

        /// <summary>
        /// 检查AGV 棍台消息任务
        /// </summary>
        /// <param name="item"></param>
        /// <param name="v"></param>
        private void CheckPlc(NDCItem item, Message_vpil v)
        {
            Console.WriteLine("PLC:" + v.PlcLp1 + " Value:" + v.Value1);
            if (v.PlcLp1 == 29 && v.Value1 == 1)
            {
                //装货中
                item.PLCStatus = NDCPlcStatus.Loading;
                LoadItemList.Remove(item._mTask.INDEX);
                //通知WCS
                _NoticeWcsLoading(item._mTask.TASKID, item.CarrierId + "");
            }
            else if (v.PlcLp1 == 29 && v.Value1 == 2)
            {
                //卸货中
                item.PLCStatus = NDCPlcStatus.Unloading;
                UnLoadItemList.Remove(item._mTask.INDEX);
            }
        }

        /// <summary>
        /// 绑定临时任务和最终任务信息
        /// </summary>
        /// <param name="item"></param>
        private void MatchTempInfo(NDCItem item)
        {
            try
            {
                if (item._mTask.TASKID != 0)
                {
                    try
                    {
                        _NoticeWcsMagic(item._mTask.TASKID, item.CarrierId + "", item.Magic);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        #endregion


        #region 抽象方法

        internal abstract void _NoticeDelete(NDCItem model);

        internal abstract void _NoticeUpdate(NDCItem model);

        internal abstract void _NoticeRedirect(NDCItem model);

        internal abstract void _NoticeWcsLoading(int taskid, string agvid);

        internal abstract void _NoticeWcsMagic(int id, string agv, int magic);

        #endregion
    }
}
