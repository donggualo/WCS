using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace WCS_phase1.Functions
{
    public class SimpleTools
    {
        /// <summary>
        /// 判断DataTable是否无数据
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool IsNoData(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// int 转 byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }

        /// <summary>
        /// byte[] 转 int
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int BytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8)
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24));
            return value;
        }

        /// <summary>
        /// byte[] 转 String
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public string BytetToString(byte[] byteArray)//byte[]转String
        {
            var str = new System.Text.StringBuilder();
            for (int i = 0; i < byteArray.Length; i++)
            {
                str.Append(String.Format("{0:X} ", byteArray[i]));//var拼接
            }
            string s = str.ToString();
            return s;
        }
    }
}
