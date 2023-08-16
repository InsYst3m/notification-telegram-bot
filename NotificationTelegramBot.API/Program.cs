using Microsoft.Extensions.Options;

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

            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(
                serviceProvider =>
                {
                    NotificationTelegramBotOptions options =
                        serviceProvider.GetRequiredService<IOptions<NotificationTelegramBotOptions>>().Value;

                    return new TelegramBotClient(options.Token);
                });
            services.AddSingleton<ITelegramBotService, TelegramBotService>();

            services.AddHostedService(x => x.GetRequiredService<ITelegramBotService>());

            #endregion

            #region Middlewares

            var app = builder.Build();

            #endregion

            app.Run();
        }
    }
}