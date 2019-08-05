using NDC8.ACINET.ACI;
using System;
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
        Init =0,

        /// <summary>
        /// 走到第四步 就可以重定位
        /// </summary>
        CanRedirect = 1,

        /// <summary>
        /// 已经有需重定位数据
        /// </summary>
        HasDirectInfo = 2,

        /// <summary>
        /// 已经到第六步了  还没定位
        /// </summary>
        NeedRedirect = 3,

        /// <summary>
        /// 已经重新定位
        /// </summary>
        Redirected = 4, 
    }
    /// <summary>
    /// NDC任务信息
    /// </summary>
    public class NDCItem
    {
        /// <summary>
        /// 任务唯一标识
        /// </summary>
        public int TaskID;

        internal _sMessage s;
        internal _bMessage b;
        private Message_vpil vpilmessage;

        public int OrderIndex;
        public int IKey;
        public int Magic;
        public int Status;
        public int CarrierId;//分配的agv小车


        public NDCItemStatus DirectStatus;

        public string StatusInfo;
        public string TaskInfo;

        //原始区域数据
        public string LoadStation;
        public string UnloadStation;
        public string RedirectUnloadStation;

        //Ndc处理数据
        public string NdcLoadStation;
        public string NdcUnloadStation;
        public string NdcRedirectUnloadStation;

        //重新定位数据计算
        public DateTime lastDirectTime;

        public bool CanDirect()
        {
            if (DirectStatus != NDCItemStatus.HasDirectInfo) return false;
            if (DateTime.Now.Subtract(lastDirectTime).TotalSeconds > 10)
            {
                lastDirectTime = DateTime.Now;
                return true;
            }
            return false;
        }

        public NDCItem()
        {
            s = new _sMessage();

            b = new _bMessage();

            DirectStatus = NDCItemStatus.Init;

            lastDirectTime = DateTime.Now;
        }

        /// <summary>
        /// 更新S消息
        /// </summary>
        /// <param name="message"></param>
        public void SetSMessage(Message_s message)
        {
            s.OrderIndex = message.Index;
            s.TransportStructure = message.TransportStructure;
            Magic = message.Magic;
            s.Magic1 = message.Magic;
            s.Magic2 = message.Magic2;
            s.Magic3 = message.Magic3;
            s.CarrierId = message.CarrierNumber;
            s.Station = message.CarrierStation;
            NdcLoadStation = message.CarrierStation != 0 ? message.CarrierStation+"":"";
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
            Status = message.Status;
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
