/*
  WiFiConnect.h - Library for connect ESP8266 to WiFi.
  Created by Flávio Omar Losada, March 30, 2019.
  Released into the public domain.
*/
#ifndef WiFiConnect_h
#define WiFiConnect_h

#include <Arduino.h>
#include <ESP8266WebServer.h>

class WiFiConnect {
  public:    
    WiFiConnect();
    void handleWebServer();
    void start(const char* APssid, const char* APpwd);  
    String getConnectionType();    
  private:        
    String readFile(String file);
    void writeFile(String content, String file);
    String getConfigFromPage(String parameter);
    void connectAccessPoint(const char* APssid, const char* APpwd);
    boolean connectWiFi(String wifiConfig);
    void webServer();
};

#endif