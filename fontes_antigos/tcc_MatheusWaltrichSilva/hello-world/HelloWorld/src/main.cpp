#include <Arduino.h>
#include <WiFiConnect.h>
#include <UDPConn.h>

//VIVO-52C0|Td9VYk7m3W 
WiFiConnect wifiConn;
UDPConn udpConn;
unsigned int multicastPort = 5000;
const char* ssid = "Aquario";
const char* password = "aquario-virtual";

void setup() {
  Serial.begin(9600);
  wifiConn.start(ssid, password);
  if (wifiConn.getConnectionType() == "STA") {
    udpConn.startMulticast(227, 55, 77, 99, multicastPort);
  }
}

void test(String req) {
  Serial.println("Callback message: " + req);
}

void loop() {
  wifiConn.createWebServer(test);
  if (wifiConn.getConnectionType() == "STA") {
    udpConn.listeningMulticast();
    if (udpConn.getMulticastPacket() != "") {
      Serial.println(udpConn.getMulticastPacket());
    }
  }
}