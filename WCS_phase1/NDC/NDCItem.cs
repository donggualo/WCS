using NDC8.ACINET.ACI;
using System.Text;
using WCS_phase1.NDC.Message;

namespace WCS_phase1.NDC
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum NDCItemStatus
    {

        /// <summary>
        /// 初始化
        /// </summary>
        Init = 0,
        /// <summary>
        /// 任务开始
        /// </summary>
        OrderStart = 1,

        /// <summary>
        /// 去装货点
        /// </summary>
        ToLoadPos = 2,

        /// <summary>
        /// 装货
        /// </summary>
        Loaded = 3,

        /// <summary>
        /// 为重新分配卸货点
        /// </summary>
        UnRedirect = 4,

        /// <summary>
        /// 已经重新分配卸货点
        /// </summary>
        Redirect = 5,

        /// <summary>
        /// 前往卸货点
        /// </summary>
        ToUnLoadPos = 6,

        /// <summary>
        /// 卸货
        /// </summary>
        Unloaded = 7,

        /// <summary>
        /// 任务完成
        /// </summary>
        OrderFinish = 8
    }
    /// <summary>
    /// NDC任务信息
    /// </summary>
    public class NDCItem
    {
        private _sMessage s;
        private _bMessage b;
        private Message_vpil vpilmessage;

        public int OrderIndex;
        public int IKey;
        public int CarrierId;//分配的agv小车

        public bool CarAllocate = false;//是否分配车了
        public NDCItemStatus status;

        public string StatusInfo;
        public string TaskInfo;

        public NDCItem()
        {
            s = new _sMessage();

            b = new _bMessage();

            status = NDCItemStatus.Init;
        }

        /// <summary>
        /// 更新S消息
        /// </summary>
        /// <param name="message"></param>
        public void SetSMessage(Message_s message)
        {
            s.OrderIndex = message.Index;
            s.TransportStructure = message.TransportStructure;
            s.Magic1 = message.Magic;
            s.Magic2 = message.Magic2;
            s.Magic3 = message.Magic3;
            s.CarrierId = message.CarrierNumber;
            s.Station = message.CarrierStation;
            StatusInfo = s.ToString();
        }

        /// <summary>
        /// 更新B消息
        /// </summary>
        /// <param name="message"></param>
        public void SetBMessage(Message_b message)
        {
            b.OrderIndex = message.Index;
            b.TransportStructure = message.TransportStructure;
            b.Status = message.Status;
            b.Parnumber = message.ParNo;
            b.IKEY = message.IKEY;
            TaskInfo = b.ToString();

            //小车分配和连接上了
            if (b.Status == 37)
            {
                CarrierId = b.Parnumber;
                CarAllocate = true;
            }
        }

        /// <summary>
        /// 返回详细信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            if (s.OrderIndex != 0)
            {
                str.Append("S:index=" + s.OrderIndex+",");
                str.Append("S:CarrierId=" + s.CarrierId + ",");
                str.Append("S:Magic1=" + s.Magic1 + ",");
            }

            if (b.OrderIndex != 0)
            {
                str.Append("B:index=" + b.OrderIndex + ",");
                str.Append("B:IKEY=" + b.IKEY + ",");
                str.Append("B:Status=" + b.Status + ",");
            }

            return str.ToString();
        }
    }
}
