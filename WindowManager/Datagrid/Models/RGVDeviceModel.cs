using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Devices;

namespace WindowManager.Datagrid.Models
{
    /// <summary>
    /// 行车设备信息
    /// </summary>
    [Serializable]
    public class RGVDeviceModel : BaseDataGrid
    {
        private string deviceid;
        private byte actionsta;
        private byte devicesta;
        private byte commandsta;
        private long des_x_y_z;
        private byte now_task;
        private long now_x_y_z;
        private byte finish_task;
        private byte loadstatus;
        private byte rollerstatus;
        private byte rollerdirection;
        private byte errormsg;
        private bool isconnect;
        private string datatime;
        private RGV rgv;



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
            get { return actionsta == RGV.Stop ? "停止" : "运行中"; }
            set
            {
                OnPropertyChanged("ActionStatus");
            }
        }

        [DataGridColumn("设备状态")]
        public string DeviceStatus
        {
            get { return devicesta == RGV.DeviceError ? "故障" : "正常"; }
            set
            {
                OnPropertyChanged("DeviceStatus");
            }
        }

        [DataGridColumn("命令状态")]
        public string CommandStatus
        {
            get { return commandsta == RGV.CommandError ? "命令异常" : "命令正常"; }
            set
            {
                OnPropertyChanged("CommandStatus");
            }
        }


        [DataGridColumn("目标坐标")]
        public long Des_X_Y_Z
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
        public long Now_X_Y_Z
        {
            get { return now_x_y_z; }
            set
            {
                now_x_y_z = value;
                OnPropertyChanged("Now_X_Y_Z");
            }
        }

        [DataGridColumn("棍台状态")]
        public string RollerStatus
        {
            get { return RGV.GetRollerStatusMes(rollerstatus); }
            set
            {
                OnPropertyChanged("RollerStatus");
            }
        }

        [DataGridColumn("棍台方向")]
        public string RollerDirection
        {
            get { return rollerdirection == RGV.RunFront ? "正向启动" : "反向启动"; }
            set
            {
                OnPropertyChanged("RollerDirection");
            }
        }


        [DataGridColumn("完成任务")]
        public string Finish_Task
        {
            get { return RGV.GetTaskMes(finish_task); }
            set
            {
                OnPropertyChanged("Finish_Task");
            }
        }

        [DataGridColumn("货物状态")]
        public string LoadStatus
        {
            get { return RGV.GetGoodsStatusMes(loadstatus); }
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
            if (isconnect != rgv.IsAlive())
            {
                ISConnect = rgv.IsAlive();
            }

            if (!rgv.IsAlive()) return;

            if (actionsta != rgv.ActionStatus())
            {
                actionsta = rgv.ActionStatus();
                ActionStatus = "";
            }

            if(devicesta != rgv.DeviceStatus())
            {
                devicesta = rgv.DeviceStatus();
                DeviceStatus = "";
            }

            if(commandsta != rgv.CommandStatus())
            {
                commandsta = rgv.CommandStatus();
                CommandStatus = "";
            }

            if(des_x_y_z != rgv.GetGoodsSite())
            {
                Des_X_Y_Z = rgv.GetGoodsSite();
            }

            if (now_task != rgv.CurrentTask())
            {
                now_task = rgv.CurrentTask();
                Now_Task = "";
            }

            if(now_x_y_z != rgv.GetCurrentSite())
            {
                Now_X_Y_Z = rgv.GetCurrentSite();
            }

            if(finish_task != rgv.FinishTask())
            {

                finish_task = rgv.FinishTask();
                Finish_Task = "";
            }

            if(loadstatus != rgv.GoodsStatus())
            {
                loadstatus = rgv.GoodsStatus();
                LoadStatus = "";
            }

            if(rollerstatus != rgv.CurrentStatus())
            {
                rollerstatus = rgv.CurrentStatus();
                RollerStatus = "";
            }

            if(rollerdirection != rgv.RunDirection())
            {
                rollerdirection = rgv.RunDirection();
                RollerDirection = "";
            }

            if(errormsg != rgv.ErrorMessage())
            {
                errormsg = rgv.ErrorMessage();
                ErrorMsg = "";
            }

            if(rgv.GetUpdateTime(out string time))
            {
                if(datatime != time)
                {
                    DataTime = time;
                }
            }
        }

        public RGVDeviceModel(string devid,string area)
        {
            rgv = new RGV(devid);
            DeviceID = devid;
            Area = area;
            Update();
        }
    }
}
