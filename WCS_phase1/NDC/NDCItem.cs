using NDC8.ACINET.ACI;
using System;
using System.Text;
using WCS_phase1.NDC.Message;

namespace WCS_phase1.NDC
{

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
        private _vpilMessage v;

        public int OrderIndex;
        public int IKey;
        public int Magic;
        public int Status;
        public int CarrierId;//分配的agv小车
        public bool IsFinish = false;

        /// <summary>
        /// 车重定位任务的状态
        /// </summary>
        public NDCItemStatus DirectStatus;

        /// <summary>
        /// 小车PLC状态
        /// </summary>
        public NDCPlcStatus PLCStatus;

        public string StatusInfo;
        public string TaskInfo;
        public string VpiInfo;

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
        public DateTime lastLoadTime;
        public DateTime lastUnLoadTime;

        /// <summary>
        /// 判断是否符合重定位
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 判断是否符合装货条件
        /// </summary>
        /// <returns></returns>
        public bool CanLoadPlc()
        {
            if (PLCStatus != NDCPlcStatus.LoadReady) return false;
            if (DateTime.Now.Subtract(lastLoadTime).TotalSeconds > 10)
            {
                lastLoadTime = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否符合卸货条件
        /// </summary>
        /// <returns></returns>
        public bool CanUnLoadPlc()
        {
            if (PLCStatus != NDCPlcStatus.UnloadReady) return false;
            if (DateTime.Now.Subtract(lastUnLoadTime).TotalSeconds > 10)
            {
                lastUnLoadTime = DateTime.Now;
                return true;
            }
            return false;
        }

        public NDCItem()
        {
            s = new _sMessage();

            b = new _bMessage();

            v = new _vpilMessage();

            DirectStatus = NDCItemStatus.Init;

            PLCStatus = NDCPlcStatus.LoadUnReady;

            lastDirectTime = DateTime.Now;
            lastLoadTime = DateTime.Now;
            lastUnLoadTime = DateTime.Now;
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
        /// 更新V消息
        /// </summary>
        /// <param name="message"></param>
        public void SetVMessage(Message_vpil message)
        {
            v.CarId = message.CarId;
            v.PlcLp1 = message.PlcLp1;
            v.Value1 = message.Value1;
            VpiInfo = v.ToString();
        }

        /// <summary>
        /// 返回详细信息
        /// </summary>
        /// <returns></returns>
        //public override string ToString()
        //{
        //    StringBuilder str = new StringBuilder();
        //    if (s.OrderIndex != 0)
        //    {
        //        str.Append("S:index=" + s.OrderIndex+",");
        //        str.Append("S:CarrierId=" + s.CarrierId + ",");
        //        str.Append("S:Magic1=" + s.Magic1 + ",");
        //    }

        //    if (b.OrderIndex != 0)
        //    {
        //        str.Append("B:index=" + b.OrderIndex + ",");
        //        str.Append("B:IKEY=" + b.IKEY + ",");
        //        str.Append("B:Status=" + b.Status + ",");
        //    }

        //    return str.ToString();
        //}
    }



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
    /// PLC状态
    /// </summary>
    public enum NDCPlcStatus
    {
        /// <summary>
        /// 装货未准备好
        /// </summary>
        LoadUnReady = 0,

        /// <summary>
        /// 装货准备好了
        /// </summary>
        LoadReady = 1,

        /// <summary>
        /// 装货中
        /// </summary>
        Loading = 2,

        /// <summary>
        /// 装货完成 
        /// </summary>
        Loaded = 3,

        /// <summary>
        /// 卸货未准备好
        /// </summary>
        UnloadUnReady = 4,

        /// <summary>
        /// 卸货准备好
        /// </summary>
        UnloadReady = 5,

        /// <summary>
        /// 卸货中
        /// </summary>
        Unloading = 6,

        /// <summary>
        /// 卸货完成
        /// </summary>
        Unloaded = 7
    }

}
