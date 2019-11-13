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
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == item._mTask.IKEY && c.Order == item._mTask.NDCINDEX; });

            if (m != null && m.IKey != 0)
            {
                m.Update(item);
            }
            else if (item._mTask.IKEY != 0 || item._mTask.NDCINDEX != 0)
            {
                _mTaskList.Add(new NdcTaskModel(item));
            }
        }

        public void DeleteTask(NDCItem model)
        {
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == model._mTask.IKEY && c.Order == model._mTask.NDCINDEX; });
            if (m != null && m.IKey != 0)
            {
                _mTaskList.Remove(m);
            }
        }
    }
}
