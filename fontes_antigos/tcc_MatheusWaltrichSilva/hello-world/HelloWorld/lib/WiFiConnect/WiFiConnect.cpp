#include <Arduino.h>
#include "WiFiConnect.h"
#include <FS.h>
#include <ESP8266WiFi.h>
#include <user_interface.h>

WiFiServer server(80);
String HTML_PAGE = "";
String connectionType;

WiFiConnect::WiFiConnect() {

}
  
void WiFiConnect::loadHTML() {
  HTML_PAGE = "<html>";
  HTML_PAGE +=  "<head>";
  HTML_PAGE +=    "<title>Aquário Virtual - Configurações</title>";  
  HTML_PAGE +=  "</head>";
  HTML_PAGE +=  "<body>";
  HTML_PAGE +=    "<label>Informe o SSID</label>";
  HTML_PAGE +=    "<input type=\"text\" id=\"input-ssid\"/>";
  HTML_PAGE +=    "<br>";
  HTML_PAGE +=    "<label>Informe a senha</label>";
  HTML_PAGE +=    "<input type=\"password\" id=\"input-password\"/>";
  HTML_PAGE +=    "<br>";
  HTML_PAGE +=    "<a href='' onclick=\"this.href = '\?ssid=' + document.getElementById('input-ssid').value + '&password=' + document.getElementById('input-password').value\">Gravar</a>";
  HTML_PAGE +=  "</body>";
  HTML_PAGE +="</html>";
}
      
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
  server.begin();
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
  int tries = 0;
  while (tries < 10 && WiFi.status() != WL_CONNECTED) {
    delay(3000);
    WiFi.begin(wifiStr, pwdStr);
    tries++;
  }
  return WiFi.status() == WL_CONNECTED;
}    

String WiFiConnect::getConnectionType() {
  return connectionType;
}

void WiFiConnect::createWebServer(void (*callback)(String request)) {  
  WiFiClient client = server.available();
  if (!client) {
    return;
  }

  while(!client.available()){
    delay(1);
  }

  String req = client.readStringUntil('\r');
  Serial.println(req);
  client.flush();

  client.print(HTML_PAGE);
  client.flush();

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
  } else {
    callback(req);
  }
}

void WiFiConnect::start(const char* APssid, const char* APpwd) {  
  SPIFFS.begin();
  loadHTML();
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
      server.begin();
      connectionType = "STA";
    }
  }
}
