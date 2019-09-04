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

        public NdcAgvDataGrid() { }

        public ObservableCollection<NdcTaskModel> NdcTaskDataList
        {
            set
            {
                _mTaskList = value;
                //OnPropertyChanged("NdcTaskDataList");
            }
            get
            {
                return _mTaskList;
            }
        }

        //.Netformwork4.0
        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string strPropertyInfo)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(strPropertyInfo));
        //    }
        //}


        //.Netformwork4.5
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}


        public void UpdateTaskInList(NdcTaskModel model)
        {
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == model.IKey && c.Order == model.Order; });

            if (m != null && m.IKey != 0)
            {
                //m = model;
                m.TaskID = model.TaskID;
                m.IKey = model.IKey;
                m.Order = model.Order;
                m.AgvName = model.AgvName;
                m.LoadSite = model.LoadSite;
                m.UnLoadSite = model.UnLoadSite;
                m.RedirectSite = model.RedirectSite;
                m.HasLoad = model.HasLoad;
                m.HasUnLoad = model.HasUnLoad;
            }
            else if (model.IKey != 0 || model.Order != 0)
            {
                _mTaskList.Add(model);
            }
            //OnPropertyChanged("NdcTaskDataList");
        }

        public void DeleteTask(NdcTaskModel model)
        {
            NdcTaskModel m = _mTaskList.FirstOrDefault(c => { return c.IKey == model.IKey && c.Order == model.Order; });
            if (m != null && m.IKey != 0)
            {
                _mTaskList.Remove(m);
            }
        }
    }
}
