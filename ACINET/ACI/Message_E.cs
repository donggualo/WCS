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
    public enum E_CODE : ushort
    {
        E_MSG_TYPE_SYSER = 1,           // Old 'e' msg.
        E_MSG_TYPE_SYSEV = 2,           // Old 'x' msg.
        E_MSG_TYPE_USEREV = 3           // Old 't' msg.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACI_MSG_E
    {
        public ushort unit_type;        // unit type
        public ushort unit_id;          // unit id
        public ushort unit_ix;          // unit index
        public ushort ph_mod;           // physical module id
        public ushort sw_mod;           // software module id
        public ushort reserved;

        public ushort e_type;           // Syserr(1), Sysevt(2), Userevt(3)
        public UInt32 e_code;           // runtime error code

        public UInt32 utc_time;         // when expressed in UTC
        public Int32 zone_offset;       // Time zone offset in seconds

        public UInt32 location;         // Location index
        public UInt32 reserved2;

        public UInt32 parameter1;
        public UInt32 parameter2;
        public UInt32 parameter3;
        public UInt32 parameter4;
    }

    public class Message_E : ACIMessageBase
    {

        public Message_E(byte[] msg) : base("E")
        {
            ACI_MSG_E msg_e = BufferToStruct<ACI_MSG_E>(msg);

            UnitType = shiftBytes(msg_e.unit_type);

            UnitId = shiftBytes(msg_e.unit_id);

            UnitIndex = shiftBytes(msg_e.unit_ix);

            PhysicalModule = shiftBytes(msg_e.ph_mod);

            SoftwareModule = shiftBytes(msg_e.sw_mod);

            EventType = shiftBytes(msg_e.e_type);

            EventCode = shiftBytes(msg_e.e_code);

            UtcTime = shiftBytes(msg_e.utc_time);

            ZoneOffset = shiftBytes(msg_e.zone_offset);

            LocationIndex = shiftBytes(msg_e.location);

            Parameter1 = shiftBytes(msg_e.parameter1);

            Parameter2 = shiftBytes(msg_e.parameter2);

            Parameter3 = shiftBytes(msg_e.parameter3);

            Parameter4 = shiftBytes(msg_e.parameter4);
        }

        public int UnitType
        {
            get; private set;
        }

        public int UnitId
        {
            get; private set;
        }
        public int UnitIndex
        {
            get; private set;
        }

        public int PhysicalModule
        {
            get; private set;
        }

        public int SoftwareModule
        {
            get; private set;
        }

        public int EventType
        {
            get; private set;
        }

        public UInt32 EventCode
        {
            get; private set;
        }

        public UInt32 UtcTime
        {
            get; private set;
        }

        public Int32 ZoneOffset
        {
            get; private set;
        }

        public UInt32 LocationIndex
        {
            get; private set;
        }

        public UInt32 Parameter1
        {
            get; private set;
        }

        public UInt32 Parameter2
        {
            get; private set;
        }

        public UInt32 Parameter3
        {
            get; private set;
        }

        public UInt32 Parameter4
        {
            get; private set;
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', UnitType '{1}', UnitId '{2}', UnitIndex '{3}', Type '{4}', Code '{5}', Utc Time '{6}', Zone offset '{7}', Parameter1 '{8}', Parameter2 '{9}', Parameter3 '{10}', Parameter4 '{11}'  - '{11}'", Type, UnitType, UnitId, UnitIndex, EventType, EventCode, UtcTime, ZoneOffset, Parameter1, Parameter2, Parameter3, Parameter4, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
