const BeaconScanner = require('node-beacon-scanner');
const scanner = new BeaconScanner();
const date = require('date-and-time');

var Protocol = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').Client;
var Message = require('azure-iot-device').Message;

var macaddress = require('macaddress');
var mac = process.env.BALENA_DEVICE_UUID;
console.log("MAC = " + mac);

var deviceConnectionString = "DEVICECONNECTIONSTRING";
var client = Client.fromConnectionString(deviceConnectionString, Protocol);

// Set an Event handler for becons
scanner.onadvertisement = (ad) => {
    output = "{\"BeaconDateTime\":\"" + date.format(new Date(), 'YYYY/MM/DD HH:mm:ss')  + "\",\"DeviceName\":\"" + mac +  "\",\"BeaconName\":\"" + ad.iBeacon.major + "-" + ad.iBeacon.minor + "\",\"Rssi\":" + ad.rssi + "}"
//  var output = (JSON.stringify(ad, null, '  '));
  console.log("Beacon: " + output);
  var message = new Message(output);
  client.sendEvent(message);
};

// Start scanning
scanner.startScan().then(() => {
  console.log('Started to scan.')  ;
}).catch((error) => {
  console.error(error);
});