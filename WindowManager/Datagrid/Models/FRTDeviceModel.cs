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
    public class FRTDeviceModel : BaseDataGrid
    {
        private string deviceid;
        private byte actionsta;
        private byte devicesta;
        private byte commandsta;
        private byte now_task;
        private byte finish_task;
        private byte loadstatus;
        private byte rollerstatus;
        private byte rollerdirection;
        private byte errormsg;
        private bool isconnect;
        private string datatime;
        private FRT frt;



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
            get { return actionsta == FRT.Stop ? "停止" : "运行中"; }
            set
            {
                OnPropertyChanged("ActionStatus");
            }
        }

        [DataGridColumn("设备状态")]
        public string DeviceStatus
        {
            get { return devicesta == FRT.DeviceError ? "故障" : "正常"; }
            set
            {
                OnPropertyChanged("DeviceStatus");
            }
        }

        [DataGridColumn("命令状态")]
        public string CommandStatus
        {
            get { return commandsta == FRT.CommandError ? "命令异常" : "命令正常"; }
            set
            {
                OnPropertyChanged("CommandStatus");
            }
        }


        [DataGridColumn("当前任务")]
        public string Now_Task
        {
            get { return now_task == FRT.TaskTake ? "辊台任务" : "停止辊台任务"; }
            set
            {
                OnPropertyChanged("Now_Task");
            }
        }

        [DataGridColumn("棍台状态")]
        public string RollerStatus
        {
            get { return FRT.GetRollerStatusMes(rollerstatus); }
            set
            {
                OnPropertyChanged("RollerStatus");
            }
        }

        [DataGridColumn("棍台方向")]
        public string RollerDirection
        {
            get { return rollerdirection == FRT.RunFront ? "正向启动" : "反向启动"; }
            set
            {
                OnPropertyChanged("RollerDirection");
            }
        }


        [DataGridColumn("完成任务")]
        public string Finish_Task
        {
            get { return finish_task == FRT.TaskTake ? "辊台任务" : "停止辊台任务"; }
            set
            {
                OnPropertyChanged("Finish_Task");
            }
        }

        [DataGridColumn("货物状态")]
        public string LoadStatus
        {
            get { return FRT.GetGoodsStatusMes(loadstatus); }
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
            if (isconnect != frt.IsAlive())
            {
                ISConnect = frt.IsAlive();
            }

            if (!frt.IsAlive()) return;

            if (actionsta != frt.ActionStatus())
            {
                actionsta = frt.ActionStatus();
                ActionStatus = "";
            }

            if(devicesta != frt.DeviceStatus())
            {
                devicesta = frt.DeviceStatus();
                DeviceStatus = "";
            }

            if(commandsta != frt.CommandStatus())
            {
                commandsta = frt.CommandStatus();
                CommandStatus = "";
            }

            if (now_task != frt.CurrentTask())
            {
                now_task = frt.CurrentTask();
                Now_Task = "";
            }

            if(finish_task != frt.FinishTask())
            {

                finish_task = frt.FinishTask();
                Finish_Task = "";
            }

            if(loadstatus != frt.GoodsStatus())
            {
                loadstatus = frt.GoodsStatus();
                LoadStatus = "";
            }

            if(rollerstatus != frt.CurrentStatus())
            {
                rollerstatus = frt.CurrentStatus();
                RollerStatus = "";
            }

            if(rollerdirection != frt.RunDirection())
            {
                rollerdirection = frt.RunDirection();
                RollerDirection = "";
            }

            if(errormsg != frt.ErrorMessage())
            {
                errormsg = frt.ErrorMessage();
                ErrorMsg = "";
            }

            if(frt.GetUpdateTime(out string time))
            {
                if(datatime != time)
                {
                    DataTime = time;
                }
            }
        }

        public FRTDeviceModel(string devid,string area)
        {
            frt = new FRT(devid);
            DeviceID = devid;
            Area = area;
            Update();
        }
    }
}
