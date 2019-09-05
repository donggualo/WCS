using ModuleManager.NDC;
using NdcManager.DataGrid.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NdcManager.DataGrid
{
    /// <summary>
    /// 数据用于展示界面显示任务数据
    /// </summary>
    public class NdcAgvDataGrid
    {

        private ObservableCollection<NdcTaskModel> _mTaskList = new ObservableCollection<NdcTaskModel>();

        public ObservableCollection<NdcTaskModel> NdcTaskDataList
        {
            set
            {
                _mTaskList = value;
            }
            get
            {
                return _mTaskList;
            }
        }

        public void UpdateTaskInList(NDCItem item)
        {
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == item.IKey && c.Order == item.OrderIndex; });

            if (m != null && m.IKey != 0)
            {
                m.Update(item);
            }
            else if (item.IKey != 0 || item.OrderIndex != 0)
            {
                _mTaskList.Add(new NdcTaskModel(item));
            }
        }

        public void DeleteTask(NDCItem model)
        {
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == model.IKey && c.Order == model.OrderIndex; });
            if (m != null && m.IKey != 0)
            {
                _mTaskList.Remove(m);
            }
        }
    }
}
