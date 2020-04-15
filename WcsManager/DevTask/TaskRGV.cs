using Module;
using ModuleManager.WCS;
using PubResourceManager;
using WcsManager.DevModule;

namespace WcsManager.DevTask
{
    public class TaskRGV
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
        /// 接货数量
        /// </summary>
        public int takeNum;

        /// <summary>
        /// 送货数量
        /// </summary>
        public int giveNum;

        /// <summary>
        /// 设备参考信息
        /// </summary>
        public DevFlag flag;

        /// <summary>
        /// 设备信息
        /// </summary>
        public DevInfoRGV device;

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
        /// 来源对接设备
        /// </summary>
        public DevType fromdev;

        /// <summary>
        /// 目的对接设备
        /// </summary>
        public DevType todev;

        /// <summary>
        /// 是否能继续
        /// </summary>
        public bool isLeaveAble;

        /// <summary>
        /// 插入数据库
        /// </summary>
        public void InsertDB()
        {
            CommonSQL.InsertJobDetail(id, jobid, area, (int)tasktype, taskid, DeviceType.运输车, (int)flag, device?.devName,
                GetDevTypeS(fromdev), GetDevTypeS(todev), takeNum, takesite, 0, 0, giveNum, givesite, 0, 0);
        }

        /// <summary>
        /// 获取设备类型转义
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string GetDevTypeS(DevType dt)
        {
            switch (dt)
            {
                case DevType.行车:
                    return DeviceType.行车;
                case DevType.固定辊台:
                    return DeviceType.固定辊台;
                case DevType.摆渡车:
                    return DeviceType.摆渡车;
                case DevType.运输车:
                    return DeviceType.运输车;
                case DevType.包装线辊台:
                    return DeviceType.包装线辊台;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="s"></param>
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
            CommonSQL.UpdateJobDetail(id, takesite, 0, 0, givesite, 0, 0);
        }
    }
}
