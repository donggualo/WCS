using Module;
using ModuleManager.WCS;
using PubResourceManager;
using WcsManager.DevModule;

namespace WcsManager.DevTask
{
    public class TaskARF
    {
        public int id;

        /// <summary>
        /// 所属区域
        /// </summary>
        public string area;

        /// <summary>
        /// 任务类型
        /// </summary>
        public TaskTypeEnum tasktype;

        /// <summary>
        /// 货物数量
        /// </summary>
        public int goodsnum;

        /// <summary>
        /// 设备信息
        /// </summary>
        public DevInfoARF device;

        /// <summary>
        /// 作业号
        /// </summary>
        public string jobid;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool activie;

        /// <summary>
        /// 接货点
        /// </summary>
        public int takesite;

        /// <summary>
        /// 送货点
        /// </summary>
        public int givesite;

        /// <summary>
        /// 是否执行接货
        /// </summary>
        public bool takeready;

        /// <summary>
        /// 是否执行送货
        /// </summary>
        public bool giveready;

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus taskstatus;

        /// <summary>
        /// 插入数据库
        /// </summary>
        public void InsertDB()
        {
            CommonSQL.InsertJobDetail(id, jobid, area, (int)tasktype, null, DeviceType.摆渡车, (int)tasktype, device?.devName,
                null, null, goodsnum, takesite, 0, 0, goodsnum, givesite, 0, 0);
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public void UpdateStatus(TaskStatus s)
        {
            taskstatus = s;

            CommonSQL.UpdateJobDetail(id, (int)s);
        }

        /// <summary>
        /// 更新锁定设备名
        /// </summary>
        public void UpdateDev()
        {
            CommonSQL.UpdateJobDetail(id, device.devName);
        }

        /// <summary>
        /// 更新对接固定辊台坐标
        /// </summary>
        public void UpdateSite(int s)
        {
            switch (tasktype)
            {
                case TaskTypeEnum.入库:
                    takesite = s;
                    CommonSQL.UpdateTakeSite(id, takesite);
                    break;
                case TaskTypeEnum.出库:
                    givesite = s;
                    CommonSQL.UpdateGiveSite(id, givesite);
                    break;
                default:
                    break;
            }
        }


    }
}
