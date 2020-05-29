using Module;
using Module.DEV;
using System.Collections.Generic;
using System.Threading;

namespace Socket
{
    public class SocketServer
    {
        /// <summary>
        /// 运行？
        /// </summary>
        private bool IsTurnOn = true;

        private readonly object _obj = new object();

        /// <summary>
        /// 刷新间隔
        /// </summary>
        internal const int REFRESH_TIMEOUT = 1 * 1000;

        public delegate void ReciveAwcDataHandler(string devName, DeviceAWC module);
        public delegate void ReciveArfDataHandler(string devName, DeviceARF module);
        public delegate void ReciveRgvDataHandler(string devName, DeviceRGV module);
        public delegate void ReciveFrtDataHandler(string devName, DeviceFRT module);
        public delegate void RecivePklDataHandler(string devName, DevicePKL module);
        public event ReciveAwcDataHandler AwcDataRecive;
        public event ReciveArfDataHandler ArfDataRecive;
        public event ReciveRgvDataHandler RgvDataRecive;
        public event ReciveFrtDataHandler FrtDataRecive;
        public event RecivePklDataHandler PklDataRecive;

        /// <summary>
        /// 设备组
        /// </summary>
        public readonly List<SocketClient> Clients;

        public SocketServer()
        {
            Clients = new List<SocketClient>();
            new Thread(RefreshClient) { IsBackground = true }.Start();
        }

        public void AddClient(string dev, string host, int port, byte[] refreshorder)
        {
            SocketClient client = new SocketClient(dev, host, port, refreshorder);
            client.ReceiveData += Client_ReceiveData;
            Clients.Add(client);
        }

        private void Client_ReceiveData(string devName, DevType head, IBaseModule module)
        {
            switch (head)
            {
                case DevType.行车:
                    AwcDataRecive?.Invoke(devName, (DeviceAWC)module);
                    break;
                case DevType.固定辊台:
                    FrtDataRecive?.Invoke(devName, (DeviceFRT)module);
                    break;
                case DevType.摆渡车:
                    ArfDataRecive?.Invoke(devName, (DeviceARF)module);
                    break;
                case DevType.运输车:
                    RgvDataRecive?.Invoke(devName, (DeviceRGV)module);
                    break;
                case DevType.包装线辊台:
                    PklDataRecive?.Invoke(devName, (DevicePKL)module);
                    break;
                default:
                    break;
            }
        }

        private void RefreshClient()
        {
            while (IsTurnOn)
            {
                Thread.Sleep(REFRESH_TIMEOUT);
                lock (_obj)
                {
                    if (Clients.Count == 0 || Clients == null) continue;

                    foreach (var client in Clients)
                    {
                        client.SenderHandler();
                    }
                }
            }
        }

        /// <summary>
        /// 发送设备指令(是否循环)
        /// </summary>
        /// <param name="dev">设备名</param>
        /// <param name="order">指令</param>
        /// <param name="isCycling">是否循环发送</param>
        public void SendOrder(string dev, byte[] order, bool isCycling)
        {
            lock (_obj)
            {
                Clients.Find(c => c.m_DevName == dev).SendMessage(order, isCycling);
            }
        }

        /// <summary>
        /// 关闭设备连接
        /// </summary>
        /// <param name="devname">关闭所有设备，指定名字则关闭指定设备</param>
        public void Close(string devname = null)
        {
            if (IsTurnOn)
            {
                IsTurnOn = false;

                lock (Clients)
                {
                    if (devname == null)
                    {
                        foreach (var client in Clients)
                        {
                            client.Close();
                        }
                        Clients.Clear();
                    }
                    else
                    {
                        SocketClient client = Clients.Find(c => { return c.m_DevName.Equals(devname); });
                        if (client != null)
                        {
                            client.Close();
                            Clients.Remove(client);
                        }
                    }
                }
            }
        }

        public bool IsConnected(string dev)
        {
            if (Clients.Exists(c=>c.m_DevName == dev))
            {
                return Clients.Find(c => c.m_DevName == dev).IsConnected;
            }
            else
            {
                return false;
            }
        }
    }
}
