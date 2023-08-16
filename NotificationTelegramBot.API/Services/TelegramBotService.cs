using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NotificationTelegramBot.API.Services
{
    public sealed class TelegramBotService : ITelegramBotService, IDisposable
    {
        private readonly NotificationTelegramBotOptions _options;
        private readonly ITelegramBotClient _client;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly CancellationTokenSource _tokenSource;

        public TelegramBotService(
            IOptions<NotificationTelegramBotOptions> options,
            ITelegramBotClient client,
            ILogger<TelegramBotService> logger)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = options.Value;
            _tokenSource = new CancellationTokenSource();
        }

        #region IHostedService Implementation

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReceiverOptions receiverOptions = new();

            _client.ReceiveAsync(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                _tokenSource.Token);

            _logger.LogInformation("Telegram bot was successfully initialized.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();

            return Task.CompletedTask;
        }

        #endregion

        #region ITelegramBotService Implementation

        public Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            string errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API error:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);

            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceivedAsync(update.Message!, cancellationToken),
                _ => Task.CompletedTask
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(client, ex, cancellationToken);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Provides handler to process incoming messages from the telegram bot.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        private async Task OnMessageReceivedAsync(Message message, CancellationToken cancellationToken)
        {
            await _client.SendTextMessageAsync(_options.ChatId, $"ECHO: {message.Text}", cancellationToken: cancellationToken);
        }

        #endregion

        #region IDiagnosticService Implementation

        private readonly DateTime _serviceStartedTime = DateTime.UtcNow;
        private TimeSpan _serviceUptime => DateTime.UtcNow - _serviceStartedTime;

        public Dictionary<string, string> GetDiagnosticsInfo()
        {
            return new Dictionary<string, string>
            {
                { "Service Started Time MSK", TimeZoneInfo.ConvertTimeFromUtc(_serviceStartedTime, TimeZoneInfo.FindSystemTimeZoneById("MSK")).ToString() },
                { "Service Uptime", _serviceUptime.ToString() }
            };
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
