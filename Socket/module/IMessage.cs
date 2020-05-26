using System;

namespace Socket.module
{
    public interface IMessage
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreatedUTC
        {
            get;
        }

        MsgBuffer ToAciMsgBuffer();

        bool CanSend();

        string ClientId
        {
            get; set;
        }
    }
}
