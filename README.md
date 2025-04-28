# Projeto: Controle de Pêndulo Invertido via MQTT

## Descrição
Este projeto tem como objetivo controlar um pêndulo invertido utilizando um ESP32 DevKit, realizando a comunicação MQTT para receber parâmetros do controlador (Lead-Lag) de um aplicativo desenvolvido em MAUI.

O sistema permite:
- **Recebimento de parâmetros** (a, b, k) via MQTT.
- **Controle do motor** para estabilização do pêndulo.
- **Envio de dados de feedback** para o aplicativo, como o ângulo atual, o torque e o tempo.

## Arquivos
- **Controle_Pendulo_Invertido_MQTT.ino**
  - Código principal para o ESP32.
  - Integra conexão Wi-Fi, controle Lead-Lag e comunicação MQTT.

- **EWiFi.h** / **EWiFi.cpp**
  - Biblioteca personalizada para gerenciamento da conexão Wi-Fi.

- **WifiPassword.h**
  - Arquivo de configuração contendo SSID e senhas das redes Wi-Fi utilizadas no laboratório.

## Estrutura MQTT
- **Broker:** 200.19.144.16 (porta 1883)

### Tópicos
- **Subscrição:**
  - `pendulo/parametros` → Recebe os parâmetros do controlador: `a,b,k` (separados por vírgula)

- **Publicação:**
  - `pendulo/feedback` → Publica o ângulo (°), torque (mN) e tempo (s), separados por vírgula.

## Conexão Wi-Fi
A conexão é feita utilizando autenticação WPA2-Enterprise para as redes:
- UFU-Institucional
- eduroam

Em caso de falha, tenta a terceira opção SSID definida sem autenticação Enterprise.

## Observações
- O ESP32 realiza controle a cada 1ms e publica os dados de feedback continuamente.
- Certifique-se que o aplicativo MAUI esteja configurado para se conectar ao mesmo broker MQTT.
- As bibliotecas necessárias (`PubSubClient`, `WiFi`) devem estar instaladas no Arduino IDE.

---

**Desenvolvido para os laboratórios de Sistemas de Controle Linear.**
