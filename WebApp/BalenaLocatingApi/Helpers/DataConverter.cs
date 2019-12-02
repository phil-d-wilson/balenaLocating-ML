using System.Collections.Generic;
using BalenaLocatingApi.Models;

namespace BalenaLocatingApi.Helpers
{
    public static class DataConverter
    {

        public static double[] Convert(HubSampleSet result)
        {
            var output = new List<double>();
            var device1 = result.DeviceValues.ContainsKey("56c9e0a87b4a795e09da5579420eed32")
                ? result.DeviceValues["56c9e0a87b4a795e09da5579420eed32"].Rssi
                : -1001;
            output.Add(device1);
            var device2 = result.DeviceValues.ContainsKey("4a3fde3f9fe79bb5ce47718413502b5f")
                ? result.DeviceValues["4a3fde3f9fe79bb5ce47718413502b5f"].Rssi
                : -1001;
            output.Add(device2);
            var device3 = result.DeviceValues.ContainsKey("abf31fdd6af3691cd640e84b68a12009")
                ? result.DeviceValues["abf31fdd6af3691cd640e84b68a12009"].Rssi
                : -1001;
            output.Add(device3);

            return output.ToArray();
        }
    }
}
