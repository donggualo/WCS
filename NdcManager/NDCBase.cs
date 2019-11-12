using ModuleManager.NDC;
using NDC8.ACINET.ACI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolManager;

namespace NdcManager
{
    /// <summary>
    /// 控制NDC 开关，接收发
    /// </summary>
    public abstract class NDCBase
    {
        #region 参数

        /// <summary>
        /// 是否暂停处理NDC过来的信息
        /// </summary>
        private bool systemHalted;

        /// <summary>
        /// 人工干预停止
        /// </summary>
        private bool disconnectedByUser = false;

        /// <summary>
        /// 使用重连
        /// </summary>
        private bool connecting = false;

        /// <summary>
        /// NDC 服务IP
        /// </summary>
        internal string IPaddress = "10.9.30.120";

        /// <summary>
        /// NDC服务端口
        /// </summary>
        internal int Port = 30001;

        /// <summary>
        /// 当前IKEY值
        /// </summary>
        internal int Ikey = 1;

        /// <summary>
        /// 日志保存
        /// </summary>
        public Log log;


        #endregion

        #region 构造函数/对外连接断开
        public NDCBase()
        {
            //Set a start value, that the system is not halted
            systemHalted = false;

            log = new Log("ndcAGV");

            log.LOG("Host Started.");
        }


        /// <summary>
        /// 连接NDC服务
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
        /// 断开NDC服务
        /// </summary>
        public void DoDisConnectNDC()
        {
            if (VCP9412.Instance.IsConnected || connecting)
            {
                Disconnect();
            }
        }
        #endregion

        #region 发送任务方法

        /// <summary>
        /// 开始一个任务
        /// </summary>
        /// <param name="PrioFromUser">优先级</param>
        /// <param name="IKEYFromUser">ikey值</param>
        /// <param name="DropStationFromUser">卸货地点</param>
        /// <param name="PickStationFromUser">装货地点</param>
        internal void DoStartOrder(TempItem tempItem)
        {
            string PrioFromUser = tempItem.Prio;
            string IKEYFromUser = tempItem.IKey + "";
            string PickStationFromUser = tempItem.NdcLoadSite;
            string DropStationFromUser = tempItem.NdcUnloadSite;
            List<string> StartOrderList = new List<string>();

            //int Prio, IKEY, DropStation, PickStation;

            bool prio = int.TryParse(PrioFromUser, out _);
            bool ikey = int.TryParse(IKEYFromUser, out _);
            bool dropstation = int.TryParse(DropStationFromUser, out _);
            bool pickstation = int.TryParse(PickStationFromUser, out _);

            //if only numbers in textboxes
            if (prio && ikey && dropstation && pickstation)
            {

                StartOrderList.Add(PrioFromUser);
                StartOrderList.Add(IKEYFromUser);
                StartOrderList.Add(DropStationFromUser);
                StartOrderList.Add(PickStationFromUser);

                //Send event
                Q_StartOrder(StartOrderList);
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
        internal void DoRedirect(int index, string station)
        {
            if (!int.TryParse(station, out int sta))
            {
                //Log redirect Sta Is Wrong
            }
            SendNewMForRedirect(index, sta);
            log.LOG(string.Format("[Index {0}]  Redirect sent, station: {1}", index, station));

        }

        /// <summary>
        /// 发送装货命令给小车PLC
        /// </summary>
        /// <param name="index">任务ID</param>
        /// <param name="carid"></param>
        internal void DoLoad(int index, int carid)
        {
            SendHpilWordForPLC(carid, 29, 1);
        }

        /// <summary>
        /// 发送卸货命令给小车PLC
        /// </summary>
        /// <param name="index">任务ID</param>
        /// <param name="carid"></param>
        internal void DoUnLoad(int index, int carid)
        {
            SendHpilWordForPLC(carid, 29, 2);
        }

        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="IndexFromUser"></param>
        internal void DoDeleteOrder(string IndexFromUser)
        {
            List<string> DeleteOrderList = new List<string>();

            bool index = int.TryParse(IndexFromUser, out _);

            //If only number in textboxes
            if (index)
            {
                DeleteOrderList.Add(IndexFromUser);

                //Send event
                D_DeleteOrder(DeleteOrderList);
            }

            //Not only numbers in any of the textboxes
            else
            {
                //MessageBox.Show("Wrong data entered");
            }
        }

        #endregion

        #region 发送NDC格式消息

        /// <summary>
        /// Delete order event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void D_DeleteOrder(List<string> DeleteOrderList)
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
        private void Q_StartOrder(List<string> orderList)
        {
            List<int> StartOrderList = new List<int>();

            //byte[] data = new byte[1024 * 1024];
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
        private void Q_StartOrderSeq(List<string[]> StartOrderSeqEventData)
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

        #region 连接和接收信息

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

            //初始化需要发送一条信息到SM
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
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 8)//到达卸货
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
                else if (msg.Type == "<")//改写PLC的结果返回
                {
                    Message_vpil v_response = (Message_vpil)msg;
                    //MessageBox.Show(string.Format("成功改写Data{0},改写的值为{1}", v_response.PlcLp1 + 1, v_response.Value1));
                    UpdateItem(v_response);
                }
            }
        }

        #endregion

        #region 发送NDC指令

        /// <summary>
        /// Send _m message for next phase
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nextPhase"></param>
        private void SendNewM(int index, int nextPhase)
        {
            List<int> list = new List<int>
            {
                nextPhase
            };
            Message_m m = new Message_m(index, 1, 0x12, list);
            VCP9412.Instance.SendMessage(m);
        }

        /// <summary>
        /// Send _m message for redirect only
        /// </summary>
        /// <param name="index"></param>
        /// <param name="station"></param>
        private void SendNewMForRedirect(int index, int station)
        {
            List<int> list = new List<int>
            {
                142,
                station
            };
            Message_m m = new Message_m(index, 1, 0x12, list);
            VCP9412.Instance.SendMessage(m);
        }

        /// <summary>
        /// Send _g message for Start Up
        /// </summary>
        //private void SendNewG(int magic, byte code, byte par_num, int par_ix, List<int> SendNewGList)
        private void SendNewG()
        {
            List<int> SendNewGList = new List<int>
            {
                2
            };
            //SendNewG(0, 2, 1, 0, SendNewGList);
            Message_g g = new Message_g(0, 2, 1, 0, SendNewGList);
            //Message_g g = new Message_g(magic, code, par_num, par_ix, SendNewGList);
            VCP9412.Instance.SendMessage(g);
        }

        /// <summary>
        /// Send _hpil message for PLC param value change
        /// </summary>
        /// <param name="Carid">车ID</param>
        /// <param name="Param">参数位置</param>
        /// <param name="Value">改变的值</param>
        private void SendHpilWordForPLC(int Carid, int Param, int Value)
        {
            Message_hpil_word h1 = new Message_hpil_word(Carid, 57344, 2, Param, Value);

            VCP9412.Instance.SendMessage(h1);
        }

        #endregion

        #region 抽象方法
        internal abstract void UpdateItem(Message_vpil m);
        internal abstract void UpdateItem(Message_s m);
        internal abstract void UpdateItem(Message_b m);

        #endregion

       
    }
}
