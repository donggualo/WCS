using System.Collections.Generic;
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
    public struct ACI_MSG_s
    {
        public ushort index;
        public byte trpsno;
        public byte ordstat;
        public ushort magic;
        public ushort magic_2;
        public byte carno;
        public byte spare;
        public ushort status;
        public ushort station;
        public ushort magic_3;
        public ushort NoValidLp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public ushort[] LpValues;
    }

    public class Message_s : ACIMessageBase
    {
        private List<int> m_Values;

        public Message_s(byte[] msg) : base("s")
        {
            ACI_MSG_s msg_s = BufferToStruct<ACI_MSG_s>(msg);

            Index = shiftBytes(msg_s.index);

            TransportStructure = msg_s.trpsno;

            Status = msg_s.ordstat;

            Magic = shiftBytes(msg_s.magic);

            Magic2 = shiftBytes(msg_s.magic_2);

            CarrierNumber = (msg_s.carno);

            CarrierStatus = shiftBytes(msg_s.status);

            CarrierStation = shiftBytes(msg_s.station);

            Magic3 = shiftBytes(msg_s.magic_3);

            NoVal = shiftBytes(msg_s.NoValidLp);

            if(NoVal != 0)
            {
                m_Values = new List<int>();
                for(int i = 0; i < NoVal; i++)
                {
                    m_Values.Add(shiftBytes(msg_s.LpValues[i]));
                }
            }
        }

        public int Index
        {
            get; private set;
        }

        public int TransportStructure
        {
            get; private set;
        }

        public int Status
        {
            get; private set;
        }

        public int Magic
        {
            get; private set;
        }

        public int Magic2
        {
            get; private set;
        }

        public int CarrierNumber
        {
            get; private set;
        }

        public int CarrierStatus
        {
            get; private set;
        }

        public int CarrierStation
        {
            get; private set;
        }

        public int Magic3
        {
            get; private set;
        }

        public int NoVal
        {
            get; private set;
        }

        public List<int> Values
        {
            get
            {
                return m_Values;
            }
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', Order '{1}', AGV '{2}', AGVState '{3}', AGV Station '{4}', Magic1'{5}', Magic2'{6}', Magic3'{7}' LocalParameters '{8}', Time :{9} "
                , Type, Index, CarrierNumber, CarrierStatus, CarrierStation, Magic, Magic2, Magic3, (NoVal == 0 ? null : string.Join(",", Values.ToArray())), CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
