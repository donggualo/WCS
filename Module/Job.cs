
using ModuleManager.WCS;
using PubResourceManager;

namespace Module
{
    /// <summary>
    /// WCS 作业
    /// </summary>
    public class Job
    {
        /// <summary>
        /// WCS作业ID
        /// </summary>
        public string jobid;

        /// <summary>
        /// WCS作业区域
        /// </summary>
        public string area;

        /// <summary>
        /// WMS任务1
        /// </summary>
        public WmsTask wmstask1;

        /// <summary>
        /// WMS任务2
        /// </summary>
        public WmsTask wmstask2;

        /// <summary>
        /// 作业类型
        /// </summary>
        public TaskTypeEnum jobtype;

        /// <summary>
        /// 入库作业状态
        /// </summary>
        public WcsInStatus instatus;

        /// <summary>
        /// 出库作业状态
        /// </summary>
        public WcsOutStatus outstatus;

        /// <summary>
        /// AGV作业状态
        /// </summary>
        public WcsAgvStatus agvstatus;

        /// <summary>
        /// 设备参考信息
        /// </summary>
        public DevFlag flag;

        /// <summary>
        /// 插入数据库
        /// </summary>
        public void InsertDB()
        {
            string taskid1 = "";
            string taskid2 = "";
            string frt = "";
            if (wmstask1 != null)
            {
                taskid1 = wmstask1.taskuid ?? "";
                frt = wmstask1.dev ?? "";
            }
            if (wmstask2 != null)
            {
                taskid2 = wmstask2.taskuid ?? "";
                frt = wmstask2.dev ?? "";
            }

            CommonSQL.InsertJobHeader(jobid, area, (int)jobtype, taskid1, taskid2, (int)flag, frt);
        }

        /// <summary>
        /// 更新作业状态
        /// </summary>
        public void UpdateStatus(int s)
        {
            switch (jobtype)
            {
                case TaskTypeEnum.入库:
                    instatus = (WcsInStatus)s;
                    break;
                case TaskTypeEnum.出库:
                    outstatus = (WcsOutStatus)s;
                    break;
                default:
                    return;
            }

            CommonSQL.UpdateJobHeader(jobid, s);
        }

        /// <summary>
        /// 更新作业任务号
        /// </summary>
        public void UpdateTask(string wmsTask)
        {
            CommonSQL.UpdateJobHeader(jobid, wmsTask);
        }

        public string Tostring()
        {
            string wmsT1 = string.Format("任务ID：{0} | 货物条码：{1} | 任务类型：{2} | 任务状态：{3} | 接货点：{4} | 送货点：{5}",
                wmstask1.taskuid, wmstask1.barcode, wmstask1.tasktype, wmstask1.taskstatus, wmstask1.takesite, wmstask1.givesite);
            string wmsT2 = "";
            if (wmstask2 != null)
            {
                wmsT2 = string.Format("任务ID：{0} | 货物条码：{1} | 任务类型：{2} | 任务状态：{3} | 接货点：{4} | 送货点：{5}",
                    wmstask2.taskuid, wmstask2.barcode, wmstask2.tasktype, wmstask2.taskstatus, wmstask2.takesite, wmstask2.givesite);
            }
            return string.Format("【作业号：{0}】【作业区域：{1}】【作业类型：{2}】【入库作业状态：{3} | 出库作业状态：{4}】【设备参考：{5}】 \n任务1：【{6}】 \n任务2：【{7}】",
                jobid, area, jobtype, instatus, outstatus, flag, wmsT1, wmsT2);
        }
    }

    /// <summary>
    /// 入库作业状态
    /// </summary>
    public enum WcsInStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        init,

        /// <summary>
        /// 固定辊台 → 摆渡车
        /// </summary>
        frttoarf,

        /// <summary>
        /// 摆渡车 → 运输车
        /// </summary>
        arftorgv,

        /// <summary>
        /// 运输车 → 行车
        /// </summary>
        rgvtoawc,

        /// <summary>
        /// 完成
        /// </summary>
        finish
    }

    /// <summary>
    /// 出库作业状态
    /// </summary>
    public enum WcsOutStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        init,

        /// <summary>
        /// 行车 → 运输车
        /// </summary>
        awctorgv,

        /// <summary>
        /// 运输车 → 摆渡车
        /// </summary>
        rgvtoarf,

        /// <summary>
        /// 摆渡车 → 固定辊台
        /// </summary>
        arftofrt,

        /// <summary>
        /// 完成
        /// </summary>
        finish
    }

    /// <summary>
    /// AGV作业状态
    /// </summary>
    public enum WcsAgvStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        init,

        /// <summary>
        /// 包装线 → AGV
        /// </summary>
        pktoagv,

        /// <summary>
        /// 分配WMS任务
        /// </summary>
        waitforwms,

        /// <summary>
        /// AGV → 库存区
        /// </summary>
        agvtowh,

        /// <summary>
        /// 完成
        /// </summary>
        finish
    }



    /// <summary>
    /// WMS 任务
    /// </summary>
    public class WmsTask
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string taskuid;

        /// <summary>
        /// 货物贴码
        /// </summary>
        public string barcode;

        /// <summary>
        /// 作业类型
        /// </summary>
        public TaskTypeEnum tasktype;

        /// <summary>
        /// 任务状态
        /// </summary>
        public WmsTaskStatus taskstatus;

        /// <summary>
        /// 起始设备
        /// </summary>
        public string dev;

        /// <summary>
        /// 接货点
        /// </summary>
        public string takesite;

        /// <summary>
        /// 送货点
        /// </summary>
        public string givesite;

        /// <summary>
        /// 作业坐标
        /// </summary>
        public JobSite site;

        /// <summary>
        /// 插入数据
        /// </summary>
        public void InsertDB()
        {
            CommonSQL.InsertTaskInfo(taskuid, (int)tasktype, barcode, takesite, givesite, dev ?? "");
        }

        /// <summary>
        /// 更新实际u坐标
        /// </summary>
        public void UpdateSite()
        {
            string wms;
            switch (tasktype)
            {
                case TaskTypeEnum.入库:
                    wms = givesite;
                    break;
                case TaskTypeEnum.出库:
                    wms = takesite;
                    break;
                default:
                    return;
            }
            WCS_CONFIG_LOC list = CommonSQL.GetLocInfo(wms);
            if (list == null) return;

            site = new JobSite
            {
                arfbuttfrt = CommonSQL.GetArfByFrt(dev),
                rgvsite1 = int.Parse(list.RGV_LOC_1 ?? "0"),
                rgvsite2 = int.Parse(list.RGV_LOC_2 ?? "0"),
                awcbuttRgv = list.AWC_LOC_TRACK ?? "",
                awcsite = list.AWC_LOC_STOCK ?? ""
            };
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="s"></param>
        public void UpdateStatus(WmsTaskStatus s)
        {
            taskstatus = s;
            CommonSQL.UpdateTaskInfo(taskuid, (int)s);
        }

    }

    /// <summary>
    /// 作业坐标
    /// </summary>
    public class JobSite
    {
        /// <summary>
        /// 摆渡车对接固定辊台坐标
        /// </summary>
        public int arfbuttfrt;

        /// <summary>
        /// 运输车1#辊台作业坐标
        /// </summary>
        public int rgvsite1;

        /// <summary>
        /// 运输车2#辊台作业坐标
        /// </summary>
        public int rgvsite2;

        /// <summary>
        /// 行车对接运输车坐标
        /// </summary>
        public string awcbuttRgv;

        /// <summary>
        /// 行车库存作业坐标
        /// </summary>
        public string awcsite;

        /// <summary>
        /// 摆渡车对接固定辊台坐标是否无效
        /// </summary>
        public bool IsArfNull()
        {
            return arfbuttfrt == 0 ? true : false;
        }

        /// <summary>
        /// 行车对接运输车作业 X轴坐标
        /// </summary>
        public int GetAwcButtRgvX()
        {
            return int.Parse(awcbuttRgv.Split('-')[0]);
        }

        /// <summary>
        /// 行车对接运输车作业 Y轴坐标
        /// </summary>
        public int GetAwcButtRgvY()
        {
            return int.Parse(awcbuttRgv.Split('-')[1]);
        }

        /// <summary>
        /// 行车对接运输车作业 Z轴坐标
        /// </summary>
        public int GetAwcButtRgvZ()
        {
            return int.Parse(awcbuttRgv.Split('-')[2]);
        }

        /// <summary>
        /// 行车库存作业 X轴坐标
        /// </summary>
        public int GetAwcSiteX()
        {
            return int.Parse(awcsite.Split('-')[0]);
        }

        /// <summary>
        /// 行车库存作业 Y轴坐标
        /// </summary>
        public int GetAwcSiteY()
        {
            return int.Parse(awcsite.Split('-')[1]);
        }

        /// <summary>
        /// 行车库存作业 Z轴坐标
        /// </summary>
        public int GetAwcSiteZ()
        {
            return int.Parse(awcsite.Split('-')[2]);
        }
    }

    /// <summary>
    /// WMS 任务状态
    /// </summary>
    public enum WmsTaskStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        初始化,

        /// <summary>
        /// 执行中
        /// </summary>
        执行中,

        /// <summary>
        /// 完成
        /// </summary>
        完成
    }
}
