#include <Arduino.h>
#include "IUTConnect.h"
#include <WiFiConnect.h>
#include <ESPAsyncUDP.h>
#include <stdlib.h>
#include <ESP8266WiFi.h>

WiFiServer wifiServer(8080);
String socketData = "";
WiFiClient client;

IUTConnect::IUTConnect() {
  this->accessPointSSID = "IUTConnect";
  this->multicastIP = "227.55.77.99";
  this->multicastPort = 5000;
}

void IUTConnect::setAccessPointSSID(char* SSID) {
  this->accessPointSSID = SSID;
}

char* IUTConnect::getAccessPointSSID() {
  return this->accessPointSSID;
}

void IUTConnect::setAccessPointPassword(char* password) {
  this->accessPointPassword = password;
}

char* IUTConnect::getAccessPointPassword() {
  return this->accessPointPassword;
}

void IUTConnect::setTokenID(char* tokenID) {
  this->tokenID = tokenID;
}

char* IUTConnect::getTokenID() {
  return this->tokenID;
}

void IUTConnect::setMulticastIP(char* multicastIP) {
  this->multicastIP = multicastIP;
}

char* IUTConnect::getMulticastIP() {
  return this->multicastIP;
}

void IUTConnect::setMulticastPort(unsigned int multicastPort) {
  this->multicastPort = multicastPort;
}

unsigned int IUTConnect::getMulticastPort() {
  return this->multicastPort;
}

IPAddress IUTConnect::getDeviceIP() {
  return this->deviceIP;
}

bool IUTConnect::isWiFi() {
  return wifiConn.getConnectionType().equals("STA");
}

void IUTConnect::setSocketCallback(void (*callback)(WiFiClient client, String content)) {
  this->socketCallback = callback;
}

void IUTConnect::listenSocket() {
  this->wifiConn.handleWebServer();
  WiFiClient client = wifiServer.available();  
  if (client) {
    while (client.connected()) {      
      socketData = "";
      this->wifiConn.handleWebServer();            
      while (client.available() > 0) {        
        char c = client.read();
        socketData += c;        
      }
      delay(10);
      this->socketCallback(client, socketData);
    }
    client.stop();
  }
}

void IUTConnect::initializeWiFiConn() {
  this->wifiConn.start(this->accessPointSSID, this->accessPointPassword);
  wifiServer.begin();
}

void IUTConnect::handlePacket(AsyncUDPPacket packet) {
  if (packet.remoteIP() != WiFi.localIP() && packet.isMulticast()) {
    char* data = (char *) packet.data();
    data[strlen(data)-1] = 0;
    if (data && strcmp(data, this->tokenID)) {
      deviceIP = packet.remoteIP();      
      packet.printf("HELLO_DEVICE");
    }
  }
}

void IUTConnect::initializeMulticast() {
  IPAddress multicastAddress;
  multicastAddress.fromString(this->multicastIP);
  if (this->udpConn.listenMulticast(multicastAddress, this->multicastPort)) {
    this->udpConn.onPacket([this](AsyncUDPPacket packet) {
      handlePacket(packet);
    });
  }
}

void IUTConnect::start(char* accessPointSSID, char* accessPointPassword, char* tokenID) {
  this->setAccessPointSSID(accessPointSSID);
  this->setAccessPointPassword(accessPointPassword);
  this->setTokenID(tokenID);
  this->start();
}

void IUTConnect::start(char* tokenID) {
  this->setTokenID(tokenID);
  this->start();
}

void IUTConnect::start() {
  if (!this->accessPointSSID || strlen(this->accessPointSSID) <= 0) {
    Serial.println("Access Point SSID is mandatory!");
    return;
  }
  if (!this->tokenID || strlen(this->tokenID) <= 0) {
    Serial.println("Token Identification is mandatory!");
    return;
  }
  if (!this->multicastIP || strlen(this->multicastIP) <= 0) {
    Serial.println("Multicast IP is mandatory!");
    return;
  }
  if (!this->multicastPort || this->multicastPort <= 0) {
    Serial.println("Multicast Port must be greater than zero!");
    return;
  }
  this->initializeWiFiConn();
  if (this->wifiConn.getConnectionType().equals("STA")) {
    this->initializeMulticast();
  }
}