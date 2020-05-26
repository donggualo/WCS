using Module;
using ModuleManager.WCS;
using PubResourceManager;
using WcsManager.DevModule;

namespace WcsManager.DevTask
{
    public class TaskAWC
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
        /// 设备参考信息
        /// </summary>
        public DevFlag flag;

        /// <summary>
        /// 设备信息
        /// </summary>
        public DevInfoAWC device;

        /// <summary>
        /// 作业号
        /// </summary>
        public string jobid;

        /// <summary>
        /// 任务号
        /// </summary>
        public string taskid;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool activie;

        /// <summary>
        /// 接货点 X轴值
        /// </summary>
        public int takesiteX;

        /// <summary>
        /// 接货点 Y轴值
        /// </summary>
        public int takesiteY;

        /// <summary>
        /// 接货点 Z轴值
        /// </summary>
        public int takesiteZ;

        /// <summary>
        /// 是否执行接货
        /// </summary>
        public bool takeready;

        /// <summary>
        /// 送货点 X轴值
        /// </summary>
        public int givesiteX;

        /// <summary>
        /// 送货点 Y轴值
        /// </summary>
        public int givesiteY;

        /// <summary>
        /// 送货点 Z轴值
        /// </summary>
        public int givesiteZ;

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
            CommonSQL.InsertJobDetail(id, jobid, area, (int)tasktype, taskid, DeviceType.行车, (int)flag, device?.devName,
                null, null, 1, takesiteX, takesiteY, takesiteZ, 1, givesiteX, givesiteY, givesiteZ);
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
        /// 更新坐标
        /// </summary>
        public void UpdateSite()
        {
            CommonSQL.UpdateJobDetail(id, takesiteX, takesiteY, takesiteZ, givesiteX, givesiteY, givesiteZ);
        }
    }
}
