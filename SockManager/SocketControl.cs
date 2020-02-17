using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Socket
{
    /// <summary>
    /// 设备管理器
    /// 1.通过该类添加连接设备
    /// 2.通过该类获取设备信息
    /// </summary>
    public class SocketControl
    {
        private readonly object _obj = new object();
        private bool runRefresh = true;
        
        /// <summary>
        /// 设备列表
        /// </summary>
        private readonly List<SocketClient> client =  new List<SocketClient>();

        private List<SocketClient> Clients
        {
            get
            {
                lock (_obj)
                {
                    return client;
                }
            }
        }


        /// <summary>
        /// 构造函数
        /// TODO 可以加载读取数据库设备信息
        /// </summary>
        public SocketControl()
        {
            //测试
            new Thread(RefreshClient)
            {
                IsBackground = true
            }.Start();
        }

        private void RefreshClient()
        {
            while (runRefresh)
            {
                Thread.Sleep(3000);
                foreach (var client in Clients)
                {
                    client.Refresh();
                }
            }
        }

        /// <summary>
        /// 判断设备是否在线
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsAlive(string name)
        {
            SocketClient client = Clients.Find(c => { return c.Name.Equals(name); });
            if (client != null)
            {
                return client.IsAlive;
            }

            return false;
        }


        /// <summary>
        /// 获取设备当前的最新数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public byte[] GetByteArr(string name)
        {
            SocketClient client = Clients.Find(c => { return c.Name.Equals(name); });
            if (client != null)
            {
                return client.Bdata;
            }

            return new byte[0];
        }

        /// <summary>
        /// 获取最新的时间
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool GetUpdateTime(string name,out string date)
        {
            SocketClient client = Clients.Find(c => { return c.Name.Equals(name); });
            if (client != null)
            {
                date = client.UpDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                return true;
            }
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return false;
        }


        /// <summary>
        /// 添加联网设备
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns>bool</returns>
        public bool AddClient(string name, string ip, int port, byte[] refreshB, out string result)
        {
            if (!IsIP(ip))
            {
                result = name + "的IP 不合法:" + ip;
                return false;
            }

            if(name==null || name.Equals(""))
            {
                result = name + "不能为空";
                return false;
            }

            if (port == 0)
            {
                result = name + "的端口不能为零";
                return false;
            }

            if (Clients.Find(c => { return c.Name.Equals(name); }) == null)
            {
                Clients.Add(new SocketClient(ip, port, name,GetCRCByte(refreshB)));
                result = "";
                return true;
            }

            result = name+"名称的设备已经存在了";
            return false;
        }

        public void Exiting()
        {
            runRefresh = false;
            Close();
        }
        /// <summary>
        /// 关闭设备连接
        /// </summary>
        /// <param name="name">名字为null 关闭所有设备， 指定名字则关闭指定设备</param>
        public void Close(string name = null)
        {
            if (name == null)
            {
                foreach (var client in Clients)
                {
                    client.Close();
                }
                Clients.Clear();
            }
            else
            {
                SocketClient client = Clients.Find(c => { return c.Name.Equals(name); });
                if (client != null)
                {
                    client.Close();
                    Clients.Remove(client);
                }
            }

        }


        /// <summary>
        /// 向指定设备发送信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="order"></param>
        public bool SendToClient(string name, string order, out string result)
        {
            return SendToClient(name, CRCMethod.StringToHexByte(order),out result);
        }        
        
        /// <summary>
        /// 向指定设备发送信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="order"></param>
        public bool SendToClient(string name, byte[] msg, out string result)
        {
            SocketClient clinet = Clients.Find(c => { return name.Equals(c.Name); });
            if (clinet != null && clinet.IsConnect())
            {
                //byte[] b = new byte[msg.Length + 2];
                //msg.CopyTo(b, 0);
                //CRCMethod.ToModbusCRC16Byte(msg).CopyTo(b, msg.Length);
                //clinet.Send(b);
                clinet.Send(GetCRCByte(msg));
                result = "";
                return true;
            }
            if (clinet == null) result = "未找到该客户端";
            else result = "客户端未连接";
            return false;
        }

        public void SwithRefresh(string name,bool onoff)
        {
            SocketClient client = Clients.Find(c => { return name.Equals(c.Name); });
            if (client != null && client.IsConnect())
            {
                client.DoRefresh = onoff;
            }
         }

        /// <summary>
        /// 获得添加校验码后的结果
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] GetCRCByte(byte[] msg)
        {
            byte[] b = new byte[msg.Length + 2];
            msg.CopyTo(b, 0);
            CRCMethod.ToModbusCRC16Byte(msg).CopyTo(b, msg.Length);
            return b;
        }


        #region 检查是否为IP地址
        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        #endregion
    }
}
