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
    class _bMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public int Parnumber { set { parnumber = value; } get { return parnumber; } }

        /// <summary>
        /// 
        /// </summary>
        public int OrderIndex { set { orderIndex = value; } get { return orderIndex; } }

        /// <summary>
        /// 
        /// </summary>
        public int TransportStructure { set { transportStructure = value; } get { return transportStructure; }  }

        /// <summary>
        /// 
        /// </summary>
        public int IKEY { set { iKEY = value; } get { return iKEY; } }

        /// <summary>
        /// 
        /// </summary>
        public int Status { set { status = value; } get { return status; } }

        private int parnumber;
        private int orderIndex;
        private int transportStructure;
        private int status = 0;
        private int iKEY;

        public override string ToString()
        {
            if (status == 1) //Order acknowledge
            {
                return string.Format("[Index {0}]  Order started, IKEY: {1}", orderIndex, iKEY);
            }

            else if (status == 3) //Order finished
            {//Will not use _b message to indicate order finished, instead using _s to get the IKEY of the order
                return  string.Format("[Index {0}]  Order finished", orderIndex);
            }

            else if (status == 25) //Cancel acknowledge (Cancel sent from Host)
            {
                return string.Format("[Index {0}]  Cancel acknowledge", orderIndex, transportStructure);
            }

            else if (status == 19) //Parameter accepted (P_Ack)
            {
                return string.Format("[Index {0}]  TS: {1} Status: Parameter accepted", orderIndex, transportStructure);
            }

            else if (status == 20) //Parameter not accepted (P_Nak)
            {
                return string.Format("[Index {0}]  TS: {1} Status: Parameter not accepted", orderIndex, transportStructure);
            }

            else if (status == 17) //Fatal error
            {
                return string.Format("[Index {0}]  TS: {1} Status: Fatal error, Order Cancelled", orderIndex, transportStructure);
            }

            else if (status == 14) //not activated index
            {
                return string.Format("[Index {0}]  No active index", orderIndex);
            }

            else if (status == 9) //Priority error
            {
                return string.Format("_b index: TS: {0} Status: Prio is to high", transportStructure);
            }

            else if (status == 10) //Invalid structure
            {
                return string.Format("_b index: TS: {0} Status: TS dont exist", transportStructure);
            }

            else if (status == 22) //Carrier number error
            {
                return string.Format("_b Vehicle id error, ID: {0}, Reply from _j?", transportStructure);
            }

            else if (status == 7) //parameter acknowledge
            {//This is commented out sine we always get this when writing a new state to the order.
                return "";// string.Format("_b parameter acknowledge, TS: {0}, Paramter id: {1}", transportStructure, parnumber);
            }

            else if (status == 15) //parameter number too high for this
            {
                return string.Format("_b parameter number to high, value:{0}", parnumber);
            }

            else if (status == 35) //change order instance priority acknowledge
            {
                return string.Format("[Index {0}]  Priority acknowledge, TS:{1}, Prio:{2}", orderIndex, transportStructure, parnumber);
            }
            else if (status == 2) //Carrier Allocated
            {
                return string.Format("[Index {0}]  Vehicle {1} allocated", orderIndex, parnumber);
            }
            else if (status == 37) //Carrier Connected
            {
                return string.Format("[Index {0}]  Vehicle {1} connected", orderIndex, parnumber);
            }
            else if (status == 4) //Order cancelled
            {
                return string.Format("[index {0}]  Cancelled", orderIndex);
            }
            else if (status == 18) //Parameter deleted
            {
                return string.Format("Parameter {0} deleted", parnumber);
            }
            else if (status == 24) //Invalid index number
            {
                return string.Format("Index number invalid");
            }
            else if (status == 27) //IKEY in use
            {
                return string.Format("IKEY {0} is already in use", iKEY);
            }
            else //Unknown status, not implemented
            {
                return "Nothing here:"+orderIndex;
            }
        }

    }
}
