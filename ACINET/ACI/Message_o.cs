using System;
using System.Runtime.InteropServices;
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
namespace NDC8.ACINET.ACI
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_o
    {
        public ushort magic;
        public byte sp0;
        public byte itemc;

        public ushort oix;
        public ushort sp1;
        public UInt32 stime;
        public byte strp;
        public byte trp;
        public byte row;

        public byte olist;
        public byte ostate;
        public byte ostatus;
        public byte opri;
        public byte trig;
        public byte trig_p0;
        public byte trig_p1;

        public byte cid;
        public byte mainstat;
        public ushort cstatus;
        public ushort movestate;
        public ushort pstn;
        public ushort dstn;

    }

    public class Message_o : ACIMessageBase
    {

        public Message_o(byte[] msg)
            : base("o")
        {
            ACI_MSG_o msg_o = BufferToStruct<ACI_MSG_o>(msg);

            Magic = shiftBytes(msg_o.magic);

            ItemCode = msg_o.itemc;

            OrderIndex = shiftBytes(msg_o.oix);

            StartTime = shiftBytes(msg_o.stime);

            StartTransportStructure = msg_o.strp;

            TransportStructure = msg_o.trp;

            Row = msg_o.row;

            OrderListMonitor = msg_o.olist;

            OrderState = msg_o.ostate;

            OrderStatus = msg_o.ostatus;

            OrderPriorty = msg_o.opri;

            OrderTrigger = msg_o.trig;

            OrderTrigger_Parameter0 = msg_o.trig_p0;

            OrderTrigger_Parameter1 = msg_o.trig_p1;

            CarrierId = msg_o.cid;

            MainStatus = msg_o.mainstat;

            CarrierStatus = shiftBytes(msg_o.cstatus);

            MoveStat = shiftBytes(msg_o.movestate);

            PreviusStation = shiftBytes(msg_o.pstn);

            DestinationStation = shiftBytes(msg_o.dstn);
        }

        public int Magic
        {
            get; private set;
        }
        public byte ItemCode
        {
            get; private set;
        }
        public int OrderIndex
        {
            get; private set;
        }

        public UInt32 StartTime
        {
            get; private set;
        }

        public byte StartTransportStructure
        {
            get; private set;
        }

        public byte TransportStructure
        {
            get; private set;
        }

        public byte Row
        {
            get; private set;
        }

        public byte OrderListMonitor
        {
            get; private set;
        }

        public byte OrderState
        {
            get; private set;
        }

        public int OrderStatus
        {
            get; private set;
        }

        public byte OrderPriorty
        {
            get; private set;
        }

        public byte OrderTrigger
        {
            get; private set;
        }

        public byte OrderTrigger_Parameter0
        {
            get; private set;
        }

        public byte OrderTrigger_Parameter1
        {
            get; private set;
        }

        public byte CarrierId
        {
            get; private set;
        }

        public byte MainStatus
        {
            get; private set;
        }

        public int CarrierStatus
        {
            get; private set;
        }

        public int MoveStat
        {
            get; private set;
        }

        public int PreviusStation
        {
            get; private set;
        }

        public int DestinationStation
        {
            get; private set;
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', Magic '{1}', ItemCode '{2}', Orderindex '{3}', StartTime '{5}', StartTransportStructure '{6}', TransportStructure '{7}', Row '{8}', OrderListMonitor '{9}', OrderState'{10}', OrderStatus '{11}', OrderPriorty '{12}', OrderTrigger '{13}', OrderTrigger_Parameter0 '{14}', OrderTrigger_Parameter1 '{15}', CarrierId '{16}', MainStatus '{17}', CarrierStatus '{18}', MoveStat '{19}', PreviusStation '{20}', DestinationStation '{21} - {22}' "
                , Type, Magic, ItemCode, OrderIndex, StartTime, StartTransportStructure, TransportStructure, Row, OrderListMonitor, OrderState, OrderStatus, OrderPriorty, OrderTrigger, OrderTrigger_Parameter0, OrderTrigger_Parameter1, CarrierId
                , MainStatus, CarrierStatus, MoveStat, PreviusStation, DestinationStation, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}

