namespace SocketManager.module
{
    public class ISocketConst
    {
        /// <summary>
        /// 头部值
        /// </summary>
        internal const int HEADTAIL_SIZE = 2;

        /// <summary>
        /// 尾部值
        /// </summary>
        internal const int TAIL_KEY = 65534; //[0xFF,0xFE]
        internal const int BUFFER_SIZE = 128;

        /// <summary>
        /// 设备值
        /// </summary>
        internal const int AWC_SIZE = 27;
        internal const int RGV_SIZE = 23;
        internal const int ARF_SIZE = 20;
        internal const int FRT_SIZE = 19;
        internal const int PKL_SIZE = 12;

        internal const int CODE_SIZE = 47;

        /// <summary>
        /// 字符串长度
        /// </summary>
        public const int MIN_STRING_SIZE = 10;
        public const int NOR_STRING_SIZE = 20;
        public const int MID_STRING_SIZE = 50;
        public const int MAX_STRING_SIZE = 100;
        public const int TIME_SIZE = 24;
    }
}
