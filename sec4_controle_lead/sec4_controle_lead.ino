#include <EWiFi.h>
#include <PubSubClient.h>
#include <WiFiPassword.h>

// Pinos do encoder
#define enc_a 22
#define enc_b 23

// Pinos do L298N
#define PWM_PIN 18
#define IN1     19
#define IN2     21

WiFiClient espClient;
PubSubClient MQTT(espClient);

const char* topicSubscribe = "pendulo/parametroslead";
const char* topicPublish = "pendulo/angulolead";

// Variáveis de controle
double theta = 0.0, TempoIni = 0.0, erro = 0.0, erro_deriv = 0.0, u = 0.0, T = 0.0;
double a = 0.00, b = 0.00, k = 0.00, td = 0.00;
volatile long contEnc = 0;
double erro_anterior = 0, T_anterior = 0.0, f = 0.0;
double delta_tempo = 0.001;
double thetaRef = 0.0, thetaRefDeg = 0.00;
int count = 0;

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
    // Parse da mensagem recebida (esperando formato: "thetaRef,a,b,k,td")
    sscanf(mensagemRecebida.c_str(), "%lf,%lf,%lf,%lf,%lf", &thetaRefDeg, &a, &b, &k, &td);
    Serial.println("Parâmetros atualizados.");
  }
}

void setup() {
  Serial.begin(115200);

  pinMode(PWM_PIN, OUTPUT);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(enc_a, INPUT_PULLUP);
  pinMode(enc_b, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(enc_a), handleEncoder, CHANGE);

  // Conectar Wi-Fi com múltiplas redes
  do {
    if (count) Serial.println("\n\nConnection error, trying again.\n");

    count %= 3;

    switch (count) {
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
  } while (ewifi.status() != WL_CONNECTED);

  count = 0;
  MQTT.setServer(MQTTServer, MQTTPort);
  MQTT.setCallback(callback);
}

void loop() {
  if (!MQTT.connected()) conectarMQTT();
  MQTT.loop();

  TempoIni = micros();
  
  noInterrupts();
  long contEncCopy = contEnc;
  interrupts();

  theta = contEncCopy * 2.0 * 3.1415 / (334.0 * 4.0);

  if (TempoIni / 1000000.0 > 8.0) {
    thetaRef = thetaRefDeg * (3.1415 / 180);
  }

  erro = thetaRef - theta;
  erro_deriv = (erro - erro_anterior) / delta_tempo;

  f = -b * T_anterior + k * (erro_deriv + a * erro);
  T = T_anterior + (f * delta_tempo);

  T_anterior = T;
  erro_anterior = erro;

  // Direção
  if (T >= 0) {
    digitalWrite(IN1, HIGH);
    digitalWrite(IN2, LOW);
  } else {
    digitalWrite(IN1, LOW);
    digitalWrite(IN2, HIGH);
    T = -T;
  }

  // Saturação
  u = 19442.0 * T + 8.0;
  u = min(u, 100.0);
  u = max(u, 0.0);


  analogWrite(PWM_PIN, 255.0 * u / 100.0);
  Serial.println(u);

  while ((micros() - TempoIni) < 1000.0) { }  // Delay para manter taxa de atualização
  delay(td);

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
