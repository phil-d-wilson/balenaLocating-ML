using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BalenaLocatingApi.Models;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace BalenaLocatingApi.Services
{
    public class HubService
    {
        private const string SEventHubsCompatibleEndpoint = "HUB CONNECTION STRING";
        private static EventHubClient _sEventHubClient;
        private Dictionary<string, iBeacon> _collection;

        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct, string tagId, int timeout)
        {
            var eventHubReceiver = _sEventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            Debug.WriteLine("Create receiver on partition: " + partition);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                Debug.WriteLine("Listening for messages on: " + partition);
                var events = await eventHubReceiver.ReceiveAsync(100, TimeSpan.FromSeconds(timeout));

                if (events == null) continue;
                foreach (var eventData in events)
                {
                    var data = Encoding.UTF8.GetString(eventData.Body.Array);
                    Debug.WriteLine("Message received on partition {0}:", partition);
                    Debug.WriteLine("  {0}:", data);

                    var beacon = JsonConvert.DeserializeObject<iBeacon>(data);
                    if (null == beacon) continue;
                    if (beacon.BeaconName != tagId) continue;
                    if (_collection.ContainsKey(beacon.DeviceName))
                    {
                        if (_collection[beacon.DeviceName].Rssi >= beacon.Rssi)
                        {
                            continue;
                        }

                        _collection[beacon.DeviceName] = beacon;
                        continue;
                    }
                    _collection.Add(beacon.DeviceName, beacon);
                }
            }
        }

        public async Task<HubSampleSet> SampleHubData(string tagId, int sampleTimeInSeconds = 15)
        {
            _collection = new Dictionary<string, iBeacon>();
            _sEventHubClient = EventHubClient.CreateFromConnectionString(SEventHubsCompatibleEndpoint);
            var runtimeInfo = await _sEventHubClient.GetRuntimeInformationAsync();
            var d2CPartitions = runtimeInfo.PartitionIds;

            var cts = new CancellationTokenSource();
            cts.CancelAfter(new TimeSpan(0,0,0,sampleTimeInSeconds));

            Task.WaitAll(d2CPartitions.Select(partition => ReceiveMessagesFromDeviceAsync(partition, cts.Token, tagId, sampleTimeInSeconds)).ToArray());
            cts.Dispose();
            await _sEventHubClient.CloseAsync();

            return new HubSampleSet
            {
                DeviceValues = _collection
            };
        }
    }
}
