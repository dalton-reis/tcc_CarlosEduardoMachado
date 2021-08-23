/*
  IUTConnect.h - Library for communicate ESP8266 by HTTP protocol.
  Created by Flávio Omar Losada, April 26, 2019.
  Released into the public domain.
*/
#ifndef IUTConnect_h
#define IUTConnect_h

#include <Arduino.h>
#include <WiFiConnect.h>
#include <ESPAsyncUDP.h>

class IUTConnect {
  public:
    IUTConnect();
    void setAccessPointSSID(char* SSID);
    char* getAccessPointSSID();
    void setAccessPointPassword(char* password);
    char* getAccessPointPassword();
    void setTokenID(char* tokenID);
    char* getTokenID();
    void setMulticastIP(char* multicastIP);
    char* getMulticastIP();
    void setMulticastPort(unsigned int multicastPort);    
    unsigned int getMulticastPort();
    IPAddress getDeviceIP();
    void setSocketCallback(void (*callback)(WiFiClient client, String content));
    void start();
    void start(char* tokenID);
    void start(char* accessPointSSID, char* accessPointPassword, char* tokenID);    
    void listenSocket();
    bool isWiFi();    
  private:
    void (*socketCallback)(WiFiClient, String);
    char* accessPointSSID;
    char* accessPointPassword;
    char* tokenID;
    WiFiConnect wifiConn;
    AsyncUDP udpConn;
    char* multicastIP;
    IPAddress deviceIP;
    unsigned int multicastPort;
    void initializeWiFiConn();
    void initializeMulticast();    
    void handlePacket(AsyncUDPPacket packet);
};

#endif