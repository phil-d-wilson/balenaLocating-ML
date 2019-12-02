using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BalenaLocatingApi.Models
{
    public class iBeacon
    {
        public string BeaconDateTime { get; set; }
        public string DeviceName { get; set; }
        public string BeaconName { get; set; }
        public int Rssi { get; set; }
    }
}
