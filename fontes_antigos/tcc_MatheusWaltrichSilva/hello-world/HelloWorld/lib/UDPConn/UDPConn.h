/*
  UDPConn.h - Library to send/receive UDP messages from ESP8266.
  Created by Flávio Omar Losada, March 30, 2019.
  Released into the public domain.
*/

#ifndef UDPConn_h
#define UDPConn_h

#include <Arduino.h>

class UDPConn {
  public:
    UDPConn();
    void startMulticast(int ipPart01, int ipPart02, int ipPart03, int ipPart04, unsigned int multicastPort);
    void listeningMulticast(bool log = false);
    char* getMulticastPacket();
    String getRemoteIP();
};

#endif