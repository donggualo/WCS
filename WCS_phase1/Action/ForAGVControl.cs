using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Models;
using System.Data;
using WCS_phase1.Functions;
using WCS_phase1.Devices;
using WCS_phase1.LOG;
using System.Configuration;
using System.Threading;

namespace WCS_phase1.Action
{
    /// <summary>
    /// AGV运输货物任务
    /// </summary>
    class ForAGVControl
    {
        #region AGV 派车任务

        /// <summary>
        /// 执行AGV装货卸货的调度
        /// </summary>
        public void Run_DispatchAGV()
        {
            try
            {
                // 获取空闲包装线固定辊台
                DataTable dt = DataControl._mMySql.SelectAll("select * from wcs_config_device where LEFT(AREA,1) = 'A' and FLAG = 'Y'");
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_CONFIG_DEVICE> frtList = dt.ToDataList<WCS_CONFIG_DEVICE>();
                // 遍历执行派车任务
                foreach (WCS_CONFIG_DEVICE frt in frtList)
                {
                    SendAGV(frt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 发送调度AGV指令
        /// </summary>
        /// <param name="frt"></param>
        public void SendAGV(WCS_CONFIG_DEVICE frt)
        {
            try
            {
                // ID：获取当前时间生成 WCS-AGV 唯一识别码
                int ID = Convert.ToInt32(System.DateTime.Now.ToString("ddHHmmss"));
                // 装货点：当前包装线固定辊台对接点
                string PickStation = frt.DEVICE;
                // 卸货点：初始化站点
                string DropStation = ConfigurationManager.AppSettings["AGVDropStation"];

                // 发送 NDC
                bool res;
                string mes = "";
                res = DataControl._mNDCControl.AddNDCTask(ID, PickStation, DropStation, out mes);
                if (!res)
                {
                    throw new Exception(mes);
                }

                // 数据库新增AGV任务资讯
                String sql = String.Format(@"insert into wcs_agv_info(ID,PICKSTATION,DROPSTATION) values('{0}','{1}','{2}')", ID, PickStation, DropStation);
                DataControl._mMySql.ExcuteSql(sql);
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("SendAGV()","调度AGV装货卸货", frt.DEVICE,"",ex.ToString());
            }
        }

        #endregion

        #region 相关辊台控制

        /// <summary>
        /// 执行对应AGV任务的辊台设备
        /// </summary>
        public void Run_Roller()
        {
            try
            {
                //获取满足启动辊台条件的AGV任务---位于装货卸货点状态
                DataTable dt = DataControl._mMySql.SelectAll("select * from wcs_agv_info where MAGIC in(4,7) and TASK_UID is not null and AGV is not null order by CREATION_TIME");
                if (DataControl._mStools.IsNoData(dt))
                {
                    return;
                }
                List<WCS_AGV_INFO> agvList = dt.ToDataList<WCS_AGV_INFO>();
                // 遍历执行辊台
                foreach (WCS_AGV_INFO agv in agvList)
                {
                    CreatOrderTask(agv);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreatOrderTask(WCS_AGV_INFO agv)
        {
            try
            {
                // 指令
                byte[] order = null;
                // 设备号
                byte frtNum = 0;
                //启动方式
                byte site1 = FRT.RollerRunAll; // 初始默认辊台全启
                //启动方向
                byte site2 = FRT.RunFront; // 初始默认正向启动
                //接送类型
                byte site3 = FRT.GoodsReceive; // 初始默认接货
                //货物数量
                byte site4 = FRT.GoodsQty1; // 固定数量1

                // 按任务当前状态处理
                switch (Convert.ToInt32(agv.MAGIC))
                {
                    case AGVMagic.到达装货点:
                        // 获取对应包装线固定辊台资讯
                        FRT frt = new FRT(agv.PICKSTATION);
                        frtNum = frt.FRTNum();
                        // 是否存在货物
                        if (frt.GoodsStatus() == FRT.GoodsYesAll)
                        {
                            //=》 发指令请求AGV启动辊台装货
                        }
                        
                        break;
                    case AGVMagic.准备装货:
                        // 获取对应包装线固定辊台资讯
                        FRT frtpick = new FRT(agv.PICKSTATION);
                        frtNum = frtpick.FRTNum();
                        // 是否存在货物
                        if (frtpick.GoodsStatus() == FRT.GoodsYesAll)
                        {
                            // 生成指令请求包装线固定辊台反向启动辊台送货
                            site2 = FRT.RunObverse;
                            site3 = FRT.GoodsDeliver;
                        }

                        break;
                    case AGVMagic.到达卸货点:
                        // 获取对应包装线固定辊台资讯
                        FRT frtdrop = new FRT(agv.DROPSTATION);
                        frtNum = frtdrop.FRTNum();

                        // 当仅2#辊台有货
                        if (frtdrop.GoodsStatus() == FRT.GoodsYes2)
                        {
                            // 生成指令请求库存区固定辊台正向启动辊台接货
                            site2 = FRT.RunFront;
                            site3 = FRT.GoodsReceive;
                            // 只启动1#辊台
                            site1 = FRT.RollerRun1;
                        }

                        break;
                    default:
                        return;
                }
                // 获取指令
                order = FRT._RollerControl(frtNum, site1, site2, site3, site4);
                // 加入任务作业链表
                WCS_TASK_ITEM item = new WCS_TASK_ITEM()
                {
                    ITEM_ID = Convert.ToInt32(agv.MAGIC) == AGVMagic.到达卸货点 ? "库存区接货" : "包装线送货",
                    WCS_NO = agv.TASK_UID,
                    ID = agv.ID,
                    DEVICE = agv.AGV
                };
                DataControl._mTaskControler.StartTask(new FRTTack(item, DeviceType.固定辊台, order));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region AGV Method

        // 更新AGV卸货点
        public void UpdateAGVStation(int id, string station)
        {
            try
            {
                // 发送 NDC
                bool res;
                string mes = "";
                res = DataControl._mNDCControl.DoReDerect(id, station, out mes);
                if (!res)
                {
                    throw new Exception(mes);
                }
            }
            catch (Exception ex)
            {
                // LOG
                DataControl._mTaskTools.RecordTaskErrLog("UpdateAGVStation()", "更新AGV卸货点", id.ToString(), "", ex.ToString());
            }
        }

        #endregion


        #region AGV Function [对外]

        /// <summary>
        /// 更新AGV当前任务状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="agv"></param>
        /// <param name="magic"></param>
        public void SubmitAgvMagic(int id, string agv, int magic)
        {
            try
            {
                // 更新AGV任务资讯
                StringBuilder sql = new StringBuilder();
                sql.Append(String.Format(@"update wcs_agv_info set MAGIC = '{1}' where ID = '{0}'", id, magic));

                if (magic == AGVMagic.到达装货点)
                {
                    // 更新数据库内对应AGV任务资讯---获得对应AGV设备号
                    sql.Append(String.Format(@";update wcs_agv_info set AGV = '{0}'", agv));
                }

                if (magic == AGVMagic.装货完成)
                {
                    // 更新对应包装线辊台状态，允许新任务派车前往
                    sql.Append(String.Format(@";update wcs_config_device set FLAG = 'Y' where DEVICE in (
select distinct PICKSTATION from wcs_agv_info where ISOVER = 'N' and ID = '{0}')", id));
                }

                DataControl._mMySql.ExcuteSql(sql.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }

    /// <summary>
    /// AGV任务线程
    /// </summary>
    public class AGVTask
    {
        // 线程
        Thread _thread;
        ForAGVControl forAGV = new ForAGVControl();

        /// <summary>
        /// 构造函数
        /// </summary>
        public AGVTask()
        {
            _thread = new Thread(ThreadFunc)
            {
                Name = "AGV任务逻辑处理线程",
                IsBackground = true
            };

            _thread.Start();
        }

        /// <summary>
        /// 事务线程
        /// </summary>
        private void ThreadFunc()
        {
            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    forAGV.Run_DispatchAGV();
                    forAGV.Run_Roller();
                }
                catch (Exception)
                {
                }
            }
        }
    }

}
