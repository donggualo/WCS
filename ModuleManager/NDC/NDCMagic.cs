using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NdcManager.NDC
{
    /// <summary>
    /// 任务状态
    /// </summary>

    public enum NDCMagic
    {
        OrderStart =1,
        OrderInfo = 2,
        AboutRedirect = 3,
        LoadHost = 4,
        LoadedHostSync = 6,
        UnloadHostSync = 8,
        UnloadedHostSyncStation = 10,
        OrderFinished = 11,
        RedirectStationNeeded = 32,
        RedirectRequestFetch = 33,
        RedirectRequestDeliver = 34,
        CancelAccepted = 48,
        FetchStationInvalid = 49,
        DropStationInvalid = 50,
        RedirectingVehicleToStn = 254,
        Cancel = 255
    }

    public class NDCMagicStr
    {
        private string[] magic = new string[255];

        public NDCMagicStr()
        {
            magic[1] = "[Index {0}]  Fetch: {1} Deliver: {2}, Phase: ${3}";
            magic[2] = "[Index {0}]  Move to load, Phase: ${1}";
            magic[3] = "[Index {0}]  Redirect, Phase: ${1}";
            magic[4] = "[Index {0}]  Load host sync, Phase: ${1}";
            magic[6] = "[Index {0}]  Loaded host sync, station: {2} Phase: ${1}";
            magic[8] = "[Index {0}]  Unload host sync, Phase: ${1}";
            magic[10] = "[Index {0}]  Unloaded host sync, station: {2} Phase: ${1:X}";
            magic[11] = "[Index {0}]  Order Finished, IKEY: {1}";
            magic[32] = "[Index {0}]  Redirect station needed, Vehicle: {1}";
            magic[33] = "[Index {0}]  Redirect request fetch, Phase ${1:X}";
            magic[34] = "[Index {0}]  Redirect request deliver, Phase ${1:X}";
            magic[48] = "[Index {0}]  Cancel accepted, Phase ${1:X}";
            magic[49] = "[Index {0}]  Fetch station invalid, {1}, cancel";
            magic[50] = "[Index {0}]  Drop station invalid, {1}, cancel";
            magic[254] = "[Index {0}]  Redirecting Vehicle to stn: {1}, Phase ${2:X}";
            magic[255] = "[Index {0}]  Cancel, Phase ${1:X}";
        }

        public string Get(NDCMagic index)
        {
            if ((int)index > magic.Length) return "";
            return magic[(int)index] ?? "Nothing here:index=" + (int)index;

        }
    }
}
