using ModuleManager.NDC.Message;
using ModuleManager.NDC.SQL;
using NDC8.ACINET.ACI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManager.NDC
{
    public class NDCItem
    {

        public WCS_NDC_TASK _mTask;

        public _sMessage s;
        public _bMessage b;
        public _vpilMessage v;


        public int Magic;
        public int Status;
        public int CarrierId;//分配的agv小车
        public bool IsFinish = false;

        /// <summary>
        /// 车重定位任务的状态
        /// </summary>
        public NDCItemStatus DirectStatus;
        public bool HadDirectInfo;

        /// <summary>
        /// 小车PLC状态
        /// </summary>
        public NDCPlcStatus PLCStatus;

        public string StatusInfo;
        public string TaskInfo;
        public string VpiInfo;

        //重新定位数据计算
        public DateTime lastDirectTime;
        public DateTime lastLoadTime;
        public DateTime lastUnLoadTime;
        public DateTime finishTime;//不用急着删除，可以延时10秒钟

        public bool CanDeleteFinish()
        {
            if (!IsFinish) return false;
            if (DateTime.Now.Subtract(finishTime).TotalSeconds > 10)
            {
                return true;
            }
            return false;
        }


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
            _mTask = new WCS_NDC_TASK();

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
            _mTask.NDCLOADSITE = message.CarrierStation != 0 ? message.CarrierStation + "" : "";
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
}
