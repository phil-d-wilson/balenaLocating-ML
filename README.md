![BalenaLocating](https://i.ibb.co/svRSnf7/logo.png)
A proof of concept project used to explore the [balenaCloud](https://www.balena.io/cloud/) stack for provisioning Raspberry Pi's as Bluetooth sensors, and a (simple!) KNN classifier to predict indoor positioning of iBeacons.

### The Plan:
One of the most difficult problems when developing Internet of Things solutions is provisioning the connected devices/sensors. Typically these devices need some network connectivity credentials (e.g. WIFI SSID & passphrase) and some credentials to authenticate against the backend cloud solution. Manually configuring a single device is OK, but when multiple devices are needed for a solution, this provisioning can be a blocker to progress. In addition, rapid development of concepts relies on a tight development-feedback loop. The [balenaCloud](https://www.balena.io/cloud/) is a solution to both of these issues, and so I was keen to try it for myself!
In order to make sure I experienced the provisioning process fully, I wanted a problem to solve which required more than one device. I have also been looking for an opportunity to try and create an indoor Bluetooth Low Energy (BLE) locating system for some time having created more simple present/absent systems previously. And so the plan was hatched:
![Floorplan](https://i.ibb.co/pRJCqDm/Floorplan.jpg)
My plan involved placing three Raspberry Pi 3B/3B+ devices in different downstairs rooms of my (small) house, loading them with [iBeacon](https://developer.apple.com/ibeacon/) receiving code, connecting them to Microsoft Azure via an [IOT Hub](https://azure.microsoft.com/en-gb/services/iot-hub/), pulling the data into a database, training a Machine Learning model with that data, and then trying to predict which room a tag was in. 
![High Level Design](https://i.ibb.co/gt2LyCK/HLD.jpg)
### Provisioning a device:
First things first, I needed to get a single Raspberry Pi (RPi) connected to the Balena cloud:
![enter image description here](https://lh3.googleusercontent.com/bF2x2blz45zA-yuZIgTSoqyIG5j4Lx0E5h1GiJ_HhIfZIMGqSkStKcg4Ue_c9KhKOmaIar79y0TIIQ)

I signed up for a free Balena Cloud account, added a new application called BalenaLocating, and added a device. I selected a development image for the RPi, configured my home WIFI and downloaded the image. I then used Etcher to burn the image to the SD card. So far, so good. Here is the device connected to the Balena Cloud, which took me less than 30 minutes including waiting for the SD card to burn!
![Connected to Balena Cloud](https://i.ibb.co/jhNkGfQ/Device-Online-Wifi.jpg)
Next job: get some BLE and IOT Hub code onto the RPi and start sensing some iBeacons! "This will be easy", I thought, since I've done similar projects before......nope!
#### Python code
Having written Python code for iBeacon transcoding before, this seemed to obvious and quickest choice. So I created a docker file which loaded in some dependencies for Python:

        FROM balenalib/raspberrypi3-python:3-build
    
    WORKDIR /app
    
    RUN apt-get update && \
        apt-get install -y --allow-unauthenticated --no-install-recommends libcap2 libcap2-bin libboost-python1.62.0 python-pip libpython-dev python-bluez bluez bluez-tools python-dev libglib2.0-dev libboost-python-dev libboost-thread-dev libbluetooth-dev && \
        rm -rf /var/lib/apt/lists/* 
      
    RUN pip install azure-iot-device
    RUN pip install PyBluez
    RUN pip3 install beacontools[scan]
The main bits of interest here are PyBluez, which allows Python to access the Linux BlueZ Bluetooth stack and subscribe to BLE advertisements, and Azure-IOT-Device which allows us to send our data to Azure. I think added some Linux capabilities to the container, so that the code was allowed access to Bluetooth:

    RUN setcap 'cap_net_raw,cap_net_admin+eip' $(eval readlink -f `which python`)
And then call the Python script:

    ENTRYPOINT [ "python", "-u", "PythonScripts/main.py" ]
   The first part of this starts a beacon scanner:
   

    scanner = BeaconScanner(callback)
    scanner.start()
and registers a callback:

    async  def  callback(bt_addr, rssi, packet, additional_info):
        global HUB_MANAGER
        uuid = packet.uuid.upper()
        major =  str(packet.major)
        minor =  str(packet.minor)
        
	    print("Beacon found - UUID = %s, Major = %s, Minor = %s, RSSI = %s"  % (uuid, major, minor, str(rssi)))
        output =  "{\"BeaconDateTime\":\""  +  '{:%Y-%m-%d:%H-%M-%S}'.format(datetime.utcnow()) +  "\",\"DeviceName\":\""  +  str(mac) +  "\",\"BeaconName\":\""  + major +  "-"  + minor +  "\",\"Rssi\":"  +  str(rssi) +  "}"
        await HUB_MANAGER.forward_event_to_output(output)

<!--stackedit_data:
eyJoaXN0b3J5IjpbMTA0MDI0NTMyNCwtMzgzMDgxODgwLC0xNz
IyNzM1NDQ1LDE5Nzc1NjA1NzAsMTk0OTkwODAyMiwxMzE3NDcw
ODEzLDQ4NjIzOTA3NSwtMTUzNjUzMDU4NF19
-->