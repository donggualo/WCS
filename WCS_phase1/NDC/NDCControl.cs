using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WCS_phase1.LOG;
using NDC8.ACINET.ACI;

namespace WCS_phase1.NDC
{
    class NDCControl
    {
        #region Param
        //Parameter that will keep track of if system is halted or not
        private bool systemHalted;

        //Used for trace file
        private List<FileInfo> fileList = new List<FileInfo>();

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
        private List<NDCItem> nDCItems;

        #endregion

        #region Init

        /// <summary>
        /// 构造方法
        /// </summary>
        public NDCControl()
        {
            //Set a start value, that the system is not halted
            systemHalted = false;

            log = new Log("ndcAGV");

            nDCItems = new List<NDCItem>();

            log.LOG("Host Started.");
        }

        #endregion

        #region Main Method

        /// <summary>
        /// Connect/Disconnect to system
        /// </summary>
        public void DoConnectNDC()
        {
            //If false and not trying to connect it should connect.
            if (!VCP9412.Instance.IsConnected && !connecting)
            {
                Connect();
            }
            else
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
        public void DoStartOrder(string PrioFromUser,string IKEYFromUser,string DropStationFromUser,string PickStationFromUser)
        {

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
        public void DoRedirect(int index,int station)
        {

            SendNewMForRedirect(index, station);
            log.LOG(string.Format("[Index {0}]  Redirect sent, station: {1}", index, station));

        }


        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="IndexFromUser"></param>
        public void DoDeleteOrder(string IndexFromUser)
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
                    else if (s_response.Magic == 4)
                    {
                        SendNewM(index, phase);
                    }
                    else if (s_response.Magic == 6)
                    {
                        SendNewM(index, phase);
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

        #endregion

        #region NDCITEM

        public void UpdateItem(Message_s message)
        {

            NDCItem ndcItem = nDCItems.Find(c => { return c.OrderIndex == message.Index; });

            if (ndcItem != null)
            {
                ndcItem.SetSMessage(message);
                log.LOG(ndcItem.StatusInfo);
            }
            else
            {
                NDCItem item = new NDCItem();
                item.OrderIndex = message.Index;
                item.CarrierId = message.CarrierNumber;
                item.SetSMessage(message);
                nDCItems.Add(item);
                log.LOG(item.StatusInfo);
            }
            CheckMagic(message.Index, message.Magic);
        }

        public void UpdateItem(Message_b message)
        {

            NDCItem ndcItem = nDCItems.Find(c => { return c.OrderIndex == message.Index; });

            if (ndcItem != null)
            {
                ndcItem.SetBMessage(message);
                log.LOG(ndcItem.TaskInfo);
            }
            else
            {
                NDCItem item = new NDCItem();
                item.OrderIndex = message.Index;
                item.IKey = message.IKEY;
                item.SetBMessage(message);
                nDCItems.Add(item);
                log.LOG(ndcItem.TaskInfo);
            }
            CheckStatus(message.Status);
        }

        /// <summary>
        /// 检查任务状态
        /// </summary>
        /// <param name="status"></param>
        private void CheckStatus(int status)
        {

        }


        /// <summary>
        /// 检查任务进程
        /// </summary>
        /// <param name="index"></param>
        /// <param name="magic"></param>
        private void CheckMagic(int index,int magic)
        {
            switch (magic)
            {
                //
                case 6:
                    DoRedirect(index, 2366);
                    break;
                default:
                    break;
            }

        }

        #endregion



    }
}
