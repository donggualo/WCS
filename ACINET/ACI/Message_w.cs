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
    public struct ACI_MSG_w
    {
        public ushort index;
        public byte nopar;
        public byte p0_no;
        public byte p1_no;
        public byte p2_no;
        public byte p3_no;
        public byte p4_no;
        public ushort p0_value;
        public ushort p1_value;
        public ushort p2_value;
        public ushort p3_value;
        public ushort p4_value;
    }

    public class Message_w : ACIMessageBase
    {

        public Message_w(byte[] msg) : base("w")
        {
            ACI_MSG_w msg_w = BufferToStruct<ACI_MSG_w>(msg);

            Index = shiftBytes(msg_w.index);

            NumberOfParameters = msg_w.nopar;

            Parameter0Number = msg_w.p0_no;

            Parameter1Number = msg_w.p1_no;

            Parameter2Number = msg_w.p2_no;

            Parameter3Number = msg_w.p3_no;

            Parameter4Number = msg_w.p4_no;

            Parameter0Value = shiftBytes(msg_w.p0_value);

            Parameter1Value = shiftBytes(msg_w.p1_value);

            Parameter2Value = shiftBytes(msg_w.p2_value);

            Parameter3Value = shiftBytes(msg_w.p3_value);

            Parameter4Value = shiftBytes(msg_w.p4_value);
        }

        public int Index
        {
            get; private set;
        }

        public int NumberOfParameters
        {
            get; private set;
        }
        public int Parameter0Number
        {
            get; private set;
        }
        public int Parameter1Number
        {
            get; private set;
        }
        public int Parameter2Number
        {
            get; private set;
        }

        public int Parameter3Number
        {
            get; private set;
        }

        public int Parameter4Number
        {
            get; private set;
        }

        public int Parameter0Value
        {
            get; private set;
        }

        public int Parameter1Value
        {
            get; private set;
        }

        public int Parameter2Value
        {
            get; private set;
        }

        public int Parameter3Value
        {
            get; private set;
        }

        public int Parameter4Value
        {
            get; private set;
        }

        public override string ToString()
        {
            return string.Format("Message of type '{0}', OrderIndex:{1} ,LocalParameternumber:{2} Value = {7}, LocalParameternumber:{3},Value = {8}, LocalParameternumber:{4},Value = {9}, LocalParameternumber:{5},Value = {10}, LocalParameternumber:{6},Value = {11} Time :{12}"
                , Type, Index, Parameter0Number, Parameter1Number, Parameter2Number, Parameter3Number, Parameter4Number, Parameter0Value, Parameter1Value, Parameter2Value, Parameter3Value, Parameter4Value, CreatedUTC.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
