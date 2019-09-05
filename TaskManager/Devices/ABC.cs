using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Devices
{
    /// <summary>
    /// 自动行车 Automatic Bridge Crane
    /// </summary>
    public class ABC : Device
    {
        public ABC(string name) : base(name)
        {

        }

        #region 状态

        /// <summary>
        /// 设备停止
        /// </summary>
        public static byte Stop = 0x00;

        /// <summary>
        /// 设备运行
        /// </summary>
        public static byte Run = 0x01;

        /// <summary>
        /// 设备故障
        /// </summary>
        public static byte DeviceError = 0x01;

        /// <summary>
        /// 命令错误
        /// </summary>
        public static byte CommandError = 0x01;

        #endregion

        #region 任务类别

        /// <summary>
        /// 定位任务
        /// </summary>
        public static byte TaskLocate = 0x01;

        /// <summary>
        /// 取货任务
        /// </summary>
        public static byte TaskTake = 0x02;

        /// <summary>
        /// 放货任务
        /// </summary>
        public static byte TaskRelease = 0x03;

        /// <summary>
        /// 复位任务
        /// </summary>
        public static byte TaskRestoration = 0x04;

        public static string GetTaskMes(byte task)
        {
            if (task == TaskLocate)
            {
                return "定位任务";
            }
            else if (task == TaskTake)
            {
                return "取货任务";
            }
            else if (task == TaskRelease)
            {
                return "放货任务";
            }
            else
            {
                return "复位任务";
            }
        }

        #endregion

        #region 货物状态

        /// <summary>
        /// 无货
        /// </summary>
        public static byte GoodsNo = 0x00;

        /// <summary>
        /// 有货
        /// </summary>
        public static byte GoodsYes = 0x01;

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
        /// 行车号
        /// </summary>
        /// <returns></returns>
        public byte ABCNum()
        {
            return GetSingleByte(2);
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        /// <returns></returns>
        public byte ActionStatus()
        {
            return GetSingleByte(3);
        }

        /// <summary>
        /// 设备状态
        /// </summary>
        /// <returns></returns>
        public byte DeviceStatus()
        {
            return GetSingleByte(4);
        }

        /// <summary>
        /// 命令状态
        /// </summary>
        /// <returns></returns>
        public byte CommandStatus()
        {
            return GetSingleByte(5);
        }

        /// <summary>
        /// 目标X坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsXsite()
        {
            return GetThridByte(6);
        }

        /// <summary>
        /// 目标Y坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsYsite()
        {
            return GetDoubleByte(9);
        }

        /// <summary>
        /// 目标Z坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsZsite()
        {
            return GetDoubleByte(11);
        }

        /// <summary>
        /// 当前任务
        /// </summary>
        /// <returns></returns>
        public byte CurrentTask()
        {
            return GetSingleByte(13);
        }

        /// <summary>
        /// 当前X坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentXsite()
        {
            return GetThridByte(14);
        }

        /// <summary>
        /// 当前Y坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentYsite()
        {
            return GetDoubleByte(17);
        }

        /// <summary>
        /// 当前Z坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentZsite()
        {
            return GetDoubleByte(19);
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <returns></returns>
        public byte FinishTask()
        {
            return GetSingleByte(21);
        }

        /// <summary>
        /// 货物状态
        /// </summary>
        /// <returns></returns>
        public byte GoodsStatus()
        {
            return GetSingleByte(22);
        }

        /// <summary>
        /// 故障信息
        /// </summary>
        /// <returns></returns>
        public byte ErrorMessage()
        {
            return GetSingleByte(23);
        }

        #endregion

        /// <summary>
        /// ABC 当前位置
        /// </summary>
        /// <returns></returns>
        public String GetCurrentSite()
        {
            long x = DataControl._mStools.BytesToLong(CurrentXsite());
            int y = DataControl._mStools.BytesToInt(CurrentYsite(), 0);
            int z = DataControl._mStools.BytesToInt(CurrentZsite(), 0);

            return Convert.ToString(x) +"-"+ Convert.ToString(y) + "-" + Convert.ToString(z);
        }

        /// <summary>
        /// ABC 目标位置
        /// </summary>
        /// <returns></returns>
        public String GetGoodsSite()
        {
            long x = DataControl._mStools.BytesToLong(GoodsXsite());
            int y = DataControl._mStools.BytesToInt(GoodsYsite());
            int z = DataControl._mStools.BytesToInt(GoodsZsite());

            return Convert.ToString(x) + "-" + Convert.ToString(y) + "-" + Convert.ToString(z);
        }

        #region 行车设备命令

        /// <summary>
        /// 行车—任务控制
        /// </summary>
        /// <param name="TaskType">任务类型</param>
        /// <param name="ABCNum">行车号</param>
        /// <param name="X">X轴坐标</param>
        /// <param name="Y">Y轴坐标</param>
        /// <param name="Z">Z轴坐标</param>
        /// <returns></returns>
        public static byte[] _TaskControl(byte TaskType, byte ABCNum, byte[] X, byte[] Y, byte[] Z)
        {
            //                     字头     设备号   控制码       X轴坐标        Y轴坐标     Z轴坐标     结束符
            return new byte[] { 0x90, 0x02, ABCNum, TaskType, X[1], X[2], X[3], Y[2], Y[3], Z[2], Z[3], 0xFF, 0xFE }; //int转byte[]补0成4位
        }

        /// <summary>
        /// 行车—终止任务
        /// </summary>
        /// <param name="ABCNum">行车号</param>
        /// <returns></returns>
        public static byte[] _StopTask(byte ABCNum)
        {
            //                     字头     设备号 控制码      X轴坐标        Y轴坐标     Z轴坐标     结束符
            return new byte[] { 0x90, 0x02, ABCNum, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFE };
        }

        #endregion
    }
}
