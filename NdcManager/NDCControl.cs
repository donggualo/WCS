using NDC8.ACINET.ACI;
using ModuleManager.NDC;

namespace NdcManager
{
    /// <summary>
    /// NDC任务类
    /// </summary>
    public class NDCControl : NDCDataHelper
    {

        #region 构造函数

        /// <summary>
        /// 构造方法
        /// </summary>
        public NDCControl():base()
        {

        }

        /// <summary>
        /// 关闭任务前保存数据
        /// </summary>
        public void Close()
        {
            DoDisConnectNDC();

            DoCloseNDCDataHelper();
        }
        #endregion

        #region [添加/重定向/取消任务]

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

            NDCItem item = new NDCItem();
            item._mTask.IKEY = Ikey++;
            item._mTask.TASKID = taskid;
            item._mTask.LOADSITE = loadstation;
            item._mTask.UNLOADSITE = unloadstation;
            item._mTask.NDCLOADSITE = ndcLoadsta;
            item._mTask.NDCUNLOADSITE = ndcUnloadsta;
            Items.Add(item);
            _sqlControl.InsertNdcItem(item);

            if (Ikey >= 60000) Ikey = 500;
            DoStartOrder(item);

            result = "";
            return true;
        }

        /// <summary>
        /// 根据任务ID,重新定位卸货地点
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="unloadstation"></param>
        /// <returns></returns>
        public bool DoReDerect(int taskid, string unloadstation, out string result,int index = -1)
        {
            NDCItem item = Items.Find(c =>
            {
                return c._mTask.TASKID == taskid && (index == -1 || c._mTask.NDCINDEX == index);
            });

            if (item != null)
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
                _NoticeUpdate(item);
            }
            else
            {
                result = "并未找到任务ID为：" + taskid + "的任务";
                return false;
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
                LoadItemList.Remove(item._mTask.NDCINDEX);
                //通知WCS
                NoticeWcsOnLoad?.Invoke(item._mTask.TASKID, item.CarrierId + "");
                result = "小车已经启动辊台了";
                return true;
            }

            if (item.PLCStatus != NDCPlcStatus.LoadReady)
            {
                result = "小车未准备好接货";
                return false;
            }

            if (!LoadItemList.Contains(item._mTask.NDCINDEX))
            {
                LoadItemList.Add(item._mTask.NDCINDEX);
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

            if (!UnLoadItemList.Contains(item._mTask.NDCINDEX))
            {
                UnLoadItemList.Add(item._mTask.NDCINDEX);
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

            NDCItem item = Items.Find(c =>{ return c._mTask.NDCINDEX == index; });

            if(item == null)
            {
                item = TempItems.Find(c => { return c._mTask.NDCINDEX == index; });
            }

            if (item == null)
            {
                result = "找不到任务Index:"+index+"任务.";
                return false;
            }
            item.CancleFromSystem = true;

            DoDeleteOrder(index + "");
            result = "取消成功";
            return true;
        }

        #endregion

        #region[界面接口：数据刷新]

        public delegate void GridDataHandler(NDCItem model);
        public delegate void MsgDataHandler(string msg);

        /// <summary>
        /// 通知界面提示需要重定向
        /// </summary>
        public event GridDataHandler NoticeRedirect;

        /// <summary>
        /// 通知界面更新数据
        /// </summary>
        public event GridDataHandler NoticeUpdate;

        /// <summary>
        /// 通知界面更新数
        /// </summary>
        public event GridDataHandler NoticeDelete;

        /// <summary>
        /// 通知界面消息
        /// </summary>
        public event MsgDataHandler NoticeMsg;

        #endregion

        #region[通知WCS：agv状态]

        public delegate void NDCLoadHandler(int taskid,string agvid);
        public delegate void NDCMagicHandler(int id, string agv, int magic);

        /// <summary>
        /// AGV 装货中 通知句柄
        /// </summary>
        public event NDCLoadHandler NoticeWcsOnLoad;

        /// <summary>
        /// 更新AGV当前任务状态
        /// </summary>
        public event NDCMagicHandler NoticeWcsMagic;

        #endregion

        #region 实现抽象方法


        internal override void _NoticeMsg(string msg)
        {
            NoticeMsg?.Invoke(msg);
        }

        internal override void _NoticeRedirect(NDCItem i)
        {
            NoticeRedirect?.Invoke(i);
        }


        internal override void _NoticeDelete(NDCItem i)
        {
            NoticeDelete?.Invoke(i);
        }

        internal override void _NoticeUpdate(NDCItem i)
        {
            if (i._mTask.IKEY != 0 && i._mTask.NDCINDEX != 0 && i._mTask.TASKID != 0)
            {
                if (NoticeUpdate != null)
                {
                    NoticeUpdate(i);
                }
                else
                {
                    if (_initItems.Contains(i)) return;
                    _initItems.Add(i);
                }

            }
        }

        internal override void _NoticeWcsLoading(int taskid, string agvid)
        {
            NoticeWcsOnLoad?.Invoke(taskid, agvid);

        }

        internal override void _NoticeWcsMagic(int id, string agv, int magic)
        {
            NoticeWcsMagic?.Invoke(id, agv, magic);
        }
        #endregion

    }
}
