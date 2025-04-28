using Microsoft.Extensions.Logging;
using sec4_controle.Services;
using sec4_controle.Views;

namespace sec4_controle
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("materialdesignicons.ttf", "MaterialDesignIcons");
                });

            // Registre o serviço MQTT
            builder.Services.AddSingleton<MqttService>();
            builder.Services.AddSingleton<MainTabbedPage>();
            builder.Services.AddTransient<LeadLagControlPage>();
            builder.Services.AddTransient<PendulumClockView>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}