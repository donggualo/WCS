using Module;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using WcsManager.DevModule;
using ADS = WcsManager.Administartor;

namespace TestWim
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Device : Window
    {
        public ObservableCollection<DevDataAWC> ListAWC { set; get; } = new ObservableCollection<DevDataAWC>();
        public ObservableCollection<DevDataRGV> ListRGV { set; get; } = new ObservableCollection<DevDataRGV>();
        public ObservableCollection<DevDataARF> ListARF { set; get; } = new ObservableCollection<DevDataARF>();
        public ObservableCollection<DevDataFRT> ListFRT { set; get; } = new ObservableCollection<DevDataFRT>();

        public Device()
        {
            InitializeComponent();

            new Thread(RefreshAll) { IsBackground = true }.Start();
        }

        private void RefreshAll()
        {
            while (true)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    RefreshAWC();
                    RefreshRGV();
                    RefreshARF();
                    RefreshFRT();
                }));

                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 获取行车数据
        /// </summary>
        private void RefreshAWC()
        {
            try
            {
                ListAWC.Clear();
                List<DevInfoAWC> list = ADS.mAwc.devices;
                if (list == null || list.Count == 0) return;

                foreach (DevInfoAWC item in list)
                {
                    ListAWC.Add(new DevDataAWC()
                    {
                        IsConnected = ADS.mSocket.IsConnected(item.devName),
                        DevName = item.devName,
                        ActionStatus = item._.ActionStatus,
                        DeviceStatus = item._.DeviceStatus,
                        CommandStatus = item._.CommandStatus,
                        CurrentTask = item._.CurrentTask,
                        CurrentSiteX = item._.CurrentSiteX,
                        CurrentSiteY = item._.CurrentSiteY,
                        CurrentSiteZ = item._.CurrentSiteZ,
                        FinishTask = item._.FinishTask,
                        GoodsStatus = item._.GoodsStatus,
                        ErrorMessage = item._.ErrorMessage,
                        UpdateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    });
                }
                DG_AWC.ItemsSource = ListAWC;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取行车数据异常: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取运输车数据
        /// </summary>
        private void RefreshRGV()
        {
            try
            {
                ListRGV.Clear();
                List<DevInfoRGV> list = ADS.mRgv.devices;
                if (list == null || list.Count == 0) return;

                foreach (DevInfoRGV item in list)
                {
                    ListRGV.Add(new DevDataRGV()
                    {
                        IsConnected = ADS.mSocket.IsConnected(item.devName),
                        DevName = item.devName,
                        ActionStatus = item._.ActionStatus,
                        DeviceStatus = item._.DeviceStatus,
                        CommandStatus = item._.CommandStatus,
                        CurrentTask = item._.CurrentTask,
                        CurrentSite = item._.CurrentSite,
                        RollerStatus = item._.RollerStatus,
                        RollerDiretion = item._.RollerDiretion,
                        FinishTask = item._.FinishTask,
                        GoodsStatus = item._.GoodsStatus,
                        ErrorMessage = item._.ErrorMessage,
                        UpdateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    });
                }
                DG_RGV.ItemsSource = ListRGV;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取运输车数据异常: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取摆渡车数据
        /// </summary>
        private void RefreshARF()
        {
            try
            {
                ListARF.Clear();
                List<DevInfoARF> list = ADS.mArf.devices;
                if (list == null || list.Count == 0) return;

                foreach (DevInfoARF item in list)
                {
                    ListARF.Add(new DevDataARF()
                    {
                        IsConnected = ADS.mSocket.IsConnected(item.devName),
                        DevName = item.devName,
                        ActionStatus = item._.ActionStatus,
                        DeviceStatus = item._.DeviceStatus,
                        CommandStatus = item._.CommandStatus,
                        CurrentTask = item._.CurrentTask,
                        CurrentSite = item._.CurrentSite,
                        RollerStatus = item._.RollerStatus,
                        RollerDiretion = item._.RollerDiretion,
                        FinishTask = item._.FinishTask,
                        GoodsStatus = item._.GoodsStatus,
                        ErrorMessage = item._.ErrorMessage,
                        UpdateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    });
                }
                DG_ARF.ItemsSource = ListARF;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取摆渡车数据异常: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取固定辊台数据
        /// </summary>
        private void RefreshFRT()
        {
            try
            {
                ListFRT.Clear();
                List<DevInfoFRT> list = ADS.mFrt.devices;
                if (list == null || list.Count == 0) return;

                foreach (DevInfoFRT item in list)
                {
                    ListFRT.Add(new DevDataFRT()
                    {
                        IsConnected = ADS.mSocket.IsConnected(item.devName),
                        DevName = item.devName,
                        ActionStatus = item._.ActionStatus,
                        DeviceStatus = item._.DeviceStatus,
                        CommandStatus = item._.CommandStatus,
                        CurrentTask = item._.CurrentTask,
                        RollerStatus = item._.RollerStatus,
                        RollerDiretion = item._.RollerDiretion,
                        FinishTask = item._.FinishTask,
                        GoodsStatus = item._.GoodsStatus,
                        ErrorMessage = item._.ErrorMessage,
                        UpdateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    });
                }
                DG_FRT.ItemsSource = ListFRT;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取固定辊台数据异常: " + ex.Message);
            }
        }
    }

    [Serializable]
    public class DevDataAWC
    {
        [DisplayName("连接")]
        public bool IsConnected { set; get; }

        [DisplayName("设备名")]
        public string DevName { set; get; }

        [DisplayName("运行状态")]
        public ActionEnum ActionStatus { set; get; }

        [DisplayName("设备状态")]
        public DeviceEnum DeviceStatus { set; get; }

        [DisplayName("命令状态")]
        public CommandEnum CommandStatus { set; get; }

        [DisplayName("当前任务")]
        public AwcTaskEnum CurrentTask { set; get; }

        [DisplayName("当前X轴坐标")]
        public int CurrentSiteX { set; get; }

        [DisplayName("当前Y轴坐标")]
        public int CurrentSiteY { set; get; }

        [DisplayName("当前Z轴坐标")]
        public int CurrentSiteZ { set; get; }

        [DisplayName("完成任务")]
        public AwcTaskEnum FinishTask { set; get; }

        [DisplayName("货物状态")]
        public AwcGoodsEnum GoodsStatus { set; get; }

        [DisplayName("故障信息")]
        public int ErrorMessage { set; get; }

        [DisplayName("更新时间")]
        public string UpdateTime { set; get; }
    }

    [Serializable]
    public class DevDataRGV
    {
        [DisplayName("连接")]
        public bool IsConnected { set; get; }

        [DisplayName("设备名")]
        public string DevName { set; get; }

        [DisplayName("运行状态")]
        public ActionEnum ActionStatus { set; get; }

        [DisplayName("设备状态")]
        public DeviceEnum DeviceStatus { set; get; }

        [DisplayName("命令状态")]
        public CommandEnum CommandStatus { set; get; }

        [DisplayName("当前任务")]
        public TaskEnum CurrentTask { set; get; }

        [DisplayName("当前坐标")]
        public int CurrentSite { set; get; }

        [DisplayName("辊台状态")]
        public RollerStatusEnum RollerStatus { set; get; }

        [DisplayName("辊台方向")]
        public RollerDiretionEnum RollerDiretion { set; get; }

        [DisplayName("完成任务")]
        public TaskEnum FinishTask { set; get; }

        [DisplayName("货物状态")]
        public GoodsEnum GoodsStatus { set; get; }

        [DisplayName("故障信息")]
        public int ErrorMessage { set; get; }

        [DisplayName("更新时间")]
        public string UpdateTime { set; get; }
    }

    [Serializable]
    public class DevDataARF
    {
        [DisplayName("连接")]
        public bool IsConnected { set; get; }

        [DisplayName("设备名")]
        public string DevName { set; get; }

        [DisplayName("运行状态")]
        public ActionEnum ActionStatus { set; get; }

        [DisplayName("设备状态")]
        public DeviceEnum DeviceStatus { set; get; }

        [DisplayName("命令状态")]
        public CommandEnum CommandStatus { set; get; }

        [DisplayName("当前任务")]
        public TaskEnum CurrentTask { set; get; }

        [DisplayName("当前坐标")]
        public int CurrentSite { set; get; }

        [DisplayName("辊台状态")]
        public RollerStatusEnum RollerStatus { set; get; }

        [DisplayName("辊台方向")]
        public RollerDiretionEnum RollerDiretion { set; get; }

        [DisplayName("完成任务")]
        public TaskEnum FinishTask { set; get; }

        [DisplayName("货物状态")]
        public GoodsEnum GoodsStatus { set; get; }

        [DisplayName("故障信息")]
        public int ErrorMessage { set; get; }

        [DisplayName("更新时间")]
        public string UpdateTime { set; get; }
    }

    [Serializable]
    public class DevDataFRT
    {
        [DisplayName("连接")]
        public bool IsConnected { set; get; }

        [DisplayName("设备名")]
        public string DevName { set; get; }

        [DisplayName("运行状态")]
        public ActionEnum ActionStatus { set; get; }

        [DisplayName("设备状态")]
        public DeviceEnum DeviceStatus { set; get; }

        [DisplayName("命令状态")]
        public CommandEnum CommandStatus { set; get; }

        [DisplayName("当前任务")]
        public TaskEnum CurrentTask { set; get; }

        [DisplayName("辊台状态")]
        public RollerStatusEnum RollerStatus { set; get; }

        [DisplayName("辊台方向")]
        public RollerDiretionEnum RollerDiretion { set; get; }

        [DisplayName("完成任务")]
        public TaskEnum FinishTask { set; get; }

        [DisplayName("货物状态")]
        public GoodsEnum GoodsStatus { set; get; }

        [DisplayName("故障信息")]
        public int ErrorMessage { set; get; }

        [DisplayName("设备名")]
        public string UpdateTime { set; get; }
    }
}
