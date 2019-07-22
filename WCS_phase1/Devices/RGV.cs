using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Functions;

namespace WCS_phase1.Devices
{
    /// <summary>
    /// 有轨制导运输车 Rail Guided Vehicle
    /// </summary>
    class RGV : Device
    {
        public RGV(string name) : base(name)
        {

        }

        #region 命令状态

        /// <summary>
        /// 命令完成
        /// </summary>
        public static byte CommandFinish = 0x00;

        /// <summary>
        /// 命令执行中
        /// </summary>
        public static byte CommandExecute = 0x01;

        /// <summary>
        /// 设备故障
        /// </summary>
        public static byte DeviceError = 0xFE;

        /// <summary>
        /// 命令错误
        /// </summary>
        public static byte CommandError = 0xFF;

        #endregion

        #region 任务类别

        /// <summary>
        /// 定位任务
        /// </summary>
        public static byte TaskLocate = 0x01;

        /// <summary>
        /// 辊台任务
        /// </summary>
        public static byte TaskTake = 0x02;

        /// <summary>
        /// 停止辊台任务
        /// </summary>
        public static byte TaskRelease = 0x03;

        #endregion

        #region 辊台状态

        /// <summary>
        /// 辊台停止
        /// </summary>
        public static byte RollerStop = 0x00;

        /// <summary>
        /// 1#辊台启动
        /// </summary>
        public static byte RollerRun1 = 0x01;

        /// <summary>
        /// 2#辊台启动
        /// </summary>
        public static byte RollerRun2 = 0x02;

        /// <summary>
        /// 1#、2#辊台同时启动
        /// </summary>
        public static byte RollerRunAll = 0x03;

        #endregion

        #region 辊台方向

        /// <summary>
        /// 正向启动
        /// </summary>
        public static byte RunFront = 0x01;

        /// <summary>
        /// 反向启动
        /// </summary>
        public static byte RunObverse = 0x02;

        #endregion

        #region 货物状态

        /// <summary>
        /// 2个辊台都无货
        /// </summary>
        public static byte GoodsNoAll = 0x00;

        /// <summary>
        /// 1#辊台有货
        /// </summary>
        public static byte GoodsYes1 = 0x01;

        /// <summary>
        /// 2#辊台有货
        /// </summary>
        public static byte GoodsYes2 = 0x02;

        /// <summary>
        /// 2个辊台都有货
        /// </summary>
        public static byte GoodsYesAll = 0x03;

        #endregion


        #region 指令解析

        /// <summary>
        /// 获取命令字头
        /// </summary>
        /// <returns></returns>
        public byte[] CommandHead()
        {
            return GetDoubleByte(0);
        }


        /// <summary>
        /// 运输车号
        /// </summary>
        /// <returns></returns>
        public byte RGVNum()
        {
            return GetSingleByte(2);
        }

        /// <summary>
        /// 命令状态
        /// </summary>
        /// <returns></returns>
        public byte CommandStatus()
        {
            return GetSingleByte(3);
        }

        /// <summary>
        /// 目标值1
        /// </summary>
        /// <returns></returns>
        public byte Goods1site()
        {
            return GetSingleByte(4);
        }

        /// <summary>
        /// 目标值2
        /// </summary>
        /// <returns></returns>
        public byte Goods2site()
        {
            return GetSingleByte(5);
        }

        /// <summary>
        /// 目标值3
        /// </summary>
        /// <returns></returns>
        public byte Goods3site()
        {
            return GetSingleByte(6);
        }

        /// <summary>
        /// 目标值4
        /// </summary>
        /// <returns></returns>
        public byte Goods4site()
        {
            return GetSingleByte(7);
        }

        /// <summary>
        /// 当前任务
        /// </summary>
        /// <returns></returns>
        public byte CurrentTask()
        {
            return GetSingleByte(8);
        }

        /// <summary>
        /// 当前坐标值1
        /// </summary>
        /// <returns></returns>
        public byte Current1site()
        {
            return GetSingleByte(9);
        }

        /// <summary>
        /// 当前坐标值2
        /// </summary>
        /// <returns></returns>
        public byte Current2site()
        {
            return GetSingleByte(10);
        }

        /// <summary>
        /// 当前坐标值3
        /// </summary>
        /// <returns></returns>
        public byte Current3site()
        {
            return GetSingleByte(11);
        }

        /// <summary>
        /// 当前坐标值4
        /// </summary>
        /// <returns></returns>
        public byte Current4site()
        {
            return GetSingleByte(12);
        }

        /// <summary>
        /// 当前辊台状态
        /// </summary>
        /// <returns></returns>
        public byte CurrentStatus()
        {
            return GetSingleByte(13);
        }

        /// <summary>
        /// 辊台运行方向
        /// </summary>
        /// <returns></returns>
        public byte RunDirection()
        {
            return GetSingleByte(14);
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <returns></returns>
        public byte FinishTask()
        {
            return GetSingleByte(15);
        }

        /// <summary>
        /// 货物状态
        /// </summary>
        /// <returns></returns>
        public byte GoodsStatus()
        {
            return GetSingleByte(16);
        }

        /// <summary>
        /// 故障信息
        /// </summary>
        /// <returns></returns>
        public byte ErrorMessage()
        {
            return GetSingleByte(17);
        }

        #endregion

        /// <summary>
        /// RGV 当前位置
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSite()
        {
            SimpleTools tools = new SimpleTools();
            return tools.bytesToInt(new byte[] { Current1site(), Current2site(), Current3site(), Current4site() }, 0);
        }

        #region 运输车设备命令

        /// <summary>
        /// 运输车—定位任务
        /// </summary>
        /// <param name="RGVNum">运输车号</param>
        /// <param name="site">目标位置</param>
        /// <returns></returns>
        public static byte[] _Position(byte RGVNum, byte[] site)
        {
            //                     字头     设备号 控制码   值1      值2      值3      值4      结束符
            return new byte[] { 0x96, 0x02, RGVNum, 0x01, site[0], site[1], site[2], site[3], 0xFF, 0xFE };
        }

        /// <summary>
        /// 运输车—辊台控制
        /// </summary>
        /// <param name="RGVNum">运输车号</param>
        /// <param name="site1">启动方式</param>
        /// <param name="site2">启动方向</param>
        /// <param name="site3">接送类型</param>
        /// <param name="site4">货物数量</param>
        /// <returns></returns>
        public static byte[] _RollerControl(byte RGVNum, byte site1, byte site2, byte site3, byte site4)
        {
            //                     字头     设备号 控制码   值1    值2    值3    值4    结束符
            return new byte[] { 0x96, 0x02, RGVNum, 0x02, site1, site2, site3, site4, 0xFF, 0xFE };
        }

        /// <summary>
        /// 运输车—停止辊台
        /// </summary>
        /// <param name="RGVNum">运输车号</param>
        /// <returns></returns>
        public static byte[] _StopRoller(byte RGVNum)
        {
            //                     字头     设备号 控制码  值1   值2   值3   值4    结束符
            return new byte[] { 0x96, 0x02, RGVNum, 0x03, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
        }

        /// <summary>
        /// 运输车—终止任务
        /// </summary>
        /// <param name="RGVNum">运输车号</param>
        /// <returns></returns>
        public static byte[] _StopTask(byte RGVNum)
        {
            //                     字头     设备号 控制码  值1   值2   值3   值4    结束符
            return new byte[] { 0x96, 0x02, RGVNum, 0x7F, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
        }

        #endregion
    }
}
