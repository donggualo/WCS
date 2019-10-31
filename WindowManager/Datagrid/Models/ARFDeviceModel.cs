using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
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
    /// 摆渡车设备信息
    /// </summary>
    [Serializable]
    public class ARFDeviceModel : BaseDataGrid
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
        private ARF arf;



        [DisplayName("设备号")]
        public string DeviceID
        {
            get { return deviceid; }
            set
            {
                deviceid = value;
                OnPropertyChanged("DeviceID");
            }
        }

        [DisplayName("区域")]
        public string Area { set; get; }


        [DisplayName("运动状态")]
        public string ActionStatus
        {
            get { return actionsta == ARF.Stop ? "停止" : "运行中"; }
            set
            {
                OnPropertyChanged("ActionStatus");
            }
        }

        [DisplayName("设备状态")]
        public string DeviceStatus
        {
            get { return devicesta == ARF.DeviceError ? "故障" : "正常"; }
            set
            {
                OnPropertyChanged("DeviceStatus");
            }
        }

        [DisplayName("命令状态")]
        public string CommandStatus
        {
            get { return commandsta == ARF.CommandError ? "命令异常" : "命令正常"; }
            set
            {
                OnPropertyChanged("CommandStatus");
            }
        }


        [DisplayName("目标坐标")]
        public long Des_X_Y_Z
        {
            get { return des_x_y_z; }
            set
            {
                des_x_y_z = value;
                OnPropertyChanged("Des_X_Y_Z");
            }
        }


        [DisplayName("当前任务")]
        public string Now_Task
        {
            get { return ARF.GetTaskMes(now_task); }
            set
            {
                OnPropertyChanged("Now_Task");
            }
        }


        [DisplayName("当前坐标")]
        public long Now_X_Y_Z
        {
            get { return now_x_y_z; }
            set
            {
                now_x_y_z = value;
                OnPropertyChanged("Now_X_Y_Z");
            }
        }

        [DisplayName("棍台状态")]
        public string RollerStatus
        {
            get { return ARF.GetRollerStatusMes(rollerstatus); }
            set
            {
                OnPropertyChanged("RollerStatus");
            }
        }

        [DisplayName("棍台方向")]
        public string RollerDirection
        {
            get { return rollerdirection == ARF.RunFront ? "正向启动" : "反向启动"; }
            set
            {
                OnPropertyChanged("RollerDirection");
            }
        }


        [DisplayName("完成任务")]
        public string Finish_Task
        {
            get { return ARF.GetTaskMes(finish_task); }
            set
            {
                OnPropertyChanged("Finish_Task");
            }
        }

        [DisplayName("货物状态")]
        public string LoadStatus
        {
            get { return ARF.GetGoodsStatusMes(loadstatus); }
            set
            {
                OnPropertyChanged("LoadStatus");
            }
        }

        [DisplayName("故障信息")]
        public string ErrorMsg
        {
            get { return errormsg.ToString("X2"); }
            set
            {
                OnPropertyChanged("ErrorMsg");
            }
        }

        [DisplayName("连接")]
        public bool ISConnect
        {
            get { return isconnect; }
            set
            {
                isconnect = value;
                OnPropertyChanged("ISConnect");
            }
        }

        [DisplayName("刷新时间")]
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
            if (isconnect != arf.IsAlive())
            {
                ISConnect = arf.IsAlive();
            }

            if (!arf.IsAlive()) return;

            if (actionsta != arf.ActionStatus())
            {
                actionsta = arf.ActionStatus();
                ActionStatus = "";
            }

            if (devicesta != arf.DeviceStatus())
            {
                devicesta = arf.DeviceStatus();
                DeviceStatus = "";
            }

            if (commandsta != arf.CommandStatus())
            {
                commandsta = arf.CommandStatus();
                CommandStatus = "";
            }

            if (des_x_y_z != arf.Goods1site())
            {
                Des_X_Y_Z = arf.Goods1site();
            }

            if (now_task != arf.CurrentTask())
            {
                now_task = arf.CurrentTask();
                Now_Task = "";
            }

            if (now_x_y_z != arf.CurrentSite())
            {
                Now_X_Y_Z = arf.CurrentSite();
            }

            if (finish_task != arf.FinishTask())
            {

                finish_task = arf.FinishTask();
                Finish_Task = "";
            }

            if (loadstatus != arf.GoodsStatus())
            {
                loadstatus = arf.GoodsStatus();
                LoadStatus = "";
            }

            if (rollerstatus != arf.CurrentStatus())
            {
                rollerstatus = arf.CurrentStatus();
                RollerStatus = "";
            }

            if (rollerdirection != arf.RunDirection())
            {
                rollerdirection = arf.RunDirection();
                RollerDirection = "";
            }

            if (errormsg != arf.ErrorMessage())
            {
                errormsg = arf.ErrorMessage();
                ErrorMsg = "";
            }

            if (arf.GetUpdateTime(out string time))
            {
                if (datatime != time)
                {
                    DataTime = time;
                }
            }
        }

        public ARFDeviceModel(string devid, string area)
        {
            arf = new ARF(devid);
            DeviceID = devid;
            Area = area;
            Update();
        }
    }
}
