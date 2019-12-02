![BalenaLocating](https://i.ibb.co/svRSnf7/logo.png)
A proof of concept project used to explore the [balenaCloud](https://www.balena.io/cloud/) stack for provisioning Raspberry Pi's as Bluetooth sensors, and a (simple!) KNN classifier to predict indoor positioning of iBeacons.

### The Plan:
One of the most difficult problems when developing Internet of Things solutions is provisioning the connected devices/sensors. Typically these devices need some network connectivity credentials (e.g. WIFI SSID & passphrase) and some credentials to authenticate against the backend cloud solution. Manually configuring a single device is OK, but when multiple devices are needed for a solution, this provisioning can be a blocker to progress. In addition, rapid development of concepts relies on a tight development-feedback loop. The [balenaCloud](https://www.balena.io/cloud/) is a solution to both of these issues, and so I was keen to try it for myself!
In order to make sure I experienced the provisioning process fully, I wanted a problem to solve which required more than one device. I have also been looking for an opportunity to try and create an indoor Bluetooth Low Energy (BLE) locating system for some time having created more simple present/absent systems previously. And so the plan was hatched:
![Floorplan](https://i.ibb.co/pRJCqDm/Floorplan.jpg)
My plan involved placing three Raspberry Pi 3B/3B+ devices in different downstairs rooms of my (small) house, loading them with [iBeacon](https://developer.apple.com/ibeacon/) recieving 
![High Level Design](https://i.ibb.co/gt2LyCK/HLD.jpg)

<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEzNDM5NzQwOTAsMTk3NzU2MDU3MCwxOT
Q5OTA4MDIyLDEzMTc0NzA4MTMsNDg2MjM5MDc1LC0xNTM2NTMw
NTg0XX0=
-->