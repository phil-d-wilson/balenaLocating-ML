# balenaLocating-ML
PoC to show indoor BLE triangulation using RaspberryPi sensors and a KNN classifier model

![Floorplan](https://i.ibb.co/pRJCqDm/Floorplan.jpg)

Given the above floorplan and placement of devices, training data is captured for each room. The training data is then used to sample sensor values to predict, via the KNN, where a BLE beacon is located.

### Simple KNN classifier

 1. Finds the distance from the unknown item to the training tuples
 2. Orders the result from closest to furthest away
 3. Finds the K nearest items
 4. Identifies the most frequent class in the result set

### PoC works

[![IMAGE ALT TEXT HERE](https://i.ibb.co/xFwg8Cr/YouTube.jpg)](https://youtu.be/1Ua-MyN-3f8)
[Click to watch the video]

### Future Steps
* Remove the need for a cloud connection/account. All on the edge!

Lots of ideas for additional functionality, such as adding more tags and displaying a grid of where they all are? Streaming all of the telemetry through the KNN classifier and storing the locations in a heatmap dataset, so that you can track where a tag (or moreover, the thing it's attached to) spends most of it's time:
![Heatmap HLD](https://i.ibb.co/vJYM0mV/Heatmap.jpg)
Using that heatmap data to drive a K-Means model to detect abnormal movements, and then driving an alerting service with that? Does the robot hoover usually not go out of the front door? ALERT!!!!
![Alerting](https://i.ibb.co/2Md77wL/Alerting.jpg)

