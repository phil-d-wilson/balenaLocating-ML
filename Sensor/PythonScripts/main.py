import os
import asyncio
import sys
from azure.iot.device.aio import IoTHubDeviceClient
from uuid import getnode as get_mac
# import blescan
from datetime import datetime
from beacontools import BeaconScanner

HUB_MANAGER = None

async def callback(bt_addr, rssi, packet, additional_info):
        global HUB_MANAGER

        uuid = packet.uuid.upper()
        major = str(packet.major)
        minor = str(packet.minor)
        mac = get_mac() 

        print("Beacon found - UUID = %s, Major = %s, Minor = %s, RSSI = %s" % (uuid, major, minor, str(rssi)))
        output = "{\"BeaconDateTime\":\"" + '{:%Y-%m-%d:%H-%M-%S}'.format(datetime.utcnow()) + "\",\"DeviceName\":\"" + str(mac) +  "\",\"BeaconName\":\"" + major + "-" + minor + "\",\"Rssi\":" + str(rssi) + "}"
        await HUB_MANAGER.forward_event_to_output(output)

class HubManager(object):

        def __init__(self):
                conn_str = "DEVICECONNECTIONSTRINGHERE"

                # Create instance of the device client using the connection string
                self.device_client = IoTHubDeviceClient.create_from_connection_string(conn_str)

        async def _init(self):
                # Connect the device client.
                await self.device_client.connect()

        # Forwards the message received onto the next stage in the process.
        async def forward_event_to_output(self, event):
                await self.device_client.send_message(event)

async def create_hubmanager():
    hubmanager = HubManager()
    await hubmanager._init()
    return hubmanager

async def main():
        try:
                global HUB_MANAGER
                print("\nPython %s\n" % sys.version)

                HUB_MANAGER = await create_hubmanager()

                try:
                        scanner = BeaconScanner(callback,
                        )
                        scanner.start()
                        print("BLE thread started")

                except:
                        print("Error accessing bluetooth device...")
                        sys.exit(1)
        except KeyboardInterrupt:
                print("Module stopped")

if __name__ == '__main__':      
        loop = asyncio.get_event_loop()
        loop.run_until_complete(main())
        loop.close()