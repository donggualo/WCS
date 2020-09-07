using Module;
using WcsManager.DevModule;
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
        private bool isUseful;
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

        private string wmsloc;

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

        [DisplayName("作业坐标")]
        public string WMSloc
        {
            get { return wmsloc; }
            set
            {
                wmsloc = value;
                OnPropertyChanged("WMSloc");
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

            DevInfoAWC awc = ADS.mAwc.devices.Find(c => c.devName == DevName);

            if (isconnect != con)
            {
                IsConnected = con;
            }

            if (isUseful != awc.isUseful)
            {
                IsUseful = awc.isUseful;
            }

            if (!con) return;

            if (actionsta != awc._.ActionStatus)
            {
                ActionStatus = awc._.ActionStatus;
            }

            if(devicesta != awc._.DeviceStatus)
            {
                DeviceStatus = awc._.DeviceStatus;
            }

            if(commandsta != awc._.CommandStatus)
            {
                CommandStatus = awc._.CommandStatus;
            }

            if (currenttask != awc._.CurrentTask)
            {
                CurrentTask = awc._.CurrentTask;
            }

            if(finishtask != awc._.FinishTask)
            {

                FinishTask = awc._.FinishTask;
            }

            if (x != awc._.CurrentSiteX)
            {

                CurrentSiteX = awc._.CurrentSiteX;
            }

            if (y != awc._.CurrentSiteY)
            {

                CurrentSiteY = awc._.CurrentSiteY;
            }

            if (z != awc._.CurrentSiteZ)
            {

                CurrentSiteZ = awc._.CurrentSiteZ;
            }

            if (goodstatus != awc._.GoodsStatus)
            {
                GoodsStatus = awc._.GoodsStatus;
            }

            if(errormsg != awc._.ErrorMessage)
            {
                ErrorMessage = awc._.ErrorMessage;
            }

            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            if (wmsloc != awc.lockLocWMS)
            {
                WMSloc = awc.lockLocWMS;
            }

        }

        public AWCDeviceModel(string dev,string area)
        {
            DevName = dev;
            Area = area;
            Update();
        }
    }
}
