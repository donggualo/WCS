using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
// This software is offered as is, with no warranties, and the user assumes all liability for its use.
// Kollmorgen Automation AB assume no responsibility or liability for errors or omissions or any actions
// resulting from the use of this software or the information contained herein.
// In no event shall Kollmorgen Automation AB be liable for any damages whatsoever,
// real or imagined, resulting from the loss of use, profits,
// or data whether or not we have been advised of the possibility of such damage. 
//
// In other words, we are letting you using the software, at your own risk,
// and try it for free so that you may determine if it suites your needs.
// By so doing, you are agreeing to the stated terms and conditions and accepting full
// responsibility for your own actions. 
//
namespace WCS_phase1.NDC.Message
{
    class _sMessage
    {
        /// <summary>
        /// 任务序列号
        /// </summary>
        public int OrderIndex { set { orderIndex = value; } get { return orderIndex; } }

        /// <summary>
        /// 
        /// </summary>
        public int TransportStructure { set { transportStructure = value; } get { return transportStructure; } }

        /// <summary>
        /// 
        /// </summary>
        public int Magic1{set { magic1 = value; } get { return magic1; } }

        public int Magic2 { set { magic2 = value; } get { return magic2; } }
        public int Magic3 { set { magic3 = value; } get { return magic3; } }

        /// <summary>
        /// 小车ID
        /// </summary>
        public int CarrierId { set { carrierId = value; } get { return carrierId; } }

        /// <summary>
        /// 
        /// </summary>
        public int Station { set { station = value; } get { return station; } }

        private int orderIndex;
        private int transportStructure;
        private int magic1;
        private int magic2;
        private int magic3;
        private int carrierId;
        private int station;


        public override string ToString()
        {
            if (transportStructure == 0)
            {
                if (orderIndex == 255)
                {
                    return string.Format("_s Not Valid, Vehicle ID: {0} do not exist ", carrierId);
                }
                else
                {
                    return string.Format("_s Not Valid, Index: {0} do not exist ", orderIndex);
                }
            }
            else if (magic1 == 1)
            {
                return string.Format("[Index {0}]  Order Start, Phase: ${1}", orderIndex, magic1);
            }
            else if (magic1 == 2)
            {
                return string.Format("[Index {0}]  Fetch: {1} Deliver: {2}, Phase: ${3}", orderIndex, magic2, magic3, magic1);
            }
            else if (magic1 == 3 && magic2 != 142)//No redirect
            {
                return string.Format("[Index {0}]  Move to load, Phase: ${1}  ", orderIndex, magic1);
            }
            else if (magic1 == 3 && magic2 == 142)//Redirect
            {
                return string.Format("[Index {0}]  Redirect, Phase: ${1}  ", orderIndex, magic2);
            }
            else if (magic1 == 4)//到达接货点
            {
                return string.Format("[Index {0}]  Load host sync, Phase: ${1}", orderIndex, magic1);
            }
            else if (magic1 == 6)//接货完成
            {
                return string.Format("[Index {0}]  Loaded host sync, station: {2} Phase: ${1}", orderIndex, magic1, station);
            }
            else if (magic1 == 8)//到达卸货点 
            {
                return string.Format("[Index {0}]  Unload host sync, Phase: ${1}", orderIndex, magic1);
            }
            else if (magic1 == 10)//卸货完成
            {
                return string.Format("[Index {0}]  Unloaded host sync, station: {2} Phase: ${1:X}", orderIndex, magic1, station);
            }
            else if (magic1 == 11) //Using this instead of _b message to indicate order done, to have the IKEY
            {
                return string.Format("[Index {0}]  Order Finished, IKEY: {1}", orderIndex, magic3);
            }
            else if (magic1 == 32) //Carwash sends this request
            {
                return string.Format("[Index {0}]  Redirect station needed, Vehicle: {1}", orderIndex, magic2);
            }
            else if (magic1 == 33)
            {
                string hexValue = magic1.ToString();
                return string.Format("[Index {0}]  Redirect request fetch, Phase ${1:X}", orderIndex, magic1);
            }
            else if (magic1 == 34) //Using this instead of _b message to indicate order done, to have the IKEY
            {
                return string.Format("[Index {0}]  Redirect request deliver, Phase ${1:X}", orderIndex, magic1);
            }
            else if (magic1 == 48 && magic2 == 0)
            {
                return string.Format("[Index {0}]  Cancel accepted, Phase ${1:X}", orderIndex, magic1);
            }
            else if (magic1 == 49)//Cancel
            {
                return string.Format("[Index {0}]  Fetch station invalid, {1}, cancel", orderIndex, magic2);
            }
            else if (magic1 == 50)//Cancel
            {
                return string.Format("[Index {0}]  Drop station invalid, {1}, cancel", orderIndex, magic2);
            }
            else if (magic1 == 254)
            {
                return string.Format("[Index {0}]  Redirecting Vehicle to stn: {1}, Phase ${2:X}", orderIndex, magic2, magic1);
            }
            else if (magic1 == 255)
            {
                return string.Format("[Index {0}]  Cancel, Phase ${1:X}", orderIndex, magic1);
            }
            else
            {
                return "Nothing here:"+orderIndex;
            }
        }
    }
}
