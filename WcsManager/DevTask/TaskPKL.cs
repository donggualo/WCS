using Module;
using ModuleManager.WCS;
using PubResourceManager;
using WcsManager.DevModule;

namespace WcsManager.DevTask
{
    public class TaskPKL
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
        public DevInfoPKL device;

        /// <summary>
        /// 作业号
        /// </summary>
        public string jobid;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool activie;

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
        /// 来源对接设备
        /// </summary>
        public DevType fromdev;

        /// <summary>
        /// 目的对接设备
        /// </summary>
        public DevType todev;

        /// <summary>
        /// 插入数据库
        /// </summary>
        public void InsertDB()
        {
            CommonSQL.InsertJobDetail(id, jobid, area, (int)tasktype, null, DeviceType.包装线辊台, (int)tasktype, device?.devName,
                GetDevTypeS(fromdev), GetDevTypeS(todev), goodsnum, 0, 0, 0, goodsnum, 0, 0, 0);
        }

        /// <summary>
        /// 插入数据库
        /// </summary>
        private string GetDevTypeS(DevType dt)
        {
            switch (dt)
            {
                case DevType.包装线辊台:
                    return DeviceType.包装线辊台;
                case DevType.行车:
                    return DeviceType.行车;
                case DevType.固定辊台:
                    return DeviceType.固定辊台;
                case DevType.摆渡车:
                    return DeviceType.摆渡车;
                case DevType.运输车:
                    return DeviceType.运输车;
                default:
                    return null;
            }
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
    }
}
