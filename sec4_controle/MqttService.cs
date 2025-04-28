using MQTTnet;
using System.Text;

namespace sec4_controle.Services
{
    public class MqttService
    {
        private IMqttClient _mqttClient;
        private readonly string _server = "34.151.193.145";
        private readonly int _port = 1883;

        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        // Agora o evento passa o tópico e a mensagem separadamente
        public event Action<string, string>? MessageReceived;

        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_server, _port)
                .Build();

            _mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Conectado ao broker MQTT.");
                await Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("Desconectado do broker MQTT.");
                await Task.CompletedTask;
            };

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                MessageReceived?.Invoke(topic, payload); // Agora enviamos tópico + mensagem
                await Task.CompletedTask;
            };

            await _mqttClient.ConnectAsync(options);
        }

        public async Task PublishAsync(string topic, string message)
        {
            if (!IsConnected) await ConnectAsync();

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            await _mqttClient.PublishAsync(mqttMessage);
        }

        public async Task SubscribeAsync(string topic)
        {
            if (!IsConnected) await ConnectAsync();

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
        }

        public async Task DisconnectAsync()
        {
            if (IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }
    }
}
