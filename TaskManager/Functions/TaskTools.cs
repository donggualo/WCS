using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows;
using TaskManager.Devices;
using ModuleManager.WCS;
using PubResourceManager;
using ModuleManager.PUB;

namespace TaskManager.Functions
{
    public class TaskTools
    {
        #region Client初始化

        public TaskTools()
        {
            InitializeClient();
        }

        /// <summary>
        /// 初始化客户端
        /// </summary>
        public void InitializeClient()
        {
            try
            {
                //需连接网络设备 ！！！！
                LinkDevicesClient();
                //需让ITEM W变为Q！！！！
                ResetItemTask();
            }
            catch (Exception ex)
            {
                // 记录LOG
                RecordTaskErrLog("InitializeClient()", "初始化", null, null, ex.Message);
            }
        }

        /// <summary>
        /// 连接网络设备
        /// </summary>
        public void LinkDevicesClient()
        {
            try
            {
                // 清空设备
                DataControl._mSocket.Close();
                // 获取设备设定档资讯
                String sql = "select * from wcs_config_device where FLAG <> 'N' order by CREATION_TIME";
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 遍历加入网络设备
                foreach (WCS_CONFIG_DEVICE dev in devList)
                {
                    byte[] refreshB = null;
                    switch (dev.TYPE)
                    {
                        case DeviceType.固定辊台:
                            refreshB = FRT._GetData();
                            break;
                        case DeviceType.摆渡车:
                            refreshB = ARF._GetData();
                            break;
                        case DeviceType.行车:
                            refreshB = ABC._GetData();
                            break;
                        case DeviceType.运输车:
                            refreshB = RGV._GetData();
                            break;
                        default:
                            continue;
                    }
                    if (!DataControl._mSocket.AddClient(dev.DEVICE, dev.IP, dev.PORT, refreshB, out string result))
                    {
                        throw new Exception(result);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录LOG
                RecordTaskErrLog("LinkDevicesClient()", "连接网络设备", null, null, ex.Message);
                MessageBox.Show("连接网络设备发生异常：" + ex.Message, "Error");
                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// Item任务重新执行
        /// </summary>
        public void ResetItemTask()
        {
            try
            {
                // 将所有 [任务中] 的Item 变为 [请求执行]
                String sql = String.Format("update wcs_task_item set STATUS = '{0}' where STATUS = '{1}'", ItemStatus.请求执行, ItemStatus.任务中);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 复位

        /// <summary>
        /// 设备复位待命点
        /// </summary>
        /// <param name="area"></param>
        /// <param name="type"></param>
        public string ResetDevLoc(String area, String type)
        {
            try
            {
                StringBuilder result = new StringBuilder();
                // 获取区域内可复位设备
                String sql = String.Format(@"select * from wcs_config_device where FLAG = '{2}' and AREA = '{0}' and TYPE = '{1}' order by FLAG desc", area, type, DeviceFlag.空闲);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    throw new Exception("当前没有可复位的设备！");
                }
                List<WCS_CONFIG_DEVICE> devList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 发送复位指令
                switch (type)
                {
                    case DeviceType.摆渡车:
                        ResetLoc_ARF(devList, out string mesARF);
                        result.Append(mesARF);
                        break;
                    case DeviceType.运输车:
                        ResetLoc_RGV(devList, out string mesRGV);
                        result.Append(mesRGV);
                        break;
                    case DeviceType.行车:
                        ResetLoc_ABC(devList, out string mesABC);
                        result.Append(mesABC);
                        break;
                    default:
                        break;
                }
                return result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 摆渡车复位
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="mes"></param>
        public void ResetLoc_ARF(List<WCS_CONFIG_DEVICE> dev, out string mes)
        {
            try
            {
                // 摆渡车待命点1
                int loc1 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyF1"));
                // 摆渡车待命点2
                int loc2 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyF2"));

                // 获取当前摆渡车资讯
                ARF a1 = new ARF(dev[0].DEVICE);
                ARF a2 = new ARF(dev[1].DEVICE);

                // 定位任务指令
                byte[] order1;
                byte[] order2;

                // 任务明细
                WCS_TASK_ITEM item1;
                WCS_TASK_ITEM item2;

                if (a1.CurrentSite() < a2.CurrentSite())
                {
                    // 定位任务指令
                    order1 = ARF._Position(a1.ARFNum(), (byte)loc1);
                    order2 = ARF._Position(a2.ARFNum(), (byte)loc2);
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.摆渡车复位,
                        ID = 1,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = loc1.ToString()
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.摆渡车复位,
                        ID = 2,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = loc2.ToString()
                    };
                }
                else
                {
                    // 定位任务指令
                    order1 = ARF._Position(a1.ARFNum(), (byte)loc2);
                    order2 = ARF._Position(a2.ARFNum(), (byte)loc1);
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.摆渡车复位,
                        ID = 1,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = loc2.ToString()
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.摆渡车复位,
                        ID = 2,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = loc1.ToString()
                    };
                }
                // 加入任务作业链表
                mes = null;
                if (dev[0].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new ARFTack(item1, DeviceType.摆渡车, order1));
                }
                else
                {
                    mes = dev[0].DEVICE + " 摆渡车目前无法操作复位任务; \r";
                }

                if (dev[1].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new ARFTack(item2, DeviceType.摆渡车, order2));
                }
                else
                {
                    mes = mes + dev[0].DEVICE + " 摆渡车目前无法操作复位任务; \r";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 运输车复位
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="mes"></param>
        public void ResetLoc_RGV(List<WCS_CONFIG_DEVICE> dev, out string mes)
        {
            try
            {
                // 运输车待命点1
                int loc1 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyR1"));
                // 运输车待命点2
                int loc2 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyR2"));

                // 获取当前摆渡车资讯
                RGV a1 = new RGV(dev[0].DEVICE);
                RGV a2 = new RGV(dev[1].DEVICE);

                // 定位任务指令
                byte[] order1;
                byte[] order2;

                // 任务明细
                WCS_TASK_ITEM item1;
                WCS_TASK_ITEM item2;

                if (a1.GetCurrentSite() < a2.GetCurrentSite())
                {
                    // 定位任务指令
                    order1 = RGV._Position(a1.RGVNum(), DataControl._mStools.IntToBytes(loc1));
                    order2 = RGV._Position(a2.RGVNum(), DataControl._mStools.IntToBytes(loc2));
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.运输车复位1,
                        ID = 3,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = loc1.ToString()
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.运输车复位2,
                        ID = 4,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = loc2.ToString()
                    };
                }
                else
                {
                    // 定位任务指令
                    order1 = RGV._Position(a1.RGVNum(), DataControl._mStools.IntToBytes(loc2));
                    order2 = RGV._Position(a2.RGVNum(), DataControl._mStools.IntToBytes(loc1));
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.运输车复位2,
                        ID = 3,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = loc2.ToString()
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.运输车复位1,
                        ID = 4,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = loc1.ToString()
                    };
                }
                // 加入任务作业链表
                mes = null;
                if (dev[0].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new RGVTack(item1, DeviceType.运输车, order1));
                }
                else
                {
                    mes = dev[0].DEVICE + " 运输车目前无法操作复位任务; \r";
                }

                if (dev[1].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new ARFTack(item2, DeviceType.运输车, order2));
                }
                else
                {
                    mes = mes + dev[1].DEVICE + " 运输车目前无法操作复位任务; \r";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 行车复位
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="mes"></param>
        public void ResetLoc_ABC(List<WCS_CONFIG_DEVICE> dev, out string mes)
        {
            try
            {
                int locY = 1;
                int locZ = 1;
                // 行车待命点1
                int locX1 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyA1"));
                // 行车待命点2
                int locX2 = Convert.ToInt32(DataControl._mStools.GetValueByKey("StandbyA2"));

                // 获取当前摆渡车资讯
                ABC a1 = new ABC(dev[0].DEVICE);
                ABC a2 = new ABC(dev[1].DEVICE);

                // 定位任务指令
                byte[] order1;
                byte[] order2;

                // 任务明细
                WCS_TASK_ITEM item1;
                WCS_TASK_ITEM item2;

                if (DataControl._mStools.BytesToInt(a1.CurrentXsite(), 0) < DataControl._mStools.BytesToInt(a2.CurrentXsite(), 0))
                {
                    // 定位任务指令
                    order1 = ABC._TaskControl(ABC.TaskLocate, a1.ABCNum(), DataControl._mStools.IntToBytes(locX1), DataControl._mStools.IntToBytes(locY), DataControl._mStools.IntToBytes(locZ));
                    order2 = ABC._TaskControl(ABC.TaskLocate, a2.ABCNum(), DataControl._mStools.IntToBytes(locX2), DataControl._mStools.IntToBytes(locY), DataControl._mStools.IntToBytes(locZ));
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.行车复位,
                        ID = 5,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = Convert.ToString(locX1) + "-" + Convert.ToString(locY) + "-" + Convert.ToString(locZ)
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.行车复位,
                        ID = 6,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = Convert.ToString(locX2) + "-" + Convert.ToString(locY) + "-" + Convert.ToString(locZ)
                    };
                }
                else
                {
                    // 定位任务指令
                    order1 = ABC._TaskControl(ABC.TaskLocate, a1.ABCNum(), DataControl._mStools.IntToBytes(locX2), DataControl._mStools.IntToBytes(locY), DataControl._mStools.IntToBytes(locZ));
                    order2 = ABC._TaskControl(ABC.TaskLocate, a2.ABCNum(), DataControl._mStools.IntToBytes(locX1), DataControl._mStools.IntToBytes(locY), DataControl._mStools.IntToBytes(locZ));
                    // 任务明细
                    item1 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.行车复位,
                        ID = 5,
                        DEVICE = dev[0].DEVICE,
                        LOC_TO = Convert.ToString(locX2) + "-" + Convert.ToString(locY) + "-" + Convert.ToString(locZ)
                    };
                    item2 = new WCS_TASK_ITEM()
                    {
                        ITEM_ID = ItemId.行车复位,
                        ID = 6,
                        DEVICE = dev[1].DEVICE,
                        LOC_TO = Convert.ToString(locX1) + "-" + Convert.ToString(locY) + "-" + Convert.ToString(locZ)
                    };
                }
                // 加入任务作业链表
                mes = null;
                if (dev[0].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new ABCTack(item1, DeviceType.行车, order1));
                }
                else
                {
                    mes = dev[0].DEVICE + " 行车目前无法操作复位任务; \r";
                }

                if (dev[1].FLAG == DeviceFlag.空闲)
                {
                    DataControl._mTaskControler.StartTask(new ABCTack(item2, DeviceType.行车, order2));
                }
                else
                {
                    mes = mes + dev[1].DEVICE + " 行车目前无法操作复位任务; \r";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        #region 位置坐标

        /// <summary>
        /// 获取摆渡车于目的固定辊台对接点位
        /// </summary>
        /// <param name="frt"></param>
        /// <returns></returns>
        public String GetFRTByARFLoc(String loc)
        {
            try
            {
                String sql = String.Format(@"select distinct FRT_LOC from WCS_CONFIG_LOC where ARF_LOC = '{0}'", loc);
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "";
                }
                return dtloc.Rows[0]["FRT_LOC"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取固定辊台所在设备作业区域
        /// </summary>
        /// <param name="frt"></param>
        /// <returns></returns>
        public String GetArea(String frt)
        {
            try
            {
                String sql = String.Format(@"select distinct AREA From wcs_config_device where DEVICE = '{0}'", frt);
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "";
                }
                return dtloc.Rows[0]["AREA"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取摆渡车于目的固定辊台对接点位
        /// </summary>
        /// <param name="frt"></param>
        /// <returns></returns>
        public String GetARFLoc(String frt)
        {
            try
            {
                String sql = String.Format(@"select distinct ARF_LOC from WCS_CONFIG_LOC where FRT_LOC = '{0}'", frt);
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "";
                }
                return dtloc.Rows[0]["ARF_LOC"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取行车目的轨道点位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public String GetABCTrackLoc(String loc)
        {
            try
            {
                String sql = String.Format(@"select distinct ABC_LOC_TRACK from WCS_CONFIG_LOC where WMS_LOC = '{0}'", loc);
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "";
                }
                return dtloc.Rows[0]["ABC_LOC_TRACK"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取行车目的库存点位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public String GetABCStockLoc(String loc)
        {
            try
            {
                String sql = String.Format(@"select distinct ABC_LOC_STOCK from WCS_CONFIG_LOC where WMS_LOC = '{0}'", loc);
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "";
                }
                return dtloc.Rows[0]["ABC_LOC_STOCK"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取wms入库目标位置对应设备点位(运输车定位坐标)[id表示是哪个辊台]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public String GetRGVLoc(int id, String loc)
        {
            try
            {
                // 辊台号顺序由port口往入库方向从大到小定
                String sql;
                if (id == 1)    // 运输车辊台①[内]定位
                {
                    sql = String.Format(@"select distinct RGV_LOC_1 LOC from WCS_CONFIG_LOC where WMS_LOC = '{0}'", loc);
                }
                else if (id == 2)   // 运输车辊台②[外]定位
                {
                    sql = String.Format(@"select distinct RGV_LOC_2 LOC from WCS_CONFIG_LOC where WMS_LOC = '{0}'", loc);
                }
                else
                {
                    return "0";
                }
                DataTable dtloc = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtloc))
                {
                    return "0";
                }
                return dtloc.Rows[0]["LOC"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取运输车入库方向上优先目的位置值
        /// </summary>
        /// <param name="loc_front"></param>
        /// <param name="loc_behind"></param>
        /// <returns></returns>
        public String GetLocByRgvToLoc(String loc_front, String loc_behind)
        {
            try
            {
                String loc = "NG";
                // 不能都为0，即不能没有目的位置
                if (Convert.ToInt32(loc_front) == 0 && Convert.ToInt32(loc_behind) == 0)
                {
                    return loc;
                }

                if (Convert.ToInt32(loc_front) == 0) return loc_behind;
                if (Convert.ToInt32(loc_behind) == 0) return loc_front;

                // 比较
                if (Convert.ToInt32(loc_front) <= Convert.ToInt32(loc_behind))
                {
                    loc = loc_front;
                }
                else
                {
                    loc = loc_behind;
                }
                return loc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取设备偏差计算出的坐标（加法）
        /// [xyz]（null 为全返；X/Y/Z 为单轴坐标值返回）
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="loc"></param>
        /// <param name="result"></param>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public bool GetLocByAddGap(string dev, string loc, out string result, string xyz = null)
        {
            try
            {
                if (string.IsNullOrEmpty(loc))
                {
                    result = "loc不可用！";
                    return false;
                }

                // 获取偏差参数
                String sql = String.Format(@"select * from wcs_config_dev_gap where DEVICE = '{0}'", dev);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    result = "找不到该设备号！";
                    return false;
                }
                WCS_CONFIG_DEV_GAP info = dt.ToDataEntity<WCS_CONFIG_DEV_GAP>();

                string locNew = "";
                switch (info.TYPE)
                {
                    case DeviceType.运输车:
                        // 仅仅只有X轴
                        locNew = (Convert.ToInt32(loc) + info.GAP_X).ToString();

                        break;
                    case DeviceType.行车:
                        String[] LOC = null;
                        // 返回xyz坐标
                        if (loc.Contains("-") && string.IsNullOrEmpty(xyz))
                        {
                            LOC = loc.Split('-');
                            locNew = (Convert.ToInt32(LOC[0]) + info.GAP_X) + "-"
                                   + (Convert.ToInt32(LOC[1]) + info.GAP_Y) + "-"
                                   + (Convert.ToInt32(LOC[2]) + info.GAP_Z);
                        }
                        // 返回单轴值（loc传入xyz坐标）
                        else if (loc.Contains("-") && !string.IsNullOrEmpty(xyz))
                        {
                            LOC = loc.Split('-');
                            switch (xyz)
                            {
                                case "X":
                                    locNew = (Convert.ToInt32(LOC[0]) + info.GAP_X).ToString();
                                    break;
                                case "Y":
                                    locNew = (Convert.ToInt32(LOC[1]) + info.GAP_Y).ToString();
                                    break;
                                case "Z":
                                    locNew = (Convert.ToInt32(LOC[2]) + info.GAP_Z).ToString();
                                    break;
                                default:
                                    result = "无此轴值（X/Y/Z）类型！";
                                    return false;
                            }
                        }
                        // 返回单轴值（loc传入单轴值）
                        else if (!loc.Contains("-") && !string.IsNullOrEmpty(xyz))
                        {
                            switch (xyz)
                            {
                                case "X":
                                    locNew = (Convert.ToInt32(loc) + info.GAP_X).ToString();
                                    break;
                                case "Y":
                                    locNew = (Convert.ToInt32(loc) + info.GAP_Y).ToString();
                                    break;
                                case "Z":
                                    locNew = (Convert.ToInt32(loc) + info.GAP_Z).ToString();
                                    break;
                                default:
                                    result = "无此轴值（X/Y/Z）类型！";
                                    return false;
                            }
                        }
                        else
                        {
                            result = "传入参数异常！";
                            return false;
                        }
                        break;
                    default:
                        result = "此类型无偏差参数！";
                        return false;
                }

                result = locNew;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取设备偏差计算出的坐标（减法）
        /// [xyz]（null 为全返；X/Y/Z 为单轴坐标值返回）
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="loc"></param>
        /// <param name="result"></param>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public bool GetLocByLessGap(string dev, string loc, out string result, string xyz = null)
        {
            try
            {
                if (string.IsNullOrEmpty(loc))
                {
                    result = "loc不可用！";
                    return false;
                }

                // 获取偏差参数
                String sql = String.Format(@"select * from wcs_config_dev_gap where DEVICE = '{0}'", dev);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    result = "找不到该设备号！";
                    return false;
                }
                WCS_CONFIG_DEV_GAP info = dt.ToDataEntity<WCS_CONFIG_DEV_GAP>();

                string locNew = "";
                switch (info.TYPE)
                {
                    case DeviceType.运输车:
                        // 仅仅只有X轴
                        locNew = (Convert.ToInt32(loc) - info.GAP_X).ToString();

                        break;
                    case DeviceType.行车:
                        String[] LOC = null;
                        // 返回xyz坐标
                        if (loc.Contains("-") && string.IsNullOrEmpty(xyz))
                        {
                            LOC = loc.Split('-');
                            locNew = (Convert.ToInt32(LOC[0]) - info.GAP_X) + "-"
                                   + (Convert.ToInt32(LOC[1]) - info.GAP_Y) + "-"
                                   + (Convert.ToInt32(LOC[2]) - info.GAP_Z);
                        }
                        // 返回单轴值（loc传入xyz坐标）
                        else if (loc.Contains("-") && !string.IsNullOrEmpty(xyz))
                        {
                            LOC = loc.Split('-');
                            switch (xyz)
                            {
                                case "X":
                                    locNew = (Convert.ToInt32(LOC[0]) - info.GAP_X).ToString();
                                    break;
                                case "Y":
                                    locNew = (Convert.ToInt32(LOC[1]) - info.GAP_Y).ToString();
                                    break;
                                case "Z":
                                    locNew = (Convert.ToInt32(LOC[2]) - info.GAP_Z).ToString();
                                    break;
                                default:
                                    result = "无此轴值（X/Y/Z）类型！";
                                    return false;
                            }
                        }
                        // 返回单轴值（loc传入单轴值）
                        else if (!loc.Contains("-") && !string.IsNullOrEmpty(xyz))
                        {
                            switch (xyz)
                            {
                                case "X":
                                    locNew = (Convert.ToInt32(loc) - info.GAP_X).ToString();
                                    break;
                                case "Y":
                                    locNew = (Convert.ToInt32(loc) - info.GAP_Y).ToString();
                                    break;
                                case "Z":
                                    locNew = (Convert.ToInt32(loc) - info.GAP_Z).ToString();
                                    break;
                                default:
                                    result = "无此轴值（X/Y/Z）类型！";
                                    return false;
                            }
                        }
                        else
                        {
                            result = "传入参数异常！";
                            return false;
                        }
                        break;
                    default:
                        result = "此类型无偏差参数！";
                        return false;
                }

                result = locNew;
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取行车目的分区奇偶性
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public int GetABCPartitionParity(string area, int loc)
        {
            try
            {
                // 获取 WCS库存划分范围（行车）
                String sql = String.Format(@"select * from wcs_param where NAME = 'WCS_STOCK_PARTITION_RANGE' and VALUE6 = '{0}'", area);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return 0;
                }
                List<WCS_PARAM> pList = dt.ToDataList<WCS_PARAM>();
                // 遍历获取所在分区数
                int value = 0;
                foreach (WCS_PARAM p in pList)
                {
                    // 范围区间
                    if (Convert.ToInt32(p.VALUE2) <= loc && loc <= Convert.ToInt32(p.VALUE3))
                    {
                        value = Convert.ToInt32(p.VALUE1);
                        break;
                    }
                }
                // 判断奇偶数 => 区分行车管控
                if (value == 0)
                {
                    return 0; // ERR
                }
                else if (Convert.ToBoolean(value % 2))
                {
                    return 1; // 奇数行车1，靠外的
                }
                else
                {
                    return 2; // 偶数行车2，靠内的
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取Task RGV之间的对接点坐标
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="taskID"></param>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public int GetCloseRGVLoc(string distance, string taskID, out string loc1, out string loc2)
        {
            try
            {
                // 获取 WCS库存划分范围（行车）
                String sql = String.Format(@"select P_RANGE, RR, (RR - {1}) R1 from wcs_task_out_range_v where TASK_UID = '{0}'", taskID, distance);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    loc1 = null;
                    loc2 = null;
                    return 0;
                }
                int P = Convert.ToInt32(dt.Rows[0]["P_RANGE"].ToString());
                loc1 = dt.Rows[0]["RR"].ToString();
                loc2 = dt.Rows[0]["R1"].ToString();

                // 判断奇偶数
                if (Convert.ToBoolean(P % 2))
                {
                    return 1; // 奇数行车1，靠外的
                }
                else
                {
                    return 2; // 偶数行车2，靠内的
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 设备信息

        /// <summary>
        /// 获取同区域另一设备
        /// </summary>
        /// <param name="dev"></param>
        /// <returns></returns>
        public String GetOtherDev(String dev, String type)
        {
            try
            {
                String sql = String.Format(@"select DEVICE from wcs_config_device where FLAG <> '{0}' and TYPE = '{1}' and 
                        DEVICE <> '{2}' and AREA in (select AREA from wcs_config_device where DEVICE = '{2}')", DeviceFlag.失效, type, dev);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return null;
                }
                return dt.Rows[0]["DEVICE"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取指定区域、类型、职责的设备List
        /// </summary>
        /// <param name="area"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<WCS_CONFIG_DEVICE> GetDeviceList(String area, String type, String duty)
        {
            String sql;
            try
            {
                sql = String.Format(@"select * from wcs_config_device where FLAG <> '{4}' and AREA = '{0}' and TYPE = '{1}' and DUTY in ('{2}','{3}')
                        order by CREATION_TIME", area, type, duty, DeviceDuty.负责全部, DeviceFlag.失效);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return new List<WCS_CONFIG_DEVICE>();
                }
                List<WCS_CONFIG_DEVICE> List = dt.ToDataList<WCS_CONFIG_DEVICE>();
                return List;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取该设备的设备类型
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public String GetDeviceType(String device)
        {
            try
            {
                String sql = String.Format(@"select distinct TYPE from wcs_config_device where DEVICE = '{0}'", device);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return "";
                }
                return dt.Rows[0]["TYPE"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取任务所需设备职责
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <returns></returns>
        public String GetDeviceDuty(String wcs_no)
        {
            try
            {
                String sql = String.Format(@"select distinct TASK_TYPE from WCS_COMMAND_V where WCS_NO = '{0}'", wcs_no);
                DataTable dtstep = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtstep))
                {
                    return "";
                }
                switch (dtstep.Rows[0]["TASK_TYPE"].ToString())
                {
                    case TaskType.AGV搬运:
                    case TaskType.入库:
                        return DeviceDuty.负责入库;

                    case TaskType.出库:
                        return DeviceDuty.负责出库;

                    default:
                        return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据入库任务获取对应分区行车
        /// </summary>
        /// <param name="taskuid"></param>
        /// <returns></returns>
        public String GetABCByInTaskUID(String taskuid)
        {
            try
            {
                String sql = String.Format(@"select distinct ABC from WCS_TASK_IN_RANGE_V where TASK_UID = '{0}'", taskuid);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return "";
                }
                return dt.Rows[0]["ABC"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 该设备是否锁定
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool IsDeviceLock(String device)
        {
            try
            {
                String sql = String.Format(@"select FLAG from wcs_config_device where DEVICE = '{0}'", device);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return true;
                }
                if (dt.Rows[0]["FLAG"].ToString() == DeviceFlag.空闲 || dt.Rows[0]["FLAG"].ToString() == DeviceFlag.占用)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 锁定设备
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="device"></param>
        public void DeviceLock(String wcs_no, String device)
        {
            try
            {
                String sql = String.Format(@"update wcs_config_device set FLAG = '{2}', LOCK_WCS_NO = '{0}' where DEVICE = '{1}'", wcs_no, device, DeviceFlag.锁定);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解锁设备(By 设备号or清单号)
        /// </summary>
        /// <param name="device"></param>
        public void DeviceUnLock(String devOrwcs)
        {
            try
            {
                String sql = String.Format(@"update wcs_config_device set FLAG = '{1}', LOCK_WCS_NO = null where DEVICE = '{0}' or LOCK_WCS_NO = '{0}'", devOrwcs, DeviceFlag.空闲);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 解锁设备(By 设备号&清单号)
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="wcs_no"></param>
        public void UnLockByDevAndWcsNo(string dev, string wcs_no)
        {
            try
            {
                String sql = String.Format(@"update wcs_config_device set FLAG = '{2}', LOCK_WCS_NO = null where 
                    DEVICE = '{0}' and LOCK_WCS_NO = '{1}'", dev, wcs_no, DeviceFlag.空闲);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 任务信息

        /// <summary>
        /// 获取Item状态
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string GetItemStatus(int ID)
        {
            try
            {
                String sql = String.Format(@"select STATUS from wcs_task_item where ID = {0}", ID);
                DataTable dtstep = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtstep))
                {
                    return "";
                }
                return dtstep.Rows[0]["STATUS"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前Task最新设备任务
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="dev_type"></param>
        /// <returns></returns>
        public List<WCS_TASK_ITEM> GetItemList_Now(String wcs_no, String dev_type)
        {
            try
            {
                String sql = String.Format(@"select * from wcs_task_item where ID in (select MAX(ID) from wcs_task_item where 
                        WCS_NO = '{0}' and DEV_TYPE = '{1}' GROUP BY TASK_NOW)", wcs_no, dev_type);
                DataTable dtitem = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dtitem))
                {
                    return new List<WCS_TASK_ITEM>();
                }
                List<WCS_TASK_ITEM> itemList = dtitem.ToDataList<WCS_TASK_ITEM>();
                return itemList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取当前清单货物数量
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <returns></returns>
        public int GetTaskGoodsQty(String wcs_no)
        {
            try
            {
                String sql = String.Format(@"select CASE WHEN (TASK_UID_1 is not null and TASK_UID_1 <> '' and SITE_1 <>'{1}') 
             AND (TASK_UID_2 is not null and TASK_UID_2 <> '' and SITE_1 <>'{1}') THEN 2 ELSE 1 END AS QTY from wcs_command_v where WCS_NO = '{0}'", wcs_no, TaskSite.完成);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return 0;
                }
                return Convert.ToInt32(dt.Rows[0]["QTY"].ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新 COMMAND 状态[1.生成单号，2.请求执行，3.执行中，4.结束]
        /// </summary>
        /// <param name="step"></param>
        /// <param name="wcs_no"></param>
        public void UpdateCommand(String wcs_no, String step)
        {
            try
            {
                String sql = String.Format(@"update WCS_COMMAND_MASTER set STEP = '{0}',UPDATE_TIME = NOW() where WCS_NO = '{1}'", step, wcs_no);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新 TASK 状态 By TASK_UID [N:未执行,W:任务中,Y:完成,X:失效]
        /// </summary>
        /// <param name="task_uid"></param>
        /// <param name="site"></param>
        public void UpdateTask(String task_uid, String site)
        {
            try
            {
                String sql = String.Format(@"update WCS_TASK_INFO set SITE = '{0}',UPDATE_TIME = NOW() where TASK_UID = '{1}'", site, task_uid);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新 TASK 状态 By WCS_NO [N:未执行,W:任务中,Y:完成,X:失效]
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="site"></param>
        public void UpdateTaskByWCSNo(String wcs_no, String site)
        {
            try
            {
                String sql = String.Format(@"select TASK_UID_1,TASK_UID_2 from wcs_command_master where WCS_NO = '{0}'", wcs_no);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                string task1 = dt.Rows[0]["TASK_UID_1"].ToString();
                string task2 = dt.Rows[0]["TASK_UID_2"].ToString();

                if (!string.IsNullOrEmpty(task1))
                {
                    UpdateTask(task1, site);
                }

                if (!string.IsNullOrEmpty(task2))
                {
                    UpdateTask(task2, site);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新 ITEM 明细：
        /// 设备编号(key = DEVICE)；
        /// 来源位置(key = LOC_FROM)；
        /// 作业状态(key = STATUS)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="wcs_no"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateItem(int id, String wcs_no, String key, String value)
        {
            try
            {
                String sql = String.Format(@"update WCS_TASK_ITEM set {0} = '{1}',UPDATE_TIME = NOW() where ID = {2} and WCS_NO = '{3}'",
                    key, value, id, wcs_no);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建 WCS 入库清单
        /// </summary>
        /// <param name="task_uid"></param>
        public void CreateCommandIn(String task_uid, String frt)
        {
            try
            {
                String wcs_no;
                // 判断是否有在建清单
                String sql = String.Format(@"select WCS_NO from wcs_command_master where STEP = '{0}' and FRT = '{1}'", CommandStep.生成单号, frt);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    // 无则新增
                    wcs_no = "I" + System.DateTime.Now.ToString("yyMMddHHmmssff"); // 生成 WCS 清单号
                    sql = String.Format(@"insert into wcs_command_master(WCS_NO, FRT, TASK_UID_1) values('{0}','{1}','{2}')", wcs_no, frt, task_uid);
                }
                else
                {
                    // 有则更新
                    wcs_no = dt.Rows[0]["WCS_NO"].ToString();
                    sql = String.Format(@"update wcs_command_master set UPDATE_TIME = NOW(), TASK_UID_2 = '{0}' where WCS_NO = '{1}'", task_uid, wcs_no);

                    //更新WCS COMMAND状态——请求执行
                    DataControl._mTaskTools.UpdateCommand(wcs_no, CommandStep.请求执行);
                    // 锁定设备
                    DataControl._mTaskTools.DeviceLock(wcs_no, frt);
                }

                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建 WCS 出库清单
        /// </summary>
        /// <param name="area"></param>
        /// <param name="taskuid_1"></param>
        /// <param name="taskuid_2"></param>
        /// <returns></returns>
        public string CreateCommandOut(String area, String taskuid_1, String taskuid_2)
        {
            try
            {
                // 获取该区域可用的固定辊台
                String sqlfrt = String.Format(@"select MAX(device) FRT from wcs_config_device where AREA = '{0}' and FLAG <> '{1}' and 
                        TYPE = '{2}' and DUTY = '{3}'", area, DeviceFlag.失效, DeviceType.固定辊台, DeviceDuty.负责出库);
                DataTable dtfrt = DataControl._mMySql.SelectAll(sqlfrt);
                if (DataControl._mStools.IsNoData(dtfrt))
                {
                    return null;
                }
                string frt = dtfrt.Rows[0]["FRT"].ToString();
                //生成 WCS 清单号
                String wcs_no = "O" + DateTime.Now.ToString("yyMMddHHmmssff");
                // 生成 COMMAND
                String sql = String.Format(@"insert into wcs_command_master(WCS_NO, FRT, TASK_UID_1, TASK_UID_2) values('{0}','{1}','{2}','{3}')",
                        wcs_no, frt, taskuid_1, String.IsNullOrEmpty(taskuid_2) ? null : taskuid_2);
                DataControl._mMySql.ExcuteSql(sql);
                return wcs_no;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 创建任务Item
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="task_now"></param>
        /// <param name="item_id"></param>
        /// <param name="dev_type"></param>
        /// <param name="loc_from"></param>
        /// <param name="loc_to"></param>
        /// <param name="status"></param>
        /// <param name="device"></param>
        public void CreateTaskItem(String wcs_no, String task_now, String item_id, String dev_type, String loc_from, String loc_to, String status, String device = null)
        {
            try
            {
                String sql = String.Format(@"insert into WCS_TASK_ITEM(WCS_NO,TASK_NOW,ITEM_ID,DEV_TYPE,DEVICE,LOC_FROM,LOC_TO,STATUS) 
                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                             wcs_no, task_now, item_id, dev_type, device, loc_from, loc_to, status);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 清除 WCS生成的 COMMAND 资讯 
        /// </summary>
        /// <param name="wcs_no"></param>
        public void DeleteCommand(String wcs_no)
        {
            try
            {
                String sql = String.Format(@"delete from WCS_TASK_ITEM where WCS_NO = '{0}'", wcs_no);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除'N'/'Q'状态的 ITEM 任务[ item_id 为空则全部清除]
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <param name="item_id"></param>
        public void DeleteItem(String wcs_no, String item_id = null)
        {
            String sql;
            try
            {
                if (String.IsNullOrEmpty(item_id))
                {
                    sql = String.Format(@"delete from WCS_TASK_ITEM where STATUS in ('{1}','{2}') and WCS_NO = '{0}'", wcs_no, ItemStatus.不可执行, ItemStatus.请求执行);
                }
                else
                {
                    sql = String.Format(@"delete from WCS_TASK_ITEM where STATUS in ('{1}','{2}') and WCS_NO = '{0}' and ITEM_ID = '{3}'", wcs_no, ItemStatus.不可执行, ItemStatus.请求执行, item_id);
                }

                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 备份任务
        /// </summary>
        /// <param name="wcs_no"></param>
        public void BackupTask(String wcs_no)
        {
            try
            {
                String sql = String.Format(@"
    /*备份*/
    insert into wcs_task_backup(WCS_NO, FRT, TASK_UID, TASK_TYPE, BARCODE, W_S_LOC, W_D_LOC)
select a.WCS_NO, a.FRT, b.TASK_UID, b.TASK_TYPE, b.BARCODE, b.W_S_LOC, b.W_D_LOC from wcs_command_master a , wcs_task_info b 
where (a.TASK_UID_1 = b.TASK_UID or a.TASK_UID_2 = b.TASK_UID) and a.STEP = '{1}' and WCS_NO = '{0}';
    /*清Item*/
    delete from wcs_task_item where WCS_NO = '{0}';
    /*清Task*/
    delete from wcs_task_info where TASK_UID in
    (select TASK_UID_1 TASK_UID from wcs_command_master where WCS_NO = '{0}'
      union
     select TASK_UID_2 TASK_UID from wcs_command_master where WCS_NO = '{0}');
    /*清Command*/
    delete from wcs_command_master where WCS_NO = '{0}'", wcs_no, CommandStep.结束);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 清单任务分配RGV是否完成
        /// </summary>
        /// <param name="wcs_no"></param>
        /// <returns></returns>
        public bool IsRGVtaskOK(string wcs_no)
        {
            try
            {
                int num;
                // 获取清单 WMS任务数量
                String sqlC = String.Format(@"select distinct TASK_UID_1,TASK_UID_2 from wcs_command_master where WCS_NO = '{0}'", wcs_no);
                DataTable dtC = DataControl._mMySql.SelectAll(sqlC);
                if (DataControl._mStools.IsNoData(dtC))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(dtC.Rows[0]["TASK_UID_1"].ToString()) || string.IsNullOrEmpty(dtC.Rows[0]["TASK_UID_2"].ToString()))
                {
                    num = 1;
                }
                else
                {
                    num = 2;
                }
                // 当前Item内RGV任务比对
                String sql = String.Format(@"select distinct TASK_NOW from wcs_task_item where LEFT(ITEM_ID,2) = '02' and WCS_NO = '{0}'", wcs_no);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    return false;
                }
                if (dt.Rows.Count == num)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否满足交接条件
        /// </summary>
        /// <param name="wcsNO"></param>
        /// <param name="taskID"></param>
        /// <param name="itemID"></param>
        /// <param name="ID"></param>
        /// <param name="dev"></param>
        /// <returns></returns>
        public bool IsStandBy(string wcsNO, string taskID, string itemID, out int ID, out string dev)
        {
            try
            {
                String sql = String.Format(@"select distinct ID, STATUS, DEVICE from wcs_task_item where WCS_NO = '{0}' and TASK_NOW = '{1}' and 
                        ITEM_ID = '{2}' order by ID desc", wcsNO, taskID, itemID);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    ID = 0;
                    dev = "";
                    return false;
                }
                ID = Convert.ToInt32(dt.Rows[0]["ID"].ToString());
                dev = dt.Rows[0]["DEVICE"].ToString();
                if (dt.Rows[0]["STATUS"].ToString() == ItemStatus.交接中)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 是否存在另一任务
        /// </summary>
        /// <param name="wcsNO"></param>
        /// <param name="taskID"></param>
        /// <param name="dev"></param>
        /// <param name="status"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool IsOtherTask(string wcsNO, string taskID, string dev, string status, out int ID)
        {
            try
            {
                String sql = String.Format(@"select distinct ID from wcs_task_item where WCS_NO = '{0}' and TASK_NOW <> '{1}' and 
                    DEVICE = '{2}' and STATUS = '{3}'", wcsNO, taskID, dev, status);
                DataTable dt = DataControl._mMySql.SelectAll(sql);
                if (DataControl._mStools.IsNoData(dt))
                {
                    ID = 0;
                    return false;
                }
                ID = Convert.ToInt32(dt.Rows[0]["ID"].ToString());
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 日志记录

        /// <summary>
        /// 任务流程异常日志
        /// </summary>
        /// <param name="name"></param>
        /// <param name="remark"></param>
        /// <param name="wcs_no"></param>
        /// <param name="item_id"></param>
        /// <param name="res"></param>
        /// <param name="mes"></param>
        public void RecordTaskErrLog(String name, String remark, String wcs_no, String item_id, String mes)
        {
            try
            {
                String sql = String.Format(@"insert into wcs_function_log(FUNCTION_NAME,REMARK,WCS_NO,ITEM_ID,RESULT,MESSAGE) values('{0}','{1}','{2}','{3}','NG','{4}')",
                    name, remark, wcs_no, item_id, mes);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 生成指令日志
        /// </summary>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public String GetLogMessC(WCS_TASK_ITEM item, byte[] order)
        {
            String ORDER = DataControl._mStools.BytetToString(order);
            String message = String.Format(@"【CreatOrder】{0}({1})：WCS单号[ {2} ]，ID[ {3} ]，设备号[ {4} ]，指令[ {5} ].",
                ItemId.GetItemIdName(item.ITEM_ID), item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, ORDER);
            return message;
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public String GetLogMessE(WCS_TASK_ITEM item, byte[] order, String err)
        {
            String message = String.Format(@"【Error】{0}({1})：WCS单号[ {2} ]，ID[ {3} ]，设备号[ {4} ]，异常信息[ {5} ].",
                ItemId.GetItemIdName(item.ITEM_ID), item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, err);

            return message;
        }

        /// <summary>
        /// 成功日志
        /// </summary>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public String GetLogMessS(WCS_TASK_ITEM item, byte[] order)
        {
            String ORDER = DataControl._mStools.BytetToString(order);
            String message = String.Format(@"【Success】{0}({1})：WCS单号[ {2} ]，ID[ {3} ]，设备号[ {4} ]，来源[ {5} ]，目标[ {6} ]，指令[ {7} ].",
                ItemId.GetItemIdName(item.ITEM_ID), item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, item.LOC_FROM, item.LOC_TO, ORDER);

            return message;
        }

        /// <summary>
        /// 待命日志
        /// </summary>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public String GetLogMessW(WCS_TASK_ITEM item, byte[] order)
        {
            String ORDER = DataControl._mStools.BytetToString(order);
            String message = String.Format(@"【WaitFor】{0}({1})：WCS单号[ {2} ]，ID[ {3} ]，设备号[ {4} ]，来源[ {5} ]，目标[ {6} ]，指令[ {7} ].",
                ItemId.GetItemIdName(item.ITEM_ID), item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, item.LOC_FROM, item.LOC_TO, ORDER);

            return message;
        }

        /// <summary>
        /// 发送命令日志
        /// </summary>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public String GetLogMess(WCS_TASK_ITEM item, byte[] order)
        {
            String ORDER = DataControl._mStools.BytetToString(order);
            String message = String.Format(@"【SendOrder】{0}({1})：WCS单号[ {2} ]，ID[ {3} ]，设备号[ {4} ]，来源[ {5} ]，目标[ {6} ]，指令[ {7} ].",
                ItemId.GetItemIdName(item.ITEM_ID), item.ITEM_ID, item.WCS_NO, item.ID, item.DEVICE, item.LOC_FROM, item.LOC_TO, ORDER);

            return message;
        }

        #endregion
    }
}
