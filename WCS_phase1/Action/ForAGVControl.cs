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
        Log log = new Log("AddTaskList");

        #region AGV 派车任务

        /// <summary>
        /// 执行AGV派送任务
        /// </summary>
        public void Run_AGVTask()
        {
            //=》 获取包装线固定辊台资讯

            //=》 判断是否存在AGV待命 是则继续执行，否则发送指令NDC派车并结束本次处理

            //=》 判断是否存在货物  无货则结束本次处理
            //=》 有货物则获取当前AGV资讯
            //=》 启动AGV辊台
            //=》 当AGV辊台已启动则启动当前固定辊台的辊台

        }
        // 当包装线固定辊台无AGV待命，则派车前往
        // 待命派车发送指令含 装货点(包装线固定辊台对接点) & 卸货点(初始化模拟点)

        // 当包装线固定辊台存在货物，扫码请求WMS返回位置时更新指令卸货点(实际库存货位固定辊台对接点)
        // 发送接货指令启动AGV辊台，当AGV启动成功后，启动包装线固定辊台的辊台

        // 当AGV异常后装载货物请求卸货点，需返回准确卸货点

        // 当AGV抵达卸货点，启动对应固定辊台的辊台后，发送送货指令启动AGV辊台

        #endregion
    }
}
