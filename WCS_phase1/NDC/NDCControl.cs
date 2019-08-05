using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WCS_phase1.LOG;
using NDC8.ACINET.ACI;
using System.Text;

namespace WCS_phase1.NDC
{
    class NDCControl
    {
        #region Param
        //Parameter that will keep track of if system is halted or not
        private bool systemHalted;

        //Keep track if disconnected by button or communication is down.
        private bool disconnectedByUser = false;

        //Check if connecting
        private bool connecting = false;

        //IP address where System Manager is located
        private const string IPaddress = "10.9.30.120";

        //Port number for the ACI connection
        private const int Port = 30001;

        private Log log;

        /// <summary>
        /// 保存所有任务
        /// </summary>
        private List<NDCItem> _items = new List<NDCItem>();
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
        /// 配置文件工具类
        /// </summary>
        IniFiles ini;

        /// <summary>
        /// 装货点和卸货点NDC对应信息
        /// </summary>
        Dictionary<string, string> loadStaDic, unLoadStaDic;

        /// <summary>
        /// 配置文件保存Section名称
        /// </summary>
        private string loadSection = "Load", unloadSection = "Unload",iKeySection = "IKey",tempSection= "Temp",itemSection= "Item";

        /// <summary>
        /// 当前IKEY值
        /// </summary>
        int Ikey=1;

        Thread redirectThread;
        List<int> redirectItemList;
        #endregion

        #region Init

        /// <summary>
        /// 构造方法
        /// </summary>
        public NDCControl()
        {
            //Set a start value, that the system is not halted
            systemHalted = false;

            ini = new IniFiles(AppDomain.CurrentDomain.BaseDirectory + @"\NdcSetting.ini");

            log = new Log("ndcAGV");

            log.LOG("Host Started.");

            DoReadIniFile();
            DoReadIKey();
            DoReadItemTempIF();

            redirectItemList = new List<int>();
            redirectThread = new Thread(CheckRedirectItem)
            {
                IsBackground = true
            };

            redirectThread.Start();
        }

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

        #endregion

        #region Main Method

        /// <summary>
        /// 关闭任务前保存数据
        /// </summary>
        private void BeforeClose()
        {
            if (Ikey >= 99) Ikey = 1;
            ini.WriteValue(iKeySection, "IKey", Ikey);

            if (TempList.Count() > 0)
            {
                StringBuilder str = new StringBuilder();
                foreach(var i in TempList)
                {
                    if (str.Length != 0) str.Append(";");
                    str.Append(i.TaskID +"&");
                    str.Append(i.IKey + "&");
                    str.Append(i.Prio + "&");
                    str.Append(i.LoadStation + "&");
                    str.Append(i.UnloadStation + "&");
                    str.Append(i.NdcLoadStation + "&");
                    str.Append(i.NdcUnloadStation + "&");
                    str.Append(i.RedirectUnloadStation + "&");
                }
                ini.WriteValue(tempSection, "tempinfo", str.ToString());
            }

            if (Items.Count() > 0)
            {
                StringBuilder str = new StringBuilder();
                foreach (var i in Items)
                {
                    if (str.Length != 0) str.Append(";");
                    str.Append(i.OrderIndex + "&");
                    str.Append(i.TaskID + "&");
                }
                ini.WriteValue(itemSection, "iteminfo", str.ToString());
            }
        }


        /// <summary>
        /// Connect to system
        /// </summary>
        public void DoConnectNDC()
        {
            //If false and not trying to connect it should connect.
            if (!VCP9412.Instance.IsConnected && !connecting)
            {
                Connect();
            }
        }

        /// <summary>
        /// Disconnect to system
        /// </summary>
        public void DoDisConnectNDC()
        {
            BeforeClose();
            if (VCP9412.Instance.IsConnected || connecting)
            {
                Disconnect();
            }
        }


        /// <summary>
        /// 开始一个任务
        /// </summary>
        /// <param name="PrioFromUser">优先级</param>
        /// <param name="IKEYFromUser">ikey值</param>
        /// <param name="DropStationFromUser">卸货地点</param>
        /// <param name="PickStationFromUser">装货地点</param>
        private void DoStartOrder(TempItem tempItem)
        {
            string PrioFromUser = tempItem.Prio;
            string IKEYFromUser = tempItem.IKey;
            string PickStationFromUser = tempItem.NdcLoadStation;
            string DropStationFromUser = tempItem.NdcUnloadStation;
            List<string> StartOrderList = new List<string>();

            int Prio, IKEY, DropStation, PickStation;

            bool prio = int.TryParse(PrioFromUser, out Prio);
            bool ikey = int.TryParse(IKEYFromUser, out IKEY);
            bool dropstation = int.TryParse(DropStationFromUser, out DropStation);
            bool pickstation = int.TryParse(PickStationFromUser, out PickStation);

            //if only numbers in textboxes
            if (prio && ikey && dropstation && pickstation)
            {

                StartOrderList.Add(PrioFromUser);
                StartOrderList.Add(IKEYFromUser);
                StartOrderList.Add(DropStationFromUser);
                StartOrderList.Add(PickStationFromUser);
                
                //Send event
                q_StartOrder(StartOrderList);
            }
            //Not only numbers in any of the textboxes
            else
            {
                //MessageBox.Show("Wrong data entered");
            }
        }


        /// <summary>
        /// 重新定位任务的目标地点
        /// </summary>
        /// <param name="index"></param>
        /// <param name="station"></param>
        private void DoRedirect(int index,string station)
        {
            if(!int.TryParse(station, out int sta))
            {
                //Log redirect Sta Is Wrong
            }
            SendNewMForRedirect(index, sta);
            log.LOG(string.Format("[Index {0}]  Redirect sent, station: {1}", index, station));

        }


        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="IndexFromUser"></param>
        private void DoDeleteOrder(string IndexFromUser)
        {
            List<string> DeleteOrderList = new List<string>();

            int Index;

            bool index = int.TryParse(IndexFromUser, out Index);

            //If only number in textboxes
            if (index)
            {
                DeleteOrderList.Add(IndexFromUser);

                //Send event
                d_DeleteOrder(DeleteOrderList);
            }

            //Not only numbers in any of the textboxes
            else
            {
                //MessageBox.Show("Wrong data entered");
            }
        }

        /// <summary>
        /// 重新读取装货点，卸货点对应NDC实际使用信息
        /// </summary>
        public void DoReadIniFile()
        {
            loadStaDic = ini.ReadAllValue(loadSection);
            unLoadStaDic = ini.ReadAllValue(unloadSection);
        }

        /// <summary>
        /// 获取IKey值
        /// </summary>
        private void DoReadIKey()
        {
            if(int.TryParse(ini.ReadStrValue(iKeySection, "IKey"), out int ikey))
            {
                Ikey = ikey+1;
                if (Ikey >= 99) Ikey = 1;
            }
        }

        /// <summary>
        /// 读取关闭前保存的任务信息
        /// </summary>
        private void DoReadItemTempIF()
        {
            string tempinfo = ini.ReadStrValue(tempSection, "tempinfo");
            string iteminfo = ini.ReadStrValue(itemSection, "iteminfo");
            if (iteminfo != null && iteminfo != "")
            {
                string[] items = iteminfo.Split(';');
                foreach (var i in items)
                {
                    string[] inf = i.Split('&');
                    NDCItem item = new NDCItem();
                    item.OrderIndex = int.Parse(inf[0]);
                    item.TaskID = int.Parse(inf[1]);
                    Items.Add(item);
                }
                ini.WriteValue(itemSection, "tempinfo", "");
            }
            if (tempinfo != null && tempinfo != "")
            {
                string[] items = tempinfo.Split(';');
                foreach(var i in items)
                {
                    string[] inf = i.Split('&');
                    TempItem item = new TempItem()
                    {
                        TaskID = int.Parse(inf[0]),
                        IKey = inf[1],
                        Prio = inf[2],
                        LoadStation = inf[3],
                        UnloadStation = inf[4],
                        NdcLoadStation = inf[5],
                        NdcUnloadStation = inf[6],
                        RedirectUnloadStation = inf[7]
                    };
                    TempList.Add(item);
                }
                ini.WriteValue(tempSection, "tempinfo", "");
            }
        }
        
        #endregion

        #region Operate Method

        /// <summary>
        /// Delete order event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void d_DeleteOrder(List<string> DeleteOrderList)
        {
            //The "real" parse function is already done, so only numbers as strings in the list
            ushort Index = ushort.Parse(DeleteOrderList[0]);

            Message_n n = new Message_n(Index);
            VCP9412.Instance.SendMessage(n);
        }

        /// <summary>
        /// Single order start event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void q_StartOrder(List<string> orderList)
        {
            List<int> StartOrderList = new List<int>();

            byte[] data = new byte[1024 * 1024];
            //The "real" parse function is already done, so only numbers as strings in the list
            int Prio = int.Parse(orderList[0]);
            int Ikey = int.Parse(orderList[1]);
            int DropStn = int.Parse(orderList[2]);
            int PickStn = int.Parse(orderList[3]);

            StartOrderList.Add(Ikey);
            StartOrderList.Add(PickStn);
            StartOrderList.Add(DropStn);

            Message_q q = new Message_q(1, Prio + 128, 1, Ikey, StartOrderList);
            VCP9412.Instance.SendMessage(q);
        }


        /// <summary>
        /// Event when order list is started
        /// </summary>
        /// <param name="StartOrderSeqEventData"></param>
        void q_StartOrderSeq(List<string[]> StartOrderSeqEventData)
        {
            int count = 0;
            string datetime = " ";
            string timeNextOrder = "";
            TimeSpan StartTime = DateTime.Now.TimeOfDay;
            TimeSpan TOffset;
            TimeSpan TimeNextOrder;
            bool sendData = false;
            string[] dataToBeSent = null;
            List<string[]> orderDataList = StartOrderSeqEventData.Select(item => (string[])item.Clone()).ToList();

            while (orderDataList.Count > 0)
            {
                foreach (string[] item in orderDataList)
                {
                    count++;
                    TOffset = TimeSpan.Parse(item[0]);
                    TimeNextOrder = TOffset + StartTime;
                    timeNextOrder = string.Concat(TimeNextOrder.ToString().TakeWhile((c) => c != '.'));
                    datetime = string.Concat(DateTime.Now.TimeOfDay.ToString().TakeWhile((c) => c != '.'));

                    if (datetime == timeNextOrder || count == 1)
                    {
                        dataToBeSent = item;
                        sendData = true;
                        break;
                    }
                }

                if (orderDataList.Count == 0)
                    return;

                if (sendData)
                {
                    int Prio = Convert.ToInt32(dataToBeSent[1]); ;
                    int Ikey = Convert.ToInt32(dataToBeSent[2]);
                    int Fetch = Convert.ToInt32(dataToBeSent[3]);
                    int Drop = Convert.ToInt32(dataToBeSent[4]);

                    Message_q q = new Message_q(1, Prio + 128, 1, Ikey, new List<int>() { Ikey, Fetch, Drop });
                    VCP9412.Instance.SendMessage(q);
                    sendData = false;
                    orderDataList.Remove(dataToBeSent);

                }
                Thread.Sleep(1);
            }
        }


        #endregion

        #region Connect And Receive Method

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name=""></param>
        private void Connect()
        {
            VCP9412.Instance.Open(IPaddress, Port);
            VCP9412.Instance.Connected += Instance_Connected;
            VCP9412.Instance.Disconnected += Instance_Disconnected;
            VCP9412.Instance.ReciveData += Instance_ReciveData;

            connecting = true;

            string text = "Connecting to system....";

            log.LOG(text);
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        /// <param name=""></param>
        private void Disconnect()
        {
            //Set disconnected by user
            disconnectedByUser = true;
            connecting = false;
            VCP9412.Instance.Close();
            VCP9412.Instance.Connected -= Instance_Connected;
            VCP9412.Instance.Disconnected -= Instance_Disconnected;
            VCP9412.Instance.ReciveData -= Instance_ReciveData;
            VCP9412.Instance.Dispose();
        }

        /// <summary>
        /// Event if System is Connected/Disconnected
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void Instance_Disconnected(string host, int port)
        {
            string text;

            //Check if dosconnected by user or if COM to system went down.
            if (disconnectedByUser)
            {
                text = "Host disconnected from system.";
            }
            else
            {
                text = "Communication lost with system, reconnecting....";
                connecting = true;
            }
            log.LOG(text);
        }

        /// <summary>
        /// Event if System is connected
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void Instance_Connected(string host, int port)
        {

            string text = "Host connected to system.";
            log.LOG(text);

            connecting = false;
            disconnectedByUser = false;

            SendNewG();
        }

        /// <summary>
        /// Event when Recive Data From System Manager
        /// </summary>
        /// <param name="msg"></param>
        private void Instance_ReciveData(IACIMessage msg)
        {
            if (!systemHalted)
            {
                if (msg.Type == "s")
                {
                    Message_s s_response = (Message_s)msg;
                    int index = s_response.Index;//任务序列 (SM返回)
                    int phase = s_response.Magic;//任务的阶段标识（相当于顺序)

                    #region Send the phase back to SM
                    if (s_response.Magic == 1)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 2)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 4)//到达接货
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 6)//接货完成，由重新定位返回数据
                    {
                        ///SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 8)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 10)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 11)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 48)
                    {
                        int sendNextPhase = 143;
                        SendNewM(index, sendNextPhase);
                    }
                    else if (s_response.Magic == 49)
                    {
                        int sendNextPhase = 143;
                        SendNewM(index, sendNextPhase);
                    }
                    else if (s_response.Magic == 50)
                    {
                        int sendNextPhase = 143;
                        SendNewM(index, sendNextPhase);
                    }
                    else if (s_response.Magic == 143)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 254)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 255)
                    {
                        SendNewM(index, phase);
                    }

                    #endregion

                    UpdateItem(s_response);
                }
                else if (msg.Type == "b")
                {
                    Message_b b_response = (Message_b)msg;

                    UpdateItem(b_response);
                }
            }
        }

        #endregion

        #region SendMessage

        /// <summary>
        /// Send _m message for next phase
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nextPhase"></param>
        private void SendNewM(int index, int nextPhase)
        {
            List<int> SendNewMList = new List<int>();
            SendNewMList.Add(nextPhase);
            Message_m m = new Message_m(index, 1, 0x12, SendNewMList);
            VCP9412.Instance.SendMessage(m);
        }

        /// <summary>
        /// Send _m message for redirect only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="station"></param>
        private void SendNewMForRedirect(int index, int station)
        {
            List<int> SendNewMForRedirectList = new List<int>();
            SendNewMForRedirectList.Add(142);
            SendNewMForRedirectList.Add(station);
            Message_m m = new Message_m(index, 1, 0x12, SendNewMForRedirectList);
            VCP9412.Instance.SendMessage(m);
        }

        /// <summary>
        /// Send _g message for Start Up
        /// </summary>
        //private void SendNewG(int magic, byte code, byte par_num, int par_ix, List<int> SendNewGList)
        private void SendNewG()
        {
            List<int> SendNewGList = new List<int>();
            SendNewGList.Add(2);
            //SendNewG(0, 2, 1, 0, SendNewGList);
            Message_g g = new Message_g(0, 2, 1, 0, SendNewGList);
            //Message_g g = new Message_g(magic, code, par_num, par_ix, SendNewGList);
            VCP9412.Instance.SendMessage(g);
        }

        #endregion

        #region NDCITEM

        /// <summary>
        /// 更新S消息
        /// </summary>
        /// <param name="message"></param>
        private void UpdateItem(Message_s message)
        {

            NDCItem ndcItem = Items.Find(c => { return c.OrderIndex == message.Index; });
            if (ndcItem == null)
            {
                ndcItem = new NDCItem
                {
                    OrderIndex = message.Index,
                    CarrierId = message.CarrierNumber
                };
                Items.Add(ndcItem);
            }
            ndcItem.SetSMessage(message);
            log.LOG(ndcItem.StatusInfo);
            CheckMagic(ndcItem,message);
        }

        /// <summary>
        /// 更新B消息
        /// </summary>
        /// <param name="message"></param>
        private void UpdateItem(Message_b message)
        {

            NDCItem ndcItem = Items.Find(c => { return c.OrderIndex == message.Index; });
            if (ndcItem == null)
            { 
                ndcItem = new NDCItem
                {
                    OrderIndex = message.Index,
                    IKey = message.IKEY
                };
                Items.Add(ndcItem);
            }
            ndcItem.SetBMessage(message);
            log.LOG(ndcItem.TaskInfo);
            CheckStatus(ndcItem,message);
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
                    Items.Remove(item);
                    break;
                case 27://IKEY in use
                    TempItem temp = TempList.Find(c => { return c.IKey.Equals("" + item.IKey); });
                    if (temp != null)
                    {
                        temp.IKey = "" + Ikey++;
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
            int index = item.OrderIndex;
            switch (item.Magic)
            {
                case 2://确定生成任务
                    item.NdcLoadStation =  item.s.Magic2+"";
                    item.NdcUnloadStation = item.s.Magic3 + "";
                    break;

                case 4: //小车到达接货点
                    if (item.DirectStatus != NDCItemStatus.HasDirectInfo)
                    {
                        item.DirectStatus = NDCItemStatus.CanRedirect;
                    }
                    if (!ReDirectList.Contains(item.OrderIndex))
                    {
                        ReDirectList.Add(item.OrderIndex);
                    }                    //TODO 告诉WCS 车已经到达
                    break;
                case 6: //小车接货完成
                    if (item.DirectStatus == NDCItemStatus.CanRedirect || item.DirectStatus == NDCItemStatus.Init)
                    {
                        item.DirectStatus = NDCItemStatus.NeedRedirect;
                    }
                    if (!ReDirectList.Contains(item.OrderIndex))
                    {
                        ReDirectList.Add(item.OrderIndex);
                    }
                    break;

                case 254://重新定位成功
                    item.DirectStatus = NDCItemStatus.Redirected;
                    ReDirectList.Remove(item.OrderIndex);
                    break;
                case 11://任务完成
                    Items.Remove(item);
                    break;
                default:
                    break;
            }
            GetTempInfo(item);
        }

        private void GetTempInfo(NDCItem item)
        {
            if (item.TaskID != 0) return;
            if(item.NdcLoadStation == null) return;
            TempItem tempItem = TempList.Find(c => { return c.NdcLoadStation == item.NdcLoadStation; });
            if (tempItem != null)
            {
                item.TaskID = tempItem.TaskID;
                item.LoadStation = tempItem.LoadStation;
                item.UnloadStation = tempItem.UnloadStation;
                item.RedirectUnloadStation = tempItem.RedirectUnloadStation;
                TempList.Remove(tempItem);
            }
        }

        /// <summary>
        /// 检查需重新定位的任务
        /// </summary>
        private void CheckRedirectItem()
        {
            while (true)
            {
                Thread.Sleep(3000);
                foreach(int index in ReDirectList)
                {
                    NDCItem item = Items.Find(c => { return c.OrderIndex == index; });
                    if(item !=null && item.CanDirect())
                    {
                        DoRedirect(item.OrderIndex, item.NdcRedirectUnloadStation);
                    }
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
        public bool AddNDCTask(int taskid,string loadstation, string unloadstation,out string result)
        {
            if (!VCP9412.Instance.IsConnected)
            {
                result = "NDC服务未连接";
                return false;
            }

            if(!loadStaDic.TryGetValue(loadstation, out string ndcLoadsta))
            {
                result = "装货点未配置";
                return false;
            }

            if (!unLoadStaDic.TryGetValue(unloadstation, out string ndcUnloadsta))
            {
                result = "卸货点未配置";
                return false;
            }

            if (Items.Find(c => { return c.TaskID == taskid; })!=null)
            {
                result = "找到相同任务ID("+taskid+")任务，不能再次添加";
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
                IKey = ""+ Ikey++,
                TaskID = taskid,
                LoadStation = loadstation,
                UnloadStation = unloadstation,
                NdcLoadStation = ndcLoadsta,
                NdcUnloadStation = ndcUnloadsta
            };
            TempList.Add(item);
            if (Ikey >= 99) Ikey = 1;
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
        public bool DoReDerect(int taskid,string unloadstation,out string result)
        {
            NDCItem item = Items.Find(c => { return c.TaskID == taskid 
                && (c.DirectStatus == NDCItemStatus.CanRedirect || c.DirectStatus == NDCItemStatus.NeedRedirect); });

            if (item == null)
            {
                TempItem temp = TempList.Find(c => { return c.TaskID == taskid; });
                if (temp != null)
                {
                    temp.RedirectUnloadStation = unloadstation;
                }
                else
                {
                    result = "并未找到任务ID为："+taskid+"的任务";
                    return false;
                }
            }
            else
            {
                if(unLoadStaDic.TryGetValue(unloadstation, out string ndcUnloadSta))
                {
                    item.NdcRedirectUnloadStation = ndcUnloadSta;
                    item.RedirectUnloadStation = unloadstation;
                    item.DirectStatus = NDCItemStatus.HasDirectInfo;
                }
            }

            result = "";
            return true;
        }

        #endregion

    }

    /// <summary>
    /// 暂时保存任务信息
    /// </summary>
    class TempItem
    {
        public string IKey;
        public string Prio;
        public int TaskID;
        public string LoadStation;
        public string UnloadStation;
        public string NdcLoadStation;
        public string NdcUnloadStation;
        public string RedirectUnloadStation;

        public DateTime addTime =  DateTime.Now;

        /// <summary>
        /// 能否重新添加
        /// </summary>
        /// <returns></returns>
        public bool CanReAdd()
        {
            return addTime.Subtract(DateTime.Now).TotalSeconds > 20;
        }
    }
}
