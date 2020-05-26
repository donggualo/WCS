using Module;
using Module.DEV;
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

        [DisplayName("棍台状态")]
        public RollerStatusEnum RollerStatus
        {
            get { return rollersta; }
            set
            {
                rollersta = value;
                OnPropertyChanged("RollerStatus");
            }
        }

        [DisplayName("棍台方向")]
        public RollerDiretionEnum RollerDiretion
        {
            get { return rollerdir; }
            set
            {
                rollerdir = value;
                OnPropertyChanged("RollerDirection");
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

            DeviceFRT frt = ADS.mFrt.devices.Find(c => c.devName == DevName)._;

            if (isconnect != con)
            {
                IsConnected = con;
            }

            if (!con) return;

            if (actionsta != frt.ActionStatus)
            {
                ActionStatus = frt.ActionStatus;
            }

            if (devicesta != frt.DeviceStatus)
            {
                DeviceStatus = frt.DeviceStatus;
            }

            if (commandsta != frt.CommandStatus)
            {
                CommandStatus = frt.CommandStatus;
            }

            if (currenttask != frt.CurrentTask)
            {
                CurrentTask = frt.CurrentTask;
            }

            if (finishtask != frt.FinishTask)
            {

                FinishTask = frt.FinishTask;
            }

            if (rollersta != frt.RollerStatus)
            {
                RollerStatus = frt.RollerStatus;
            }

            if (rollerdir != frt.RollerDiretion)
            {
                RollerDiretion = frt.RollerDiretion;
            }

            if (goodstatus != frt.GoodsStatus)
            {
                GoodsStatus = frt.GoodsStatus;
            }

            if (errormsg != frt.ErrorMessage)
            {
                ErrorMessage = frt.ErrorMessage;
            }

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
