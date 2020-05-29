using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowManager.Datagrid.Models;

namespace WindowManager.Datagrid
{
    class PklDataGrid
    {
        private ObservableCollection<PKLDeviceModel> _mDeviceList = new ObservableCollection<PKLDeviceModel>();
        private ObservableCollection<string> _mDeviceNameList = new ObservableCollection<string>();

        public ObservableCollection<PKLDeviceModel> DeviceList
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
                PKLDeviceModel m = _mDeviceList.FirstOrDefault(c => { return c.DevName == name; });
                if (m == null)
                {
                    _mDeviceList.Add(new PKLDeviceModel(name, area));
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

        public void UpdateDeviceNameList(string frt)
        {
            if (!_mDeviceNameList.Contains(frt))
            {
                _mDeviceNameList.Add(frt);
            }
        }


    }
}
