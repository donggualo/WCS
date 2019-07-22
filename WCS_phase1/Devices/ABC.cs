using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_phase1.Functions;

namespace WCS_phase1.Devices
{
    /// <summary>
    /// 自动行车 Automatic Bridge Crane
    /// </summary>
    public class ABC : Device
    {
        public ABC(string name) : base(name)
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
        /// 命令状态
        /// </summary>
        /// <returns></returns>
        public byte CommandStatus()
        {
            return GetSingleByte(3);
        }

        /// <summary>
        /// 目标X坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsXsite()
        {
            return GetThridByte(4);
        }

        /// <summary>
        /// 目标Y坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsYsite()
        {
            return GetDoubleByte(7);
        }

        /// <summary>
        /// 目标Z坐标
        /// </summary>
        /// <returns></returns>
        public byte[] GoodsZsite()
        {
            return GetDoubleByte(9);
        }

        /// <summary>
        /// 当前任务
        /// </summary>
        /// <returns></returns>
        public byte CurrentTask()
        {
            return GetSingleByte(11);
        }

        /// <summary>
        /// 当前X坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentXsite()
        {
            return GetThridByte(12);
        }

        /// <summary>
        /// 当前Y坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentYsite()
        {
            return GetDoubleByte(15);
        }

        /// <summary>
        /// 当前Z坐标
        /// </summary>
        /// <returns></returns>
        public byte[] CurrentZsite()
        {
            return GetDoubleByte(17);
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <returns></returns>
        public byte FinishTask()
        {
            return GetSingleByte(19);
        }

        /// <summary>
        /// 货物状态
        /// </summary>
        /// <returns></returns>
        public byte GoodsStatus()
        {
            return GetSingleByte(20);
        }

        /// <summary>
        /// 故障信息
        /// </summary>
        /// <returns></returns>
        public byte ErrorMessage()
        {
            return GetSingleByte(21);
        }

        #endregion

        /// <summary>
        /// ABC 当前位置
        /// </summary>
        /// <returns></returns>
        public String GetCurrentSite()
        {
            SimpleTools tools = new SimpleTools();
            int x = tools.bytesToInt(CurrentXsite(), 0);
            int y = tools.bytesToInt(CurrentYsite(), 0);
            int z = tools.bytesToInt(CurrentZsite(), 0);

            return Convert.ToString(x) +"-"+ Convert.ToString(y) + "-" + Convert.ToString(z);
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
            return new byte[] { 0x90, 0x02, ABCNum, TaskType, X[0], X[1], X[2], Y[0], Y[1], Z[0], Z[1], 0xFF, 0xFE };
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
