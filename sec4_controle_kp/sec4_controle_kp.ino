#include <EWiFi.h>
#include <PubSubClient.h>
#include <WiFiPassword.h>
#include <Encoder.h>

// Pinos do encoder
#define enc_a 22
#define enc_b 23

// Pinos do L298N
#define PWM_PIN 18
#define IN1     19
#define IN2     21

WiFiClient espClient;
PubSubClient MQTT(espClient);

const char* topicSubscribe = "pendulo/parametroskp";
const char* topicPublish = "pendulo/angulokp";

//Definindo outras variaveis uteis
double theta = 0.0, erro = 0.0, u = 0.0, T = 0.0, kp = 0.5;
long contEnc = 0.0;
int count = 0;

// Escolhendo referencia para a velocidade
double thetaRef = 0.0, thetaRefDeg = 0.00; //em radianos

void IRAM_ATTR handleEncoder() {
  static int lastState = LOW;
  int state = digitalRead(enc_a);
  if (state != lastState) {
    if (digitalRead(enc_b) != state) {
      contEnc++;
    } else {
      contEnc--;
    }
  }
  lastState = state;
}

void callback(char* topic, byte* payload, unsigned int length) {
  String mensagemRecebida;
  for (int i = 0; i < length; i++) {
    mensagemRecebida += (char)payload[i];
  }

  Serial.print("Mensagem recebida no tópico ");
  Serial.print(topic);
  Serial.print(": ");
  Serial.println(mensagemRecebida);

  if (String(topic) == topicSubscribe) {
    // Parse da mensagem recebida (esperando formato: "thetaRefDeg,kp")
    sscanf(mensagemRecebida.c_str(), "%lf,%lf", &thetaRefDeg, &kp);
    Serial.println("Parâmetros atualizados.");
  }
}

void setup() {
  Serial.begin(2000000);
  pinMode(5, OUTPUT); 
  pinMode(7, OUTPUT);
  pinMode(8, OUTPUT);

  do {
    if(count) Serial.println("\n\nConnection error, trying again.\n");
    
    count %= 3;

    switch(count) {
      case 0:
        ewifi.setWiFi(SSID1, WPA2_AUTH_PEAP, anonymous, username, userpassword);
        count++;
        break;
      case 1:
        ewifi.setWiFi(SSID2, WPA2_AUTH_PEAP, anonymous, username, userpassword);
        count++;
        break;
      case 2:
        ewifi.setWiFi(SSID3, password);
        count++;
        break;
    }

    ewifi.connect();
  } while(ewifi.status() != WL_CONNECTED);

  count = 0;
  // Setup of Server
  MQTT.setServer(MQTTServer, MQTTPort);

}

void loop() {
  if (!MQTT.connected()) conectarMQTT();
  MQTT.loop();
    
  noInterrupts();
  long contEncCopy = contEnc;
  interrupts();
  
  thetaRef = thetaRefDeg*(3.1415/180);

  //Calculando theta a partir da leitura do encoder
  theta = contEncCopy * 2.0 * 3.1415 / (334.0 * 4.0);

  //Obtendo erro de rastreamento
  erro = thetaRef - theta;

  //Calculando acao do controlador
  T = kp * erro;
  
  //Alterando sentido de giro de acordo com o sinal do controle
  if (T >= 0){
      //sentido horario
      digitalWrite(7,HIGH);
      digitalWrite(8,LOW);
  }
  else{
      //sentido anti-horario
      digitalWrite(7,LOW);
      digitalWrite(8,HIGH);
      T = -T;
   }
  
  // Gerando PWM a partir do torque
  u = 19442.0*T + 8.0;
       
  //Saturando na faixa linear
  u = min(u,100.0);//limitando superiormente
  u = max(u,0.0);//limitando inferiormente
    
  //Aplicando controle a planta
  analogWrite(PWM_PIN, 255.0 * u / 100.0);

  // Publica o ângulo em graus
  char angleMsg[20];
  snprintf(angleMsg, 20, "%.2f", theta * (180.0/3.1415));
  MQTT.publish(topicPublish, angleMsg);
}

void conectarMQTT() {
  int num = 30;
  char esp_id[num];
  snprintf(esp_id, num, "ESP32-%s", ewifi.getmacAddress());
  while (!MQTT.connected()) {
    if (MQTT.connect(esp_id)) {
      MQTT.subscribe(topicSubscribe);
      Serial.println("Conectado ao broker MQTT.");
    }
  }
}