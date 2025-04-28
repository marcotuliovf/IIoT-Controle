using Microsoft.Maui.Controls;
using sec4_controle.Services;
using Microsoft.Maui.Dispatching;

namespace sec4_controle
{
    public partial class ProportionalControlPage : ContentPage
    {
        private readonly MqttService _mqttService;
        private bool _isControlActive = false;

        public ProportionalControlPage()
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

                // Agora assinamos os dois t�picos necess�rios
                await _mqttService.SubscribeAsync("pendulo/parametroskp");
                await _mqttService.SubscribeAsync("pendulo/angulokp");

                _mqttService.MessageReceived += OnMqttMessageReceived;

                statusLabel.Text = "Conectado ao broker MQTT!";
                connectButton.Text = "Conex�o estabelecida";
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Falha na conex�o: {ex.Message}";
                connectButton.Text = "Tentar novamente";
                connectButton.IsEnabled = true;
            }
        }

        // Agora OnMqttMessageReceived recebe t�pico e payload separados
        private void OnMqttMessageReceived(string topic, string payload)
        {
            Console.WriteLine($"Mensagem recebida: t�pico={topic}, payload={payload}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (topic == "pendulo/angulokp")
                {
                    if (float.TryParse(payload, out float angle))
                    {
                        pendulumClock.Angle = angle;
                        angleLabel.Text = $"�ngulo: {angle:F2}�";
                    }
                }
                else if (topic == "pendulo/parametroskp")
                {
                    receivedDataLabel.Text = $"Par�metros Recebidos:\n{payload}";
                }
            });
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            if (!_mqttService.IsConnected)
            {
                statusLabel.Text = "Erro: N�o conectado ao broker MQTT";
                return;
            }

            try
            {
                double thetaRef = double.Parse(thetaRefEntry.Text);
                double kp = double.Parse(kpEntry.Text);

                string message = $"{thetaRef},{kp}";
                await _mqttService.PublishAsync("pendulo/parametroskp", message);

                statusLabel.Text = "Par�metros enviados com sucesso!";
            }
            catch (FormatException)
            {
                statusLabel.Text = "Erro: Valores inv�lidos";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Erro ao enviar: {ex.Message}";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _mqttService.MessageReceived -= OnMqttMessageReceived;
        }
    }
}
