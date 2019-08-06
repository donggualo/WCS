using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS_phase1.NDC.Message
{
    internal class _vpilMessage
    {
        /// <summary>
        /// 小车ID
        /// </summary>
        public int CarId;


        public int Magic;

        /// <summary>
        /// PLC值位置
        /// </summary>
        public byte PlcLp1;

        /// <summary>
        /// PLC值
        /// </summary>
        public int Value1;


        public override string ToString()
        {
            if (PlcLp1 == 1)
            {

            }

            return "Carid:"+CarId + " Plclp1:"+ PlcLp1+" Value1:"+Value1;
        }
    }
}
