using Module;
using Module.DEV;
using System;
using System.ComponentModel;

using ADS = WcsManager.Administartor;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 运输车设备信息
    /// </summary>
    [Serializable]
    public class RGVDeviceModel : BaseDataGrid
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
        private int site;
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

        [DisplayName("当前坐标")]
        public int CurrentSite
        {
            get { return site; }
            set
            {
                site = value;
                OnPropertyChanged("CurrentSite");
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

            DeviceRGV rgv = ADS.mRgv.devices.Find(c => c.devName == DevName)._;

            if (isconnect != con)
            {
                IsConnected = con;
            }

            if (!con) return;

            if (actionsta != rgv.ActionStatus)
            {
                ActionStatus = rgv.ActionStatus;
            }

            if (devicesta != rgv.DeviceStatus)
            {
                DeviceStatus = rgv.DeviceStatus;
            }

            if (commandsta != rgv.CommandStatus)
            {
                CommandStatus = rgv.CommandStatus;
            }

            if (currenttask != rgv.CurrentTask)
            {
                CurrentTask = rgv.CurrentTask;
            }

            if (finishtask != rgv.FinishTask)
            {

                FinishTask = rgv.FinishTask;
            }

            if (site != rgv.CurrentSite)
            {

                CurrentSite = rgv.CurrentSite;
            }

            if (rollersta != rgv.RollerStatus)
            {
                RollerStatus = rgv.RollerStatus;
            }

            if (rollerdir != rgv.RollerDiretion)
            {
                RollerDiretion = rgv.RollerDiretion;
            }

            if (goodstatus != rgv.GoodsStatus)
            {
                GoodsStatus = rgv.GoodsStatus;
            }

            if (errormsg != rgv.ErrorMessage)
            {
                ErrorMessage = rgv.ErrorMessage;
            }

            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        public RGVDeviceModel(string dev,string area)
        {
            DevName = dev;
            Area = area;
            Update();
        }
    }
}
