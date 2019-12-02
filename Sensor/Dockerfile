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
RUN npm install --save macaddress
RUN npm install date-and-time

RUN JOBS=MAX npm install --production --unsafe-perm && npm cache verify && rm -rf /tmp/*

COPY . .

CMD ["npm", "start"]