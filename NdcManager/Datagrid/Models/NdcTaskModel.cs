using ModuleManager.NDC;
using NdcManager;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NdcManager.DataGrid.Models
{
    [Serializable]
    public class NdcTaskModel : BaseDataGrid
    {
        private int taskid;
        private int ikey;
        private int order;
        private int agvname;
        private string loadsite;
        private string unloadsite;
        private string redirectsite;
        private bool hasload;
        private bool hasunload;

        [DataGridColumn("ID")]
        public int TaskID
        {
            get
            {
                return taskid;
            }
            set
            {
                taskid = value;
                OnPropertyChanged("TaskID");
            }
        }

        [DataGridColumn("IKey")]
        public int IKey
        {
            get
            {
                return ikey;
            }
            set
            {
                ikey = value;
                OnPropertyChanged("IKey");
            }
        }

        [DataGridColumn("Order")]
        public int Order {
            get {
                return order;
            }
            set
            {
                order = value;
                OnPropertyChanged("Order");
            }
        }


        [DataGridColumn("AGV")]
        public int AgvNames
        {
            get
            {
                return agvname;
            }
            set
            {
                agvname = value;
                OnPropertyChanged("AgvName");
            }
        }

        [DataGridColumn("接货点")]
        public string LoadSite
        {
            get
            {
                return loadsite;
            }
            set
            {
                loadsite = value;
                OnPropertyChanged("LoadSite");
            }
        }

        [DataGridColumn("卸货点")]
        public string UnLoadSite
        {
            get
            {
                return unloadsite;
            }
            set
            {
                unloadsite = value;
                OnPropertyChanged("UnLoadSite");
            }
        }

        [DataGridColumn("重定向")]
        public string RedirectSite
        {
            get
            {
                return redirectsite;
            }
            set
            {
                redirectsite = value;
                OnPropertyChanged("RedirectSite");
            }
        }

        [DataGridColumn("接货")]
        public bool HasLoad
        {
            get
            {
                return hasload;
            }
            set
            {
                hasload = value;
                OnPropertyChanged("HasLoad");
            }
        }

        [DataGridColumn("接货")]
        public bool HasUnLoad
        {
            get
            {
                return hasunload;
            }
            set
            {
                hasunload = value;
                OnPropertyChanged("HasUnLoad");
            }
        }

        public void Update(NDCItem item)
        {
            if (taskid != item.TaskID)
            {
                TaskID = item.TaskID;
            }
            if (ikey != item.IKey)
            {
                IKey = item.IKey;
            }
            if (order != item.OrderIndex)
            {
                Order = item.OrderIndex;
            }
            if (agvname != item.CarrierId )
            {
                agvname = item.CarrierId;
            }
            if (loadsite != item.LoadStation)
            {
                LoadSite = item.LoadStation;
            }
            if (unloadsite != item.UnloadStation)
            {
                UnLoadSite = item.UnloadStation;
            }
            if (redirectsite != item.RedirectUnloadStation)
            {
                RedirectSite = item.RedirectUnloadStation;
            }
            if (hasload = item.HadLoad)
            {
                HasLoad = item.HadLoad;
            }
            if (hasunload = item.HadUnload)
            {
                HasUnLoad = item.HadUnload;
            }
        }

        public NdcTaskModel(TempItem item)
        {
            IKey = int.Parse(item.IKey);
            TaskID = item.TaskID;
            LoadSite = item.LoadStation;
            UnLoadSite = item.UnloadStation;
            RedirectSite = item.RedirectUnloadStation;
        }

        public NdcTaskModel(NDCItem item)
        {
            IKey = item.IKey;
            TaskID = item.TaskID;
            Order = item.OrderIndex;
            agvname = item.CarrierId;
            LoadSite = item.LoadStation;
            UnLoadSite = item.UnloadStation;
            RedirectSite = item.RedirectUnloadStation;
            HasLoad = item.HadLoad;
            HasUnLoad = item.HadUnload;
        }
    }
}
