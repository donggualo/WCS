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

        internal NDCSQLControl _sqlControl;


        /// <summary>
        /// 运行开关
        /// </summary>
        internal bool ControlRunning = true;

        /// <summary>
        /// 装货点和卸货点NDC对应信息
        /// </summary>
        internal Dictionary<string, string> loadStaDic, unLoadStaDic;

        private readonly List<NDCItem> _items = new List<NDCItem>();
        private readonly List<NDCItem> _tempItems = new List<NDCItem>();
        internal List<NDCItem> _initItems = new List<NDCItem>();

        private List<int> redirectItemList;
        private List<int> loadItemList;
        private List<int> unloadItemList;

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
        internal List<NDCItem> TempItems
        {
            get
            {
                lock (_tempItems)
                {
                    return _tempItems;
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

            _sqlControl = new NDCSQLControl();
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

            if (Ikey >= 60000) Ikey = 1;
            _sqlControl.UpdateIkeyValue(Ikey);

            foreach(NDCItem item in Items)
            {
                _sqlControl.UpdateNdcItem(item);
            }

            _sqlControl.DeleteTempItem();

            foreach(NDCItem temp in TempItems)
            {
                _sqlControl.InsertTempItem(temp);
            }
        }

        /// <summary>
        /// 重新读取装货点，卸货点对应NDC实际使用信息
        /// </summary>
        public void DoReadSQL()
        {
            try
            {
                _sqlControl.ReadWcsNdcSite(out loadStaDic, out unLoadStaDic);

                _sqlControl.ReadNDCServerAndIKEY(out IPaddress, out Port, out Ikey);

                if (_sqlControl.ReadUnFinishTask(out List<WCS_NDC_TASK> list))
                {
                    foreach (var i in list)
                    {
                        NDCItem item = Items.Find(c => { return c._mTask.TASKID == i.TASKID && c._mTask.IKEY == i.IKEY; });
                        if (item == null)
                        {
                            item = new NDCItem()
                            {
                                _mTask = i
                            };

                            Items.Add(item) ;

                            _NoticeUpdate(item);
                        }
                    }
                }

                if(_sqlControl.ReadTempTask(out List<WCS_NDC_TASK_TEMP> tlist))
                {
                    foreach(WCS_NDC_TASK_TEMP t in tlist)
                    {
                        NDCItem item = TempItems.Find(c =>
                        {
                            return c._mTask.IKEY == t.IKEY && c._mTask.NDCINDEX == t.NDCINDEX;
                        });
                        if (item != null)
                        {
                            item.CARRIERID = t.CARRIERID;
                        }
                        else
                        {
                            item = new NDCItem();
                            item.CARRIERID = t.CARRIERID;
                            item._mTask.NDCINDEX = t.NDCINDEX;
                            item._mTask.IKEY = t.IKEY;
                            TempItems.Add(item);
                        }
                    }
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
                NDCItem item = Items.Find(c => { return c._mTask.NDCINDEX == index; });
                if (item != null && item.CanDirect())
                {
                    DoRedirect(item._mTask.NDCINDEX, item._mTask.NDCREDIRECTSITE);
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
                NDCItem item = Items.Find(c => { return c._mTask.NDCINDEX == index; });
                if (item != null && item.CanLoadPlc())
                {
                    DoLoad(item._mTask.NDCINDEX, item.CARRIERID);
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
                NDCItem item = Items.Find(c => { return c._mTask.NDCINDEX == index; });
                if (item != null && item.CanUnLoadPlc())
                {
                    DoUnLoad(item._mTask.NDCINDEX, item.CARRIERID);
                }
            }
        }

        /// <summary>
        /// 清除完成的任务信息
        /// </summary>
        private void ClearFinishItem()
        {
            List<NDCItem> items = Items.FindAll(c => c.CanDeleteFinish());
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
            List<NDCItem> items = Items.FindAll(c => { return c._mTask.IKEY == 0 && c._mTask.TASKID == 0 && c._mTask.NDCINDEX == 0; });
            foreach (var i in items)
            {
                Items.Remove(i);
            }
        }

        /// <summary>
        /// 更新默认信息
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
                //找出被挂起的任务的重定向站点，重新重定向任务
                if (message.Magic == 32) //32  Carwash sends this request
                {//TODO Carwash 重定向
                    DoCarWashReDirect(message);
                    return;
                }
                NDCItem ndcItem = Items.Find(c => { return c._mTask.NDCINDEX == message.Index; });
                if (ndcItem == null)
                {
                    ndcItem = TempItems.Find(c => { return c._mTask.NDCINDEX == message.Index; });
                    
                    if (ndcItem != null)
                    {
                        ndcItem.CARRIERID = message.CarrierNumber;
                    }else
                    {
                        ndcItem = new NDCItem();
                        ndcItem.CARRIERID = message.CarrierNumber;
                        ndcItem._mTask.NDCINDEX = message.Index;
                        TempItems.Add(ndcItem);
                    }
                }
                ndcItem.SetSMessage(message);
                if (ndcItem.StatusInfo != "")
                {
                    //Console.WriteLine(ndcItem.StatusInfo);
                    log.LOG(ndcItem.StatusInfo);
                }
                CheckMagic(ndcItem, message);

                _NoticeUpdate(ndcItem);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
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
                NDCItem ndcItem = Items.Find(c => { return c._mTask.NDCINDEX == message.Index; });
                if (ndcItem == null)
                {
                    ndcItem = TempItems.Find(c => { return c._mTask.NDCINDEX == message.Index; });
                    if (ndcItem != null)
                    {
                        ndcItem._mTask.IKEY = message.IKEY;
                    }else
                    {
                        ndcItem = new NDCItem();
                        ndcItem._mTask.IKEY = message.IKEY;
                        ndcItem._mTask.NDCINDEX = message.Index;
                        TempItems.Add(ndcItem);
                    }
                }
                ndcItem.SetBMessage(message);
                if (ndcItem.TaskInfo != "")
                {
                    //Console.WriteLine(ndcItem.TaskInfo);
                    log.LOG(ndcItem.TaskInfo);
                }
                CheckStatus(ndcItem, message);

                _NoticeUpdate(ndcItem);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
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
                NDCItem ndcItem = Items.Find(c => { return c.CARRIERID == message.CarId; });
                if (ndcItem != null)
                {
                    ndcItem.SetVMessage(message);
                    CheckPlc(ndcItem, message);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
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
                    item.CARRIERID = b.ParNo;
                    break;

                case 3://任务完成
                    //
                    if(item.PLCStatus != NDCPlcStatus.Unloaded && item._mTask.PAUSE)
                    {
                        item.IsFinish = false;
                    }
                    else
                    {
                        item.IsFinish = true;
                        item.finishTime = DateTime.Now;
                    }
                    break;
                case 27://IKEY in use
                    item._mTask.IKEY = Ikey++;
                    DoStartOrder(item);
                    break;
                case 25://Cancel acknowledge
                    item.CancleFromSystem = true;
                    break;
            }
            if (item._mTask.TASKID != 0)
            {
                _sqlControl.UpdateNdcItem(item);
            }
        }

        /// <summary>
        /// 检查任务进程
        /// </summary>
        /// <param name="index"></param>
        /// <param name="magic"></param>
        private void CheckMagic(NDCItem item, Message_s s)
        {
            int index = item._mTask.NDCINDEX;
            switch (item.Magic)
            {
                case 2://确定生成任务
                    item._mTask.NDCLOADSITE = s.Magic2 + "";
                    item._mTask.NDCUNLOADSITE = s.Magic3 + "";
                    break;

                case 5:
                    #region 小车到达接货点
                    if (item.DirectStatus != NDCItemStatus.HasDirectInfo)
                    {
                        item.DirectStatus = NDCItemStatus.CanRedirect;
                    }
                    if (!ReDirectList.Contains(item._mTask.NDCINDEX))
                    {
                        ReDirectList.Add(item._mTask.NDCINDEX);
                    }

                    //准备好装货了
                    item.PLCStatus = NDCPlcStatus.LoadReady;
                    #endregion
                    break;

                case 6:
                    #region 小车接货完成
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

                    if (!ReDirectList.Contains(item._mTask.NDCINDEX))
                    {
                        ReDirectList.Add(item._mTask.NDCINDEX);
                    }

                    //装货完成
                    item.PLCStatus = NDCPlcStatus.Loaded;
                    item._mTask.HADLOAD = true;

                    if (item.DirectStatus == NDCItemStatus.NeedRedirect)
                    {
                        _NoticeRedirect(item);
                    }
                    #endregion
                    break;
                
                case 254://重新定位成功
                    item.DirectStatus = NDCItemStatus.Redirected;
                    ReDirectList.Remove(item._mTask.NDCINDEX);
                    break;

                case 9://到达卸货点

                    //准备好卸货了
                    item.PLCStatus = NDCPlcStatus.UnloadReady;
                    break;

                case 10://卸货完成

                    item.PLCStatus = NDCPlcStatus.Unloaded;
                    item._mTask.HADUNLOAD = true;
                    break;
                case 255://取消任务
                case 48://取消任务
                    if (!item.CancleFromSystem)//Carwash
                    { 
                        item._mTask.PAUSE = true;
                    }
                    break;

                case 11://任务完成
                    if(item.PLCStatus != NDCPlcStatus.Unloaded && item._mTask.PAUSE)
                    {
                        item.IsFinish = false;
                        //装货前 取消任务不用恢复，只需重新呼叫AGV
                        if (!item._mTask.HADLOAD && !item._mTask.HADUNLOAD)
                        {
                            item.BeforeReCall(Ikey++);
                            DoStartOrder(item);
                            log.LOG(item._mTask.TASKID+":任务车出现问题，现在重新呼叫AGV");
                        }
                    }
                    else
                    {
                        item.IsFinish = true;
                        item.finishTime = DateTime.Now;
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
            //Console.WriteLine("PLC:" + v.PlcLp1 + " Value:" + v.Value1);
            if (v.PlcLp1 == 29 && v.Value1 == 1)
            {
                //装货中
                item.PLCStatus = NDCPlcStatus.Loading;
                LoadItemList.Remove(item._mTask.NDCINDEX);
                //通知WCS
                //_NoticeWcsLoading(item._mTask.TASKID, item.CARRIERID + "");
            }
            else if (v.PlcLp1 == 29 && v.Value1 == 2)
            {
                //卸货中
                item.PLCStatus = NDCPlcStatus.Unloading;
                UnLoadItemList.Remove(item._mTask.NDCINDEX);
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
                if (item._mTask.TASKID == 0 && item._mTask.NDCINDEX != -1)
                {
                    NDCItem tempItem = TempItems.Find(c =>
                    {
                        return c._mTask.IKEY == item._mTask.IKEY
                                && c._mTask.NDCLOADSITE == item._mTask.NDCLOADSITE
                                && c._mTask.NDCUNLOADSITE == item._mTask.NDCUNLOADSITE;
                    });
                    if (tempItem != null)
                    {
                        NDCItem i = Items.Find(c => c._mTask.IKEY == tempItem._mTask.IKEY);
                        if (i != null)
                        {
                            i._mTask.NDCINDEX = tempItem._mTask.NDCINDEX;
                            i.CARRIERID = tempItem.CARRIERID;
                            i.DirectStatus = tempItem.DirectStatus;
                            i.Magic = tempItem.Magic;
                            i.Status = tempItem.Status;
                            TempItems.Remove(tempItem);
                            item = i;
                        }
                    }
                }

                if (item._mTask.TASKID != 0 && item._mTask.NDCINDEX != 0)
                {
                    try
                    {
                        if(item.PLCStatus !=  NDCPlcStatus.Unloaded && item._mTask.PAUSE
                            && item.Magic == 11)
                        {
                            //不做操作，挂起的任务异常结束就不通知WCS了
                        }
                        else
                        {
                            //_NoticeWcsMagic(item._mTask.TASKID, item.CARRIERID + "", item.Magic);
                        }
                        _sqlControl.UpdateNdcItem(item);
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                _sqlControl.UpdateNdcItem(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 小车Carwash需要重定向
        /// </summary>
        /// <param name="message"></param>
        private void DoCarWashReDirect(Message_s message)
        {
            int index = message.Index;
            int car = message.Magic2;
            NDCItem item = Items.Find(c => c.CARRIERID == car &&  c._mTask.PAUSE && c._mTask.HADLOAD);
            if (item == null)
            {
                string msg = "找不到挂起的小车（" + car + "）任务";
                _NoticeMsg(msg);
                log.LOG(msg);
            }
            else
            {
                item.ReUseAfterCarWash(index);
                if (item._mTask.NDCREDIRECTSITE == "")
                {
                    string msg = "Carwash原任务" + item._mTask.TASKID + "没有重定向信息！";
                    _NoticeMsg(msg);
                    log.LOG(msg);
                    return;
                }
                DoRedirect(index, item._mTask.NDCREDIRECTSITE);
            }

        }

        #endregion


        #region 抽象方法

        internal abstract void _NoticeMsg(string msg);

        internal abstract void _NoticeDelete(NDCItem model);

        internal abstract void _NoticeUpdate(NDCItem model);

        internal abstract void _NoticeRedirect(NDCItem model);

        internal abstract void _NoticeWcsLoading(int taskid, string agvid);

        internal abstract void _NoticeWcsMagic(int id, string agv, int magic);

        #endregion
    }
}
