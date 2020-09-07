using Module;
using WcsManager.DevModule;
using System;
using System.ComponentModel;

using ADS = WcsManager.Administartor;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 包装线设备信息
    /// </summary>
    [Serializable]
    public class PKLDeviceModel : BaseDataGrid
    {
        private bool isconnect;
        private bool isUseful;
        private ActionEnum actionsta;
        private DeviceEnum devicesta;
        private CommandEnum commandsta;
        private TaskEnum currenttask;
        private TaskEnum finishtask;
        private GoodsEnum goodstatus;
        private int errormsg;
        private string datetime;

        private bool isScan;


        #region info

        [DisplayName("连接")]
        public bool IsConnected
        {
            get { return isconnect; }
            set
            {
                isconnect = value;
                OnPropertyChanged("IsConnected");
            }
        }

        [DisplayName("启用")]
        public bool IsUseful
        {
            get { return isUseful; }
            set
            {
                isUseful = value;
                OnPropertyChanged("IsUseful");
            }
        }

        [DisplayName("设备名")]
        public string DevName { set; get; }

        [DisplayName("区域")]
        public string Area { set; get; }

        [DisplayName("运行状态")]
        public ActionEnum ActionStatus
        {
            get { return actionsta; }
            set
            {
                actionsta = value;
                OnPropertyChanged("ActionStatus");
            }
        }

        [DisplayName("设备状态")]
        public DeviceEnum DeviceStatus
        {
            get { return devicesta; }
            set
            {
                devicesta = value;
                OnPropertyChanged("DeviceStatus");
            }
        }

        [DisplayName("命令状态")]
        public CommandEnum CommandStatus
        {
            get { return commandsta; }
            set
            {
                commandsta = value;
                OnPropertyChanged("CommandStatus");
            }
        }

        [DisplayName("扫到码")]
        public bool IsScan
        {
            get { return isScan; }
            set
            {
                isScan = value;
                OnPropertyChanged("IsScan");
            }
        }

        [DisplayName("当前任务")]
        public TaskEnum CurrentTask
        {
            get { return currenttask; }
            set
            {
                currenttask = value;
                OnPropertyChanged("CurrentTask");
            }
        }

        [DisplayName("完成任务")]
        public TaskEnum FinishTask
        {
            get { return finishtask; }
            set
            {
                finishtask = value;
                OnPropertyChanged("FinishTask");
            }
        }

        [DisplayName("货物状态")]
        public GoodsEnum GoodsStatus
        {
            get { return goodstatus; }
            set
            {
                goodstatus = value;
                OnPropertyChanged("GoodsStatus");
            }
        }

        [DisplayName("故障信息")]
        public int ErrorMessage
        {
            get { return errormsg; }
            set
            {
                errormsg = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        [DisplayName("更新时间")]
        public string UpdateTime
        {
            get { return datetime; }
            set
            {
                datetime = value;
                OnPropertyChanged("UpdateTime");
            }
        }

        #endregion


        public void Update()
        {
            bool con = ADS.mSocket.IsConnected(DevName);

            DevInfoPKL pkl = ADS.mPkl.devices.Find(c => c.devName == DevName);


            IsConnected = con;

            IsUseful = pkl.isUseful;

            if (!con) return;

            if (actionsta != pkl._.ActionStatus)
            {
                ActionStatus = pkl._.ActionStatus;
            }

            if (devicesta != pkl._.DeviceStatus)
            {
                DeviceStatus = pkl._.DeviceStatus;
            }

            if (commandsta != pkl._.CommandStatus)
            {
                CommandStatus = pkl._.CommandStatus;
            }

            if (currenttask != pkl._.CurrentTask)
            {
                CurrentTask = pkl._.CurrentTask;
            }

            if (finishtask != pkl._.FinishTask)
            {

                FinishTask = pkl._.FinishTask;
            }

            if (goodstatus != pkl._.GoodsStatus)
            {
                GoodsStatus = pkl._.GoodsStatus;
            }

            if (errormsg != pkl._.ErrorMessage)
            {
                ErrorMessage = pkl._.ErrorMessage;
            }


            IsScan = !string.IsNullOrEmpty(pkl.lockID2);

            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        public PKLDeviceModel(string dev, string area)
        {
            DevName = dev;
            Area = area;
            Update();
        }
    }
}
