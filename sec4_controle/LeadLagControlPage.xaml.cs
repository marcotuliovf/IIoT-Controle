using Microsoft.Maui.Controls;
using MQTTnet;
using sec4_controle.Services;
using Microsoft.Maui.Dispatching;

namespace sec4_controle
{
    public partial class LeadLagControlPage : ContentPage
    {
        private readonly MqttService _mqttService;
        private bool _isControlActive = false;

        public LeadLagControlPage()
        {
            InitializeComponent();
            _mqttService = new MqttService();
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            connectButton.IsEnabled = false;
            statusLabel.Text = "Conectando ao broker MQTT...";

            try
            {
                await _mqttService.ConnectAsync();
                await _mqttService.SubscribeAsync("pendulo/parametroslead");
                await _mqttService.SubscribeAsync("pendulo/angulolead");
                _mqttService.MessageReceived += OnMqttMessageReceived;

                statusLabel.Text = "Conectado ao broker MQTT!";
                connectButton.Text = "Conexão estabelecida";
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Falha na conexão: {ex.Message}";
                connectButton.Text = "Tentar novamente";
                connectButton.IsEnabled = true;
            }
        }

        private void OnMqttMessageReceived(string topic, string payload)
        {
            Console.WriteLine($"Mensagem recebida no LeadLagControlPage: tópico={topic}, payload={payload}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Aqui você trata os dados conforme o tópico recebido
                if (topic == "pendulo/angulolead")
                {
                    if (float.TryParse(payload, out float angle))
                    {
                        pendulumClock.Angle = angle;
                        angleLabel.Text = $"Ângulo: {angle:F2}°";
                    }
                }
                else
                {
                    receivedDataLabel.Text = $"Parâmetros Recebidos:\n{payload}";
                }
            });
        }


        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            if (!_mqttService.IsConnected)
            {
                statusLabel.Text = "Erro: Não conectado ao broker MQTT";
                return;
            }

            try
            {
                double thetaRef = double.Parse(thetaRefEntry.Text);
                double k = double.Parse(kEntry.Text);
                double a = double.Parse(aEntry.Text);
                double b = double.Parse(bEntry.Text);
                double td = double.Parse(tdEntry.Text);

                string message = $"{thetaRef},{k},{a},{b},{td}";
                await _mqttService.PublishAsync("pendulo/parametroslead", message);

                statusLabel.Text = "Parâmetros enviados com sucesso.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Erro ao enviar parâmetros: {ex.Message}";
            }
        }

    }
}