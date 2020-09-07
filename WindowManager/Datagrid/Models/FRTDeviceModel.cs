using Module;
using WcsManager.DevModule;
using System;
using System.ComponentModel;

using ADS = WcsManager.Administartor;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 固定辊台设备信息
    /// </summary>
    [Serializable]
    public class FRTDeviceModel : BaseDataGrid
    {
        private bool isconnect;
        private bool isUseful;
        private ActionEnum actionsta;
        private DeviceEnum devicesta;
        private CommandEnum commandsta;
        private TaskEnum currenttask;
        private TaskEnum finishtask;
        private GoodsEnum goodstatus;
        private RollerStatusEnum rollersta;
        private RollerDiretionEnum rollerdir;
        private int errormsg;
        private string datetime;

        private bool isScan1;
        private bool isScan2;


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

        [DisplayName("扫到码1")]
        public bool IsScan1
        {
            get { return isScan1; }
            set
            {
                isScan1 = value;
                OnPropertyChanged("IsScan1");
            }
        }

        [DisplayName("扫到码2")]
        public bool IsScan2
        {
            get { return isScan2; }
            set
            {
                isScan2 = value;
                OnPropertyChanged("IsScan2");
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

        [DisplayName("辊台状态")]
        public RollerStatusEnum RollerStatus
        {
            get { return rollersta; }
            set
            {
                rollersta = value;
                OnPropertyChanged("RollerStatus");
            }
        }

        [DisplayName("辊台方向")]
        public RollerDiretionEnum RollerDiretion
        {
            get { return rollerdir; }
            set
            {
                rollerdir = value;
                OnPropertyChanged("RollerDiretion");
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

            DevInfoFRT frt = ADS.mFrt.devices.Find(c => c.devName == DevName);

            if (isconnect != con)
            {
                IsConnected = con;
            }

            if (isUseful != frt.isUseful)
            {
                IsUseful = frt.isUseful;
            }

            if (!con) return;

            if (actionsta != frt._.ActionStatus)
            {
                ActionStatus = frt._.ActionStatus;
            }

            if (devicesta != frt._.DeviceStatus)
            {
                DeviceStatus = frt._.DeviceStatus;
            }

            if (commandsta != frt._.CommandStatus)
            {
                CommandStatus = frt._.CommandStatus;
            }

            if (currenttask != frt._.CurrentTask)
            {
                CurrentTask = frt._.CurrentTask;
            }

            if (finishtask != frt._.FinishTask)
            {

                FinishTask = frt._.FinishTask;
            }

            if (rollersta != frt._.RollerStatus)
            {
                RollerStatus = frt._.RollerStatus;
            }

            if (rollerdir != frt._.RollerDiretion)
            {
                RollerDiretion = frt._.RollerDiretion;
            }

            if (goodstatus != frt._.GoodsStatus)
            {
                GoodsStatus = frt._.GoodsStatus;
            }

            if (errormsg != frt._.ErrorMessage)
            {
                ErrorMessage = frt._.ErrorMessage;
            }

            IsScan1 = !string.IsNullOrEmpty(frt.lockID1);

            IsScan2 = !string.IsNullOrEmpty(frt.lockID2);

            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        public FRTDeviceModel(string dev,string area)
        {
            DevName = dev;
            Area = area;
            Update();
        }
    }
}
