![BalenaLocating](https://i.ibb.co/svRSnf7/logo.png)
A proof of concept project used to explore the [balenaCloud](https://www.balena.io/cloud/) stack for provisioning Raspberry Pi's as Bluetooth sensors, and a (simple!) KNN classifier to predict indoor positioning of iBeacons.

### The Plan:
One of the most difficult problems when developing Internet of Things solutions is provisioning the connected devices/sensors. Typically these devices need some network connectivity credentials (e.g. WIFI SSID & passphrase) and some credentials to authenticate against the backend cloud solution. Manually configuring a single device is OK, but when multiple devices are needed for a solution, this provisioning can be a blocker to progress. In addition, rapid development of concepts relies on a tight development-feedback loop. The [balenaCloud](https://www.balena.io/cloud/) is a solution to both of these issues, and so I was keen to try it for myself!
In order to make sure I experienced the provisioning process fully, I wanted a problem to solve which required more than one device. I have also been looking for an opportunity to try and create an indoor Bluetooth Low Energy (BLE) locating system for some time having created more simple present/absent systems previously. And so the plan was hatched:
![Floorplan](https://i.ibb.co/pRJCqDm/Floorplan.jpg)
My plan involved placing three Raspberry Pi 3B/3B+ devices in different downstairs rooms of my (small) house, loading them with [iBeacon](https://developer.apple.com/ibeacon/) receiving code, connecting them to Microsoft Azure via an [IOT Hub](https://azure.microsoft.com/en-gb/services/iot-hub/), pulling the data into a database, training a Machine Learning model with that data, and then trying to predict which room a tag was in. 
![High Level Design](https://i.ibb.co/gt2LyCK/HLD.jpg)
The reason a Machine Learning model is necessary, is due to the weird and wonderful work of RF propagation. Suffice to say, it's not as simple as find the sensor receiving the strongest iBeacon signal!

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

    const  BeaconScanner  =  require('node-beacon-scanner');
    const  scanner  =  new  BeaconScanner();
    scanner.startScan().then(() => {...
grab the BLE iBeacon advertisements:

    scanner.onadvertisement  = (ad) => {
	output  =  "{\"BeaconDateTime\":\""  +  date.format(new  Date(), 'YYYY/MM/DD HH:mm:ss') +  "\",\"DeviceName\":\""  +  mac  +  "\",\"BeaconName\":\""  +  ad.iBeacon.major  +  "-"  +  ad.iBeacon.minor  +  "\",\"Rssi\":"  +  ad.rssi  +  "}"
and ping them to the IOT hub (fingers crossed!):

    var  client  =  Client.fromConnectionString(deviceConnectionString, Protocol);
    var  message  =  new  Message(output);
    client.sendEvent(message);
And....
![IOT Hub Listener](https://i.ibb.co/kBbXm9V/Node-Messages-Receieved.jpg)
That ----^ is a listener attached to the IOT Hub, and those events are the formatted iBeacon advertisements sent from the RPi!!!
### Multiple devices
OK, so now I had a single Raspberry Pi, connected to the Balena Cloud, downloading a docker container which received iBeacon advertisements and sent them to Azure via an IOT Hub. Next job, scale that up to three RPi's. Again, ridiculously easy: I used Etcher to burn the image twice more, plugged the cards into the Pi's and powered them up. Another 30 minutes later:
  ![Three devices](https://i.ibb.co/Cv23Qyc/Three-Devices.jpg)
  Balena even let me tag them, so that I could remember which device was where:
  ![Tagged devices](https://i.ibb.co/PMcNJDW/Tagged-Devices.jpg)
  Now I had three lots of iBeacon telemetry coming into the IOT Hub. Better do something with it all!
  ### KNN Classification Machine Learning Algorithm
  Machine Learning (ML) is code which uses data from past runs to improve predictions made in future runs. One form of ML predictions is classifying data whereby the algorithm uses two or more predictor values to predict a class, such as the risk (class = low, medium, high) of a machine failure based on some metrics (e.g. number of hours running, temperature, operating speed). In simple terms, you train the model by feeding it examples of each class with the associated predictor values (e.g. 365 days running, temperature 150 degrees Celsius, 10,000RPM = HIGH). You then use the model to predict the class for other predictor values.
  The K-Nearest-Neighbours (KNN) algorithm is a relatively simple example of a classifier, which works by plotting all of the (normalised!) training data (in memory) like a grid, placing the unknown item in the grid, and then finding the K number of training points closest to it. Whatever the most frequent class of the neighbours is, is the predicted class of the unknown item. There are lots of [online articles](https://towardsdatascience.com/machine-learning-basics-with-the-k-nearest-neighbors-algorithm-6a6e71d01761?gi=89232be24566) which explain it much better than I can here.
  For my application, what I needed to do, was put an iBeacon tag ![tag](https://lh3.googleusercontent.com/u-zc5jVe3lCfLftd4k8uX_bRkpNswLwDB9rcqjGxj57DnRNEAI2ncJfJXMOos8ONPln0OJ990GiP3Q) into each location in my house, and capture the location and the Received Signal Strength Indicator ([RSSI](https://en.wikipedia.org/wiki/Received_signal_strength_indication)) values from the three RPIs at that moment. This would form a tuple that I can feed into my KNN model later:
  
|Location|Device 1  | Device 2| Device 3|
|--|--|--|--|
| Office | -35 | -65 | -95 |

This (dummy) example is adding a point to the models grid to show that the tag is in the office when it's closer to Device 1, further away from Device 2 and further still from Device 3. If I do that, multiple times, from each location (class) that the model *should* be able to predict which location the tag is from future readings. Let's try!

### Collecting the training data
First I wrote some code in a C# WebApi application which connected to the IOT Hub (it has four partitions, so I actually needed to spin up four listeners), sampled the telemetry for a particular iBeacon UUID for 15 seconds and returned the data:

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
Each listener added the strongest RSSI found for the specific tag. Remember I've got three sensors, so I need to find the value for each one for my training tuple.
This was then stored in an Azure Table. I did this 7 times for each location:
![Training Data](https://i.ibb.co/BGvmD0d/Training-Data.jpg)
Lots of factors affect the received signal (e.g. reflections, signal clashes, fluctuating beacon power, weather conditions, passing pigeons....etc) which is why you need multiple tuples per class.
This gave me my training data!

### Creating the KNN classifier
Initially I wanted to use the [Azure Machine Learning service](https://azure.microsoft.com/en-gb/services/machine-learning/) to make my model, train it and then use it via it's hosted Web Service. In fact I went so far as to make the model:
![enter image description here](https://i.ibb.co/wz2wC8x/Training-Azure-Model.jpg)
which gave good indications upon evaluation that it would be a strong predictor for tag locations:
![enter image description here](https://i.ibb.co/4RGPJ3f/Confusion-Matrix.jpg)
However, the Azure Machine Learning service proved cost prohibitive for me to leave running so that the web service could be used by my API code. So instead I coded my own (simple) version of the classifier. Simply put, this:

 1. Finds the distance from the unknown item to the training tuples
 2. Orders the result from closest to furthest away
 3. Finds the K nearest items
 4. Identifies the most frequent class in the result set

You can find my implementation in this repo. I chose K=3  for my model because....I like 3 and it seemed to work.
### Testing the model!
I re-used the IOT Hub sampling code from the training API call, but this time rather than providing a location value and storing the RSSI values, I fed it into the model and asked it to predict the location:

     var result = await _hubService.SampleHubData("28851-396", 10);
                var formattedResult = DataConverter.Convert(result);
                var classification = _classificationService.Analyze(formattedResult);
                var output = (Locations) classification;
The `28851-396` is the UUID Major and Minor of the tag I was testing with, by the way. And I'm only asking for a 10 second sample of the telemetry, to keep things a bit quicker. My tag is broadcasting every 5 seconds, so this gives me two possible beacons to try and receive.
The predicted class (i.e. location) is sent back from the API call.

### It works!

[![IMAGE ALT TEXT HERE](https://i.ibb.co/xFwg8Cr/YouTube.jpg)](https://youtu.be/1Ua-MyN-3f8)
[Click to watch the video]


### Lessons Learned

 - The Balena Cloud stack allows you to provision multiple devices in minutes (with an [Etcher Pro](https://www.balena.io/etcher/pro/) it would be even quicker!)
 - The Balena development-feedback loop, even without using local mode, is tight and helpful.
 - Microsoft need to spend more time testing their Python SDKs
 - Apparently I can write (simple) Node.JS code rather quickly
 - It's possible to take 3 Raspberry Pi's, an iBeacon tag, and a KNN algorithm and accurately predict which room of a small house the tag is in. 

### Future Steps
There's lots more things which could (and may!) be done from here. Firstly there are improvements to be made to this first PoC:

 - Using Balena environment variables for the IOT Hub connection string
 ~~- Actually pulling the training data back in from Table store - I ran out of time and exported the data and put it into a `static double[][]`~~
 - Integrating the Azure components with the Balena API, so that you can provision additional RPi's (by burning more SD cards) and they are added into the IOT Hub and KNN classifier automatically.

Then there are lots of ideas for additional functionality, such as adding more tags and displaying a grid of where they all are? How about streaming all of the telemetry through the KNN classifier and storing the locations in a heatmap dataset, so that you can track where a tag (or moreover, the thing it's attached to) spends most of it's time:
![Heatmap HLD](https://i.ibb.co/vJYM0mV/Heatmap.jpg)
Or what about using that heatmap data to drive a K-Means model to detect abnormal movements, and then driving an alerting service with that? Does the robot hoover usually not go out of the front door? ALERT!!!!
![Alerting](https://i.ibb.co/2Md77wL/Alerting.jpg)
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTA2Njg2OTAxOSwxOTEwNDcxNzcsMzM1OT
g3Mzc2LC0xNjE3MTEwOTAxLDEyNjkzODI1MjEsLTEyNDk5NTI1
MiwtNzE1NDU1NjQ3LC0xNDk1MDk4ODA0LDYxMDY3MDA0NCwxMj
g4NzkwODEyLC0xMzg5MDM1NTExLDg0Mjk3MjUwMSwxOTM3ODQ4
MjE4LC0yMDkxMTk3MjE3LDEyMjQyMDI2NDUsLTE2MjYwNDg4Mz
EsNzQxMzkxMzE3LC0zODMwODE4ODAsLTE3MjI3MzU0NDUsMTk3
NzU2MDU3MF19
-->
