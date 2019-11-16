using ModuleManager.NDC.Message;
using ModuleManager.NDC.SQL;
using NDC8.ACINET.ACI;
using System;

namespace ModuleManager.NDC
{
    /// <summary>
    /// NDC调度信息类
    /// </summary>
    public class NDCItem
    {
        #region[参数定义]

        public WCS_NDC_TASK _mTask;

        public _sMessage s;
        public _bMessage b;
        public _vpilMessage v;


        public int Magic = -1 ;
        public int Status = -1;
        public bool IsFinish = false;

        /// <summary>
        /// 车重定位任务的状态
        /// </summary>
        public NDCItemStatus DirectStatus;
        public bool HadDirectInfo;
        /// <summary>
        /// true:主动取消
        /// false:被动取消
        /// </summary>
        public bool CancleFromSystem = false;

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

        #endregion

        #region[构造方法]
        
        /// <summary>
        /// 构造函数
        /// </summary>
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

        #endregion

        #region[更新数据]

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

        #endregion

        #region[其他方法]

        /// <summary>
        /// 完成后在前台清空数据（非删除后台数据）
        /// </summary>
        /// <returns></returns>
        public bool CanDeleteFinish()
        {
            if (_mTask.PAUSE) return false;
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

        /// <summary>
        /// 车故障后恢复任务
        /// </summary>
        /// <param name="index"></param>
        public void ReUseAfterCarWash(int index)
        {
            IsFinish = false;
            _mTask.NDCINDEX = index;
            _mTask.PAUSE = false;

        }
        
        /// <summary>
        /// 任务小车
        /// </summary>
        public int CARRIERID
        {
            get
            {
                return _mTask.CARRIERID;
            }
            set
            {
                if (_mTask.CARRIERID == 0)
                {
                    _mTask.CARRIERID = value;
                }
            }
        }

        /// <summary>
        /// 恢复初始状态，重新呼叫AGV
        /// </summary>
        /// <param name="newikey"></param>
        public void BeforeReCall(int newikey)
        {
            _mTask.IKEY = newikey;
            _mTask.NDCINDEX = -1;
            _mTask.CARRIERID = 0;
            _mTask.PAUSE = false;
            Magic = 0;
            Status = 0;
        }
        #endregion
    }
}
