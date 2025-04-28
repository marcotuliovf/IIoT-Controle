
# Sistema de Controle Remoto de Pêndulo Invertido via IIoT

## 📌 Visão Geral
Este projeto foi desenvolvido para a disciplina de Internet das Coisas Industriais (IIoT) da Universidade Federal de Uberlândia (UFU), com o objetivo de criar um sistema de controle remoto para um pêndulo invertido, permitindo a realização de aulas práticas de Controle Linear de forma virtual.

---

## 🛠️ Componentes e Tecnologias

**Hardware:**
- ESP32: Microcontrolador principal
- Motor DC + Ponte H L298N: Acionamento do pêndulo
- Encoder Óptico: Leitura do ângulo do pêndulo
- Protoboard e Componentes Eletrônicos: Montagem do circuito

**Software:**
- Aplicativo MAUI: Interface de controle (.NET 8)
- Broker MQTT: Mosquitto em VM Google Cloud (`34.151.193.145:1883`)
- Firmware ESP32: Programado via Arduino IDE
  - Bibliotecas: `EWiFi.h`, `PubSubClient.h`, `Encoder.h`

---

## 📋 Funcionalidades do Sistema

✅ **Controle em Tempo Real**  
- Envio de parâmetros para dois tipos de controladores:
  - Proporcional (Kp)
  - Lead-Lag

✅ **Monitoramento Visual**  
- Relógio analógico com ponteiro mostrando o ângulo atual do pêndulo
- Exibição numérica do ângulo em graus

✅ **Comunicação Estável**  
- Conexão via protocolo MQTT com QoS garantido
- Reconexão automática em falhas de rede

---

## 🚀 Como Utilizar

**Configuração Inicial:**

1. **ESP32:**
   - Carregar o firmware correspondente:
     - `Controlador_Kp.ino` (Apêndice 1)
     - `Controlador_Lead.ino` (Apêndice 2)
   - Configurar credenciais Wi-Fi em `WiFiPassword.h`

2. **Aplicativo MAUI:**
   Código para configurar o broker:
   ```csharp
   private readonly string _server = "34.151.193.145";
   private readonly int _port = 1883;
   ```

**Interface do Usuário:**

- **Aba Proporcional:**
  - Ângulo de Referência (θ)
  - Ganho Proporcional (Kp)

- **Aba Lead-Lag:**
  - Parâmetros: θ, k, a, b, td
  - Botões: Conectar ao Broker → Enviar Parâmetros

---

## 📊 Estrutura do Código

```
IIoT-Controle/
|
├── Firmware_ESP32/
│   ├── Controlador_Kp/
│   │   ├── Controlador_Kp.ino
│   │   └── WiFiPassword.h
│   └── Controlador_Lead/
│       ├── Controlador_Lead.ino
│       └── WiFiPassword.h
|
├── App_MAUI/
│   ├── Services/
│   │   └── MqttService.cs
│   ├── Views/
│   │   ├── PendulumClockView.cs
│   │   ├── ProportionalControlPage.xaml
│   │   └── LeadLagControlPage.xaml
│   └── MauiProgram.cs
|
└── Documentação/
    └── Relatório_IIoT1.pdf
```

---

## 🔧 Solução de Problemas Comuns

- **Falha na Conexão MQTT:**
  - Verifique se o broker está acessível
  - Confira as credenciais Wi-Fi no ESP32

- **Leitura Inconsistente do Ângulo:**
  - Recalibre o encoder
  - Verifique a alimentação do circuito

---

## 📚 Referências
1. [Biblioteca MQTTnet](https://www.nuget.org/packages/MQTTnet/)
2. Documentação da disciplina (Teams - FEMEC42082)
3. Roteiros de laboratório de Controle Linear (UFU)

---

## 👥 Autores
- João Victor Martins Xavier (12021EMT006)
- Marco Tulio Vilela Fonseca (12021EMT016)
- Pedro Cabrini Costa Barros (12021EMT009)
- Thalles Jonathan Trigueiro (11711EMT020)

**Orientação:** Prof. Dr. José Jean-Paul Zanlucchi

---

🔗 **Repositório:** [github.com/marcotuliovf/IIoT-Controle](https://github.com/marcotuliovf/IIoT-Controle)  
📅 **Última Atualização:** Abril/2025
