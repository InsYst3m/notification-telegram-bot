using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Clients.Interfaces;
using NotificationTelegramBot.API.Constants;
using NotificationTelegramBot.API.Entities;
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
        private readonly ITelegramBotClient _telegramClient;
        private readonly ICoinApiClient _coinApiClient;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly CancellationTokenSource _tokenSource;

        public TelegramBotService(
            IOptions<NotificationTelegramBotOptions> options,
            ITelegramBotClient client,
            ICoinApiClient coinApiClient,
            ILogger<TelegramBotService> logger)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _telegramClient = client ?? throw new ArgumentNullException(nameof(client));
            _coinApiClient = coinApiClient ?? throw new ArgumentNullException(nameof(coinApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = options.Value;
            _tokenSource = new CancellationTokenSource();
        }

        #region IHostedService Implementation

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ReceiverOptions receiverOptions = new();

            _telegramClient.ReceiveAsync(
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
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                result = message.Text switch
                {
                    string text when text.Equals(Commands.ASSETS_COMMAND, StringComparison.OrdinalIgnoreCase) =>
                        await ProcessAssetsCommandAsync(cancellationToken),
                    string text when text.StartsWith(Commands.GET_COMMAND) =>
                        await ProcessGetCommandAsync(text, cancellationToken),
                    _ => $"Unable to parse command: '{message.Text}'."
                };
            }

            await _telegramClient.SendTextMessageAsync(_options.ChatId, result, cancellationToken: cancellationToken);
        }

        #endregion

        #region Private Members

        private async Task<string> ProcessGetCommandAsync(string getCommand, CancellationToken cancellationToken)
        {
            Match match = Regexes.GetCommandRegex().Match(getCommand);

            if (match.Success)
            {
                string asset = match.Groups[2].Value;

                try
                {
                    Asset foundAsset = await _coinApiClient.GetCryptoAssetAsync(asset, cancellationToken);

                    return $"{foundAsset.Name}: {foundAsset.PriceUsd:0.000 USD}";
                }
                catch
                {
                    return $"Unable to found asset: '{asset}'.";
                }
            }

            return $"Unable to parse command: '{getCommand}'.";
        }

        private async Task<string> ProcessAssetsCommandAsync(CancellationToken cancellationToken)
        {
            List<string> availableAssets = await _coinApiClient.GetAvailableAssetsAsync(cancellationToken);

            if (availableAssets.Any())
            {
                return string.Join(", ", availableAssets);
            }

            return "Unable to get available assets.";
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
