using Panuon.UI.Silver;
using System;
using TaskManager.Devices;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 行车设备信息
    /// </summary>
    [Serializable]
    public class ABCDeviceModel : BaseDataGrid
    {
        private string deviceid;
        private byte actionsta;
        private byte devicesta;
        private byte commandsta;
        private string des_x_y_z;
        private byte now_task;
        private string now_x_y_z;
        private byte finish_task;
        private byte loadstatus;
        private byte errormsg;
        private bool isconnect;
        private string datatime;
        private ABC abc;



        [DataGridColumn("设备号")]
        public string DeviceID {
            get { return deviceid; }
            set {
                deviceid = value;
                OnPropertyChanged("DeviceID");
            }
        }

        [DataGridColumn("区域")]
        public string Area{ set; get; }


        [DataGridColumn("运动状态")]
        public string ActionStatus
        {
            get { return actionsta == ABC.Stop ? "停止" : "运行中"; }
            set
            {
                OnPropertyChanged("ActionStatus");
            }
        }

        [DataGridColumn("设备状态")]
        public string DeviceStatus
        {
            get { return devicesta == ABC.DeviceError ? "故障" : "正常"; }
            set
            {
                OnPropertyChanged("DeviceStatus");
            }
        }

        [DataGridColumn("命令状态")]
        public string CommandStatus
        {
            get { return commandsta == ABC.CommandError ? "命令异常" : "命令正常"; }
            set
            {
                OnPropertyChanged("CommandStatus");
            }
        }


        [DataGridColumn("目标坐标")]
        public string Des_X_Y_Z
        {
            get { return des_x_y_z; }
            set
            {
                des_x_y_z = value;
                OnPropertyChanged("Des_X_Y_Z");
            }
        }


        [DataGridColumn("当前任务")]
        public string Now_Task
        {
            get { return ABC.GetTaskMes(now_task); }
            set
            {
                OnPropertyChanged("Now_Task");
            }
        }


        [DataGridColumn("当前坐标")]
        public string Now_X_Y_Z
        {
            get { return now_x_y_z; }
            set
            {
                now_x_y_z = value;
                OnPropertyChanged("Now_X_Y_Z");
            }
        }


        [DataGridColumn("完成任务")]
        public string Finish_Task
        {
            get { return ABC.GetTaskMes(finish_task); }
            set
            {
                OnPropertyChanged("Finish_Task");
            }
        }


        [DataGridColumn("货物状态")]
        public string LoadStatus
        {
            get { return loadstatus == ABC.GoodsNo ? "无货" : "有货"; }
            set
            {
                OnPropertyChanged("LoadStatus");
            }
        }


        [DataGridColumn("故障信息")]
        public string ErrorMsg
        {
            get { return errormsg.ToString("X2"); }
            set
            {
                OnPropertyChanged("ErrorMsg");
            }
        }

        [DataGridColumn("连接")]
        public bool ISConnect
        {
            get { return isconnect; }
            set
            {
                isconnect = value;
                OnPropertyChanged("ISConnect");
            }
        }

        [DataGridColumn("刷新时间")]
        public string DataTime
        {
            get { return datatime; }
            set
            {
                datatime = value;
                OnPropertyChanged("DataTime");
            }
        }



        public void Update()
        {
            if (isconnect != abc.IsAlive())
            {
                ISConnect = abc.IsAlive();
            }

            if (!abc.IsAlive()) return;

            if (actionsta != abc.ActionStatus())
            {
                actionsta = abc.ActionStatus();
                ActionStatus = "";
            }

            if(devicesta != abc.DeviceStatus())
            {
                devicesta = abc.DeviceStatus();
                DeviceStatus = "";
            }

            if(commandsta != abc.CommandStatus())
            {
                commandsta = abc.CommandStatus();
                CommandStatus = "";
            }

            if(des_x_y_z != abc.GetGoodsSite())
            {
                Des_X_Y_Z = abc.GetGoodsSite();
            }

            if (now_task != abc.CurrentTask())
            {
                now_task = abc.CurrentTask();
                Now_Task = "";
            }

            if(now_x_y_z != abc.GetCurrentSite())
            {
                Now_X_Y_Z = abc.GetCurrentSite();
            }

            if(finish_task != abc.FinishTask())
            {

                finish_task = abc.FinishTask();
                Finish_Task = "";
            }

            if(loadstatus != abc.GoodsStatus())
            {
                loadstatus = abc.GoodsStatus();
                LoadStatus = "";
            }

            if(errormsg != abc.ErrorMessage())
            {
                errormsg = abc.ErrorMessage();
                ErrorMsg = "";
            }

            if(abc.GetUpdateTime(out string time))
            {
                if(datatime != time)
                {
                    DataTime = time;
                }
            }
        }

        public ABCDeviceModel(string devid,string area)
        {
            abc = new ABC(devid);
            DeviceID = devid;
            Area = area;
            Update();
        }
    }
}
