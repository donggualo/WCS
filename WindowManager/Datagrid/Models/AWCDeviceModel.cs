using Module;
using Module.DEV;
using System;
using System.ComponentModel;

using ADS = WcsManager.Administartor;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 行车设备信息
    /// </summary>
    [Serializable]
    public class AWCDeviceModel : BaseDataGrid
    {
        private bool isconnect;
        private ActionEnum actionsta;
        private DeviceEnum devicesta;
        private CommandEnum commandsta;
        private AwcTaskEnum currenttask;
        private AwcTaskEnum finishtask;
        private AwcGoodsEnum goodstatus;
        private int errormsg;
        private int x;
        private int y;
        private int z;
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
        public AwcTaskEnum CurrentTask
        {
            get { return currenttask; }
            set
            {
                currenttask = value;
                OnPropertyChanged("CurrentTask");
            }
        }

        [DisplayName("当前X轴坐标")]
        public int CurrentSiteX
        {
            get { return x; }
            set
            {
                x = value;
                OnPropertyChanged("CurrentSiteX");
            }
        }

        [DisplayName("当前Y轴坐标")]
        public int CurrentSiteY
        {
            get { return y; }
            set
            {
                y = value;
                OnPropertyChanged("CurrentSiteY");
            }
        }

        [DisplayName("当前Z轴坐标")]
        public int CurrentSiteZ
        {
            get { return z; }
            set
            {
                z = value;
                OnPropertyChanged("CurrentSiteZ");
            }
        }

        [DisplayName("完成任务")]
        public AwcTaskEnum FinishTask
        {
            get { return finishtask; }
            set
            {
                finishtask = value;
                OnPropertyChanged("FinishTask");
            }
        }

        [DisplayName("货物状态")]
        public AwcGoodsEnum GoodsStatus
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

            if (isconnect != con)
            {
                IsConnected = con;
            }

            if (!con) return;

            DeviceAWC awc = ADS.mAwc.devices.Find(c => c.devName == DevName)._;

            if (actionsta != awc.ActionStatus)
            {
                ActionStatus = awc.ActionStatus;
            }

            if(devicesta != awc.DeviceStatus)
            {
                DeviceStatus = awc.DeviceStatus;
            }

            if(commandsta != awc.CommandStatus)
            {
                CommandStatus = awc.CommandStatus;
            }

            if (currenttask != awc.CurrentTask)
            {
                CurrentTask = awc.CurrentTask;
            }

            if(finishtask != awc.FinishTask)
            {

                FinishTask = awc.FinishTask;
            }

            if (x != awc.CurrentSiteX)
            {

                CurrentSiteX = awc.CurrentSiteX;
            }

            if (y != awc.CurrentSiteY)
            {

                CurrentSiteY = awc.CurrentSiteY;
            }

            if (z != awc.CurrentSiteZ)
            {

                CurrentSiteZ = awc.CurrentSiteZ;
            }

            if (goodstatus != awc.GoodsStatus)
            {
                GoodsStatus = awc.GoodsStatus;
            }

            if(errormsg != awc.ErrorMessage)
            {
                ErrorMessage = awc.ErrorMessage;
            }

            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        }

        public AWCDeviceModel(string dev,string area)
        {
            DevName = dev;
            Area = area;
            Update();
        }
    }
}
