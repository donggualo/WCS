using Module;
using Socket.message;
using System;
using System.Diagnostics;

namespace Socket.module
{
    public static class MessageParser
    {
        public static IBaseModule Parse(DevType type, byte[] msg)
        {
            try
            {
                switch (type)
                {
                    case DevType.行车:
                        return new AwcMessage(msg).Module;
                    case DevType.固定辊台:
                        return new FrtMessage(msg).Module;
                    case DevType.摆渡车:
                        return new ArfMessage(msg).Module;
                    case DevType.运输车:
                        return new RgvMessage(msg).Module;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(string.Format("Failed to parse message e:{0}", ex.Message));
                return null;
            }
        }
    }
}
