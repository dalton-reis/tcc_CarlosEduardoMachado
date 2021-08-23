#include <Arduino.h>
#include "WiFiConnect.h"
#include <FS.h>
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <user_interface.h>

ESP8266WebServer server(80);
String connectionType;

WiFiConnect::WiFiConnect() {}
      
String WiFiConnect::readFile(String file) {
  File readFile = SPIFFS.open(file, "r");
  if (!readFile) {
    Serial.println("Erro ao abrir arquivo!");
    return "";
  }
  String content = readFile.readStringUntil('\r');
  readFile.close();
  return content;
}

void WiFiConnect::writeFile(String content, String file) {
  File writeFile = SPIFFS.open(file, "w+");
  if (!writeFile) {
    Serial.println("Erro ao gravar arquivo.");    
  } else {
    Serial.println("Gravando arquivo com conteúdo: " + content);
    writeFile.println(content);
  }
  writeFile.close();
}

String WiFiConnect::getConfigFromPage(String parameter) {
  String parameterSSID = "";
  String parameterPWD = "";
  parameterSSID = parameter.substring(0, parameter.indexOf("&"));
  parameterPWD = parameter.substring(parameter.indexOf("&")+1);
  parameterSSID = parameterSSID.substring(parameterSSID.indexOf("=")+1);
  parameterPWD = parameterPWD.substring(parameterPWD.indexOf("=")+1);
  if (parameterSSID.length() > 0 && parameterPWD.length() > 0) {
    return parameterSSID + "|" + parameterPWD;  
  }
  return "";
}

void WiFiConnect::connectAccessPoint(const char* APssid, const char* APpwd) {
  WiFi.mode(WIFI_AP);  
  WiFi.softAP(APssid, APpwd);     
  Serial.println(WiFi.softAPIP());
}

boolean WiFiConnect::connectWiFi(String wifiConfig) {    
  WiFi.mode(WIFI_STA);  
  String wifiStr = wifiConfig.substring(0, wifiConfig.indexOf("|"));
  wifiStr.trim();
  String pwdStr = wifiConfig.substring(wifiConfig.indexOf("|")+1);
  pwdStr.trim();  
  WiFi.begin(wifiStr, pwdStr);  
  Serial.println("SSID arquivo: " + wifiStr);
  Serial.println("Password arquivo: " + pwdStr);
  delay(10000);
  int tries = 0;
  while (tries < 3 && WiFi.status() != WL_CONNECTED) {
    delay(10000);
    WiFi.begin(wifiStr, pwdStr);
    tries++;
  }
  return WiFi.status() == WL_CONNECTED;
}    

String WiFiConnect::getConnectionType() {
  return connectionType;
}

void WiFiConnect::webServer() {      

  String req = server.urlDecode(server.arg("plain"));

  if (req.indexOf("ssid=") >= 0) {
    String content = req.substring(req.indexOf("ssid="));
    content = content.substring(0, content.indexOf(" "));
    Serial.println(req);
    content = getConfigFromPage(content);
    if (content.length() > 0) {
      writeFile(content, "/wifi-config.txt");
      ESP.reset();
    }
  } else if (req.indexOf("format=true") >= 0) {
    SPIFFS.format();
  }
}

void WiFiConnect::handleWebServer() {
  server.handleClient();
}

void WiFiConnect::start(const char* APssid, const char* APpwd) {  
  SPIFFS.begin();
  String wifiConfig = readFile("/wifi-config.txt");
  Serial.println("Lido arquivo");  
  if (wifiConfig.length() == 0) {
    Serial.println("Arquivo vazio ou não encontrado");
    connectAccessPoint(APssid, APpwd);
    connectionType = "AP";
  } else {
    Serial.println("Arquivo configurado");
    if (!connectWiFi(wifiConfig)) {
      Serial.println("Não foi possível conectar no WiFi");
      connectAccessPoint(APssid, APpwd);
      connectionType = "AP";
    } else {
      wifi_set_sleep_type(NONE_SLEEP_T);
      Serial.println(WiFi.localIP());      
      connectionType = "STA";
    }
  }
  server.on("/wifi_config", [this]() {
    if (server.method() == HTTP_POST && server.hasArg("plain")) {
      server.send(200, "text/plain", "Received body");
      delay(300);
      webServer();      
    } else {
      server.send(405, "text/plain", "Incorrect method or body is blank");
    }
  });
  server.begin();
}
