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

        public int DeviceID {
            get { return deviceid; }
            set {
                deviceid = value;
                OnPropertyChanged("Name");
            }
        }
        public int Status
        {
            get { return deviceid; }
            set
            {
                deviceid = value;
                OnPropertyChanged("Status");
            }
        }
        public string Des_X_Y_Z
        {
            get { return des_x_y_z; }
            set
            {
                des_x_y_z = value;
                OnPropertyChanged("Des_X_Y_Z");
            }
        }
        public int Now_Task
        {
            get { return now_task; }
            set
            {
                now_task = value;
                OnPropertyChanged("Now_Task");
            }
        }
        public string Now_X_Y_Z
        {
            get { return now_x_y_z; }
            set
            {
                now_x_y_z = value;
                OnPropertyChanged("Now_X_Y_Z");
            }
        }
        public int Finish_Task
        {
            get { return finish_task; }
            set
            {
                finish_task = value;
                OnPropertyChanged("Finish_Task");
            }
        }
        public int LoadStatus
        {
            get { return loadstatus; }
            set
            {
                loadstatus = value;
                OnPropertyChanged("LoadStatus");
            }
        }
        public string ErrorMsg
        {
            get { return errormsg; }
            set
            {
                errormsg = value;
                OnPropertyChanged("ErrorMsg");
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
        }

        public ABCDeviceModel()
        {
            deviceid = 1;
            status = 2;
            des_x_y_z = "29,32,44";
            now_task = 2;
            now_x_y_z = "23,34,55";
            finish_task = 3;
            loadstatus = 1;
            errormsg = "sdklksdf";
        }
    }
}
