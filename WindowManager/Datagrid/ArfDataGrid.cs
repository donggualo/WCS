using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowManager.Datagrid.Models;

namespace WindowManager.Datagrid
{
    public class ArfDataGrid
    {
        private ObservableCollection<ARFDeviceModel> _mDeviceList = new ObservableCollection<ARFDeviceModel>();
        private ObservableCollection<string> _mDeviceNameList = new ObservableCollection<string>();

        public ObservableCollection<ARFDeviceModel> DeviceList
        {
            set { _mDeviceList = value; }
            get
            {
                return _mDeviceList;
            }
        }

        public void UpdateDeviceList(string name = "", string area = "")
        {
            if (name == "")
            {//全刷新
                foreach (var d in _mDeviceList)
                {
                    d.Update();
                }
            }
            else
            {
                ARFDeviceModel m = _mDeviceList.FirstOrDefault(c => { return c.DeviceID == name; });
                if (m == null)
                {
                    _mDeviceList.Add(new ARFDeviceModel(name, area));
                    _mDeviceNameList.Add(name);
                }
            }
        }

        public ObservableCollection<string> DeviceNameList
        {
            set { _mDeviceNameList = value; }
            get
            {
                return _mDeviceNameList;
            }
        }

        public void UpdateDeviceNameList(string abc)
        {
            if (!_mDeviceNameList.Contains(abc))
            {
                _mDeviceNameList.Add(abc);
            }
        }


    }
}
