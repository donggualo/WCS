using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridManager.Models
{
    /// <summary>
    /// 行车设备信息
    /// </summary>
    [Serializable]
    public class ABCDeviceModel : BaseDataGrid
    {
        private int deviceid;
        private int status;
        private string des_x_y_z;
        private int now_task;
        private string now_x_y_z;
        private int finish_task;
        private int loadstatus;
        private string errormsg;
        private bool isconnect;

        [DataGridColumn("设备号")]
        public int DeviceID {
            get { return deviceid; }
            set {
                deviceid = value;
                OnPropertyChanged("Name");
            }
        }
        [DataGridColumn("状态")]
        public int Status
        {
            get { return deviceid; }
            set
            {
                deviceid = value;
                OnPropertyChanged("Status");
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
        [DataGridColumn("单前任务")]
        public int Now_Task
        {
            get { return now_task; }
            set
            {
                now_task = value;
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
        public int Finish_Task
        {
            get { return finish_task; }
            set
            {
                finish_task = value;
                OnPropertyChanged("Finish_Task");
            }
        }
        [DataGridColumn("货位状态")]
        public int LoadStatus
        {
            get { return loadstatus; }
            set
            {
                loadstatus = value;
                OnPropertyChanged("LoadStatus");
            }
        }
        [DataGridColumn("故障信息")]
        public string ErrorMsg
        {
            get { return errormsg; }
            set
            {
                errormsg = value;
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

        public void Update(ABCDeviceModel m)
        {
            if (status != m.Status)
            {
                Status = m.Status;
            }

            if(des_x_y_z != m.Des_X_Y_Z)
            {
                Des_X_Y_Z = m.Des_X_Y_Z;
            }

            if(now_task != m.Now_Task)
            {
                Now_Task = m.Now_Task;
            }

            if(now_x_y_z != m.Now_X_Y_Z)
            {
                Now_X_Y_Z = m.Now_X_Y_Z;
            }

            if(finish_task != m.Finish_Task)
            {
                Finish_Task = m.Finish_Task;
            }

            if(loadstatus != m.loadstatus)
            {
                LoadStatus = m.LoadStatus;
            }

            if(errormsg != m.ErrorMsg)
            {
                ErrorMsg = m.ErrorMsg;
            }

            if (isconnect != m.ISConnect)
            {
                ISConnect = m.ISConnect;
            }
        }

        public ABCDeviceModel(int devid,int sta, int no)
        {
            deviceid = devid;
            status = sta;
            des_x_y_z = sta+":"+no;
            now_task = no;
            now_x_y_z = no + ":" + no;
            finish_task = no*2;
            loadstatus = sta*sta;
            errormsg = no + ":" + no;
            isconnect = sta % 2 == 0;
        }
    }
}
