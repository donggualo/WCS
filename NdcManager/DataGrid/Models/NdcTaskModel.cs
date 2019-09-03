using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NdcManager.DataGrid.Models
{
    [Serializable]
    public class NdcTaskModel
    {
        [DataGridColumn("ID")]
        public int TaskID { get; set; }

        [DataGridColumn("IKey")]
        public int IKey { get; set; }

        [DataGridColumn("Order")]
        public int Order { get; set; }

        [DataGridColumn("AGV")]
        public string AgvName { get; set; }

        [DataGridColumn("接货点")]
        public string LoadSite { get; set; }

        [DataGridColumn("卸货点")]
        public string UnLoadSite { get; set; }

        [DataGridColumn("重定向")]
        public string RedirectSite { get; set; }

        [DataGridColumn("接货")]
        public bool HasLoad { get; set; }

        [DataGridColumn("接货")]
        public bool HasUnLoad { get; set; }

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
            AgvName = item.CarrierId + "";
            LoadSite = item.LoadStation;
            UnLoadSite = item.UnloadStation;
            RedirectSite = item.RedirectUnloadStation;
            HasLoad = item.HadLoad;
            HasUnLoad = item.HadUnload;
        }
    }
}
