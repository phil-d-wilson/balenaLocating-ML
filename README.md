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
It's a plain RPi 3B+, with no extra hardware other than a Micro-SD card. 
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE3MjI3MzU0NDUsMTk3NzU2MDU3MCwxOT
Q5OTA4MDIyLDEzMTc0NzA4MTMsNDg2MjM5MDc1LC0xNTM2NTMw
NTg0XX0=
-->