#include <FS.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>

WiFiServer server(80);
const char* ssid = "Aquario";
const char* password = "aquario-virtual";
String HTML_PAGE = "";

void loadHTML() {
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
       
String readFile(String file) {
  File readFile = SPIFFS.open(file, "r");
  if (!readFile) {
    Serial.println("Erro ao abrir arquivo!");
    return "";
  }
  String content = readFile.readStringUntil('\r');
  readFile.close();
  return content;
}

void writeFile(String content, String file) {
  File writeFile = SPIFFS.open(file, "w+");
  if (!writeFile) {
    Serial.println("Erro ao gravar arquivo.");    
  } else {
    writeFile.println(content);
  }
  writeFile.close();
}

String getConfigFromPage(String parameter) {
  String parameterSSID = "":
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

void createWebPage() {  
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
    }
  } else if (req.indexOf("format=true") >= 0) {
    SPIFFS.format();
  }
}

void setup() {
  Serial.begin(9600);
  SPIFFS.begin();
  loadHTML();
  if (readFile("/wifi-config.txt").length() == 0) {
    WiFi.softAP(ssid, password);   
    server.begin();
    Serial.println(WiFi.softAPIP());
  }
}

void loop() {
  //String wifiFile = readFile("/wifi.txt");
  createWebPage();
  /*if (wifiFile.length() == 0) {    
    writeFile("VIVO-52C0|Td9VYk3m7W", "/wifi.txt");
    Serial.println("Arquivo gravado!");
  }*/
}


