using NDC8.ACINET.ACI;
using System.Text;
using WCS_phase1.NDC.Message;

namespace WCS_phase1.NDC
{
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
        public int CarrierId;

        public string StatusInfo;
        public string TaskInfo;

        public NDCItem()
        {
            s = new _sMessage();

            b = new _bMessage();
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
