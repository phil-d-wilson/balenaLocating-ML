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
A quick push to my BalenaLocating app using the [Balena CLI tools](https://www.balena.io/docs/reference/cli/#install-the-cli) showed me that I was receiving iBeacon advertisements:
![beacons](https://i.ibb.co/0KRsHvG/Beacons.jpg)
Awesome! Now to send them to the IOT Hub. I've done this part before as well, albeit using Microsoft's [IOT Edge](https://azure.microsoft.com/en-gb/services/iot-edge/) framework and C#. Still how hard can it be using Python?!?
![Disconnects](https://i.ibb.co/7bcd340/Disconnect-Exception.jpg)
Firstly, what immediately became apparent here, is that even without entering into Balena's excellent [Local Mode](https://www.balena.io/docs/learn/develop/local-mode/#develop-locally), the development-feedback loop was tight and easy to use. I pushed my app using the CLI, watched the build process run (I used Visual Studio Code and the terminal - it puts my code and build process in the same window which I like!):
![VSCode](https://i.ibb.co/NNgcpkM/VsCode.jpg)
I then can watch the sensor downloading the container, updating, running and view the console output all in Balena cloud portal!!!!
![Running in Balena Cloud](https://i.ibb.co/PFgsRRp/Running-In-Balena-Cloud.jpg)
Except that ---^ wasn't the output I got to begin with, remember....this was:
![Disconnects](https://i.ibb.co/7bcd340/Disconnect-Exception.jpg)
The simple job of sending my data to the IOT Hub, wasn't simple. No worries, a quick Google showed me this was a known issue with the latest version of the Azure IOT Device Python SDK ([issue#399](https://github.com/Azure/azure-iot-sdk-python/issues/399)) , so I changed my dockerfile to pull in the previous version......then this happened:
![OutOfMemory](https://i.ibb.co/C9m4HXj/Out-Of-Mememory.jpg)
This is the previous error that the latest version fixed. *sigh
#### Node.JS code
I actually spent quite a long time trying to get the Python IOT Hub code to work, since I'd not done BLE work in any other language, other than C# on Windows. However, I couldn't get past the two issues above, so I made a new dockerfile targeting Node.js:

    FROM balenalib/raspberrypi3-node:12.7.0
	WORKDIR /app
	RUN apt-get update && \
	apt-get install make g++ python2.7 bluetooth bluez libbluetooth-dev libudev-dev && \
	rm -rf /var/lib/apt/lists/*
	RUN ln -s /usr/bin/python2.7 /usr/bin/python
	COPY . .
	RUN npm install @abandonware/noble
	RUN npm install node-beacon-scanner
	RUN npm install azure-iot-device
	RUN npm install azure-iot-device-mqtt
	RUN npm install date-and-time
	RUN JOBS=MAX npm install --production --unsafe-perm && npm cache verify && rm -rf /tmp/*
	CMD ["npm", "start"]
Once again you can see the references to bluez, but this time it's being used (along with make and g++) to build the [@abandonware/noble](https://github.com/abandonware/noble)  module. This allowed me to start a BLE scanner:

<!--stackedit_data:
eyJoaXN0b3J5IjpbODUxODkyNTA4LDc0MTM5MTMxNywtMzgzMD
gxODgwLC0xNzIyNzM1NDQ1LDE5Nzc1NjA1NzAsMTk0OTkwODAy
MiwxMzE3NDcwODEzLDQ4NjIzOTA3NSwtMTUzNjUzMDU4NF19
-->