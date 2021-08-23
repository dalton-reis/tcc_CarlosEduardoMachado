#include <Arduino.h>
#include <IUTConnect.h>
#include <MCP3008.h>

#define CS_PIN D1
#define CLOCK_PIN D4
#define MOSI_PIN D2
#define MISO_PIN D3
#define LIGADO_PIN D5
#define WIFI_PIN D6
#define CONECTADO_PIN D7

IUTConnect IUTConn;
int temperatura = 25;
int potenciometroValue = 0;
int temperaturaAnterior = 0;
int ldrValue = 0;
int luz = 0;
int luzAnterior = 0;
int botaoValue = 0;
int comida = 0;
int comidaAnterior = 0;
bool conectado = false;
bool enviar = false;

MCP3008 adc(CLOCK_PIN, MOSI_PIN, MISO_PIN, CS_PIN);

void server(WiFiClient client, String content) {
  Serial.println(content);
  if (conectado)   {
    potenciometroValue = adc.readADC(7);   
    ldrValue = adc.readADC(6);  
    botaoValue = adc.readADC(5);
    digitalWrite(CONECTADO_PIN, HIGH);
    temperatura = potenciometroValue * 100 / 1023;  
    if (temperatura != temperaturaAnterior) {
      client.printf("TEMP|%d\r\n", temperatura);            
      client.println();
      enviar = true;      
      temperaturaAnterior = temperatura;       
    }
    luz = ldrValue * 100 / 1023;
    if (luz != luzAnterior) {      
      client.printf("LIGHT|%d\r\n", luz);
      client.println();
      enviar = true;
      luzAnterior = luz;          
    }
    comida = botaoValue;
    if (comida > 800 && comida != comidaAnterior) {
      client.printf("FOOD|1\r\n");
      client.println();
      enviar = true;       
      comidaAnterior = comida;     
    } else if (comida == 0) {
      comidaAnterior = comida;
    }
    if (enviar) {      
      client.flush();
      delay(500);
      enviar = false;
    } else {
      client.println("HEART_BEAT");
      client.println();
      client.flush();
      delay(150);
    }    
  } else if (content.indexOf("CONNECTED") >= 0) {
    Serial.println("Receive \"CONNECTED\"");
    conectado = true;
    client.println("CONNECTED_TOO\r\n");
    client.println();
    delay(500);
    client.flush();
    Serial.println("Send \"CONNECTED_TOO\"");
    delay(500);
  }
}

void resetModulo() {
  digitalWrite(LIGADO_PIN, LOW);
  digitalWrite(WIFI_PIN, LOW);
  digitalWrite(CONECTADO_PIN, LOW);
  delay(1500);
}

void setup() {
  pinMode(LIGADO_PIN, OUTPUT);
  pinMode(CONECTADO_PIN, OUTPUT);
  pinMode(WIFI_PIN, OUTPUT);
  resetModulo();
  char* ssid = "Aquario";
  char* password = "aquario-virtual";
  char* tokenID = "AQUARIUM_01";
  Serial.begin(9600);  
  IUTConn.setSocketCallback(server);
  IUTConn.start(ssid, password, tokenID);  
}

void loop() {
  digitalWrite(LIGADO_PIN, HIGH);
  if (IUTConn.isWiFi()) {
    digitalWrite(WIFI_PIN, HIGH);
  } else {
    digitalWrite(WIFI_PIN, LOW);
  }
  digitalWrite(CONECTADO_PIN, LOW);
  conectado = false;
  IUTConn.listenSocket();  
}