using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Clients;
using NotificationTelegramBot.API.Clients.Interfaces;
using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services;
using NotificationTelegramBot.API.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IServiceCollection services = builder.Services;

            #region Services

            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services
                .AddOptions<NotificationTelegramBotOptions>()
                .BindConfiguration(nameof(NotificationTelegramBotOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<CoinApiOptions>()
                .BindConfiguration(nameof(CoinApiOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(
                serviceProvider =>
                {
                    NotificationTelegramBotOptions options =
                        serviceProvider.GetRequiredService<IOptions<NotificationTelegramBotOptions>>().Value;

                    return new TelegramBotClient(options.Token);
                });

            services.AddHttpClient(nameof(CoinApiClient), (serviceProvider, httpClient) =>
            {
                CoinApiOptions options =
                    serviceProvider.GetRequiredService<IOptions<CoinApiOptions>>().Value;

                httpClient.BaseAddress = new Uri(options.ServiceUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutInSec);
            });

            services.AddSingleton<ICoinApiClient, CoinApiClient>();
            services.AddSingleton<ITelegramBotService, TelegramBotService>();
            services.AddSingleton<IDiagnosticService>(x => x.GetRequiredService<ITelegramBotService>());

            services.AddHostedService(x => x.GetRequiredService<ITelegramBotService>());

            #endregion

            #region Middlewares

            var app = builder.Build();

            #endregion

            app.Run();
        }
    }
}