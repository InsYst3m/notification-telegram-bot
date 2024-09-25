using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Constants;
using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NotificationTelegramBot.API.Services
{
	public sealed class TelegramBotService : ITelegramBotService, INotificationService, IDisposable
	{
		private readonly NotificationTelegramBotOptions _options;
		private readonly ITelegramBotClient _telegramClient;
		private readonly IAssetService _assetService;
		private readonly ILogger<TelegramBotService> _logger;
		private readonly CancellationTokenSource _tokenSource;
		private readonly NumberFormatInfo _numberFormatInfo;

		private PeriodicTimer? _periodicTimer;
		private DateTime _nextTimerTick;
		private DateTime _previousTimerTick;

		public TelegramBotService(
			IOptions<NotificationTelegramBotOptions> options,
			ITelegramBotClient client,
			IAssetService assetService,
			ILogger<TelegramBotService> logger)
		{
			_ = options ?? throw new ArgumentNullException(nameof(options));
			_telegramClient = client ?? throw new ArgumentNullException(nameof(client));
			_assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_options = options.Value;
			_tokenSource = new CancellationTokenSource();
			_numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			_numberFormatInfo.NumberGroupSeparator = " ";
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

			Task.Run(() => SendDailyNotificationAsync(cancellationToken), cancellationToken);

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

		#region INotificationService Implementation

		public async Task SendDailyNotificationAsync(CancellationToken cancellationToken)
		{
			const short interval = 6;

			_periodicTimer = new(TimeSpan.FromHours(6));
			_nextTimerTick = _nextTimerTick.AddHours(interval);

			while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
			{
				_previousTimerTick = _nextTimerTick;
				_nextTimerTick = _nextTimerTick.AddHours(interval);

				_ = OnSendDailyNotificationAsync(cancellationToken);
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

		/// <summary>
		/// Provides handler to periodicaly send notifications with asset prices.
		/// </summary>
		/// <param name="cancellationToken"></param>
		private async Task OnSendDailyNotificationAsync(CancellationToken cancellationToken)
		{
			string[] cryptoAssets = new[]
			{
				"bitcoin",
				"ethereum",
				"polkadot",
				"near-protocol",
				"litecoin",
				"cosmos",
				"xrp",
				"solana",
				"dash",
				"cardano",
				"mina",
				"helium",
				"polygon"
			};

			string result = await ProcessNotificationCommand(cryptoAssets, cancellationToken);

			//await _telegramClient.SendTextMessageAsync(
			//	_options.ChatId, result, cancellationToken: cancellationToken);
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
					Asset foundAsset = await _assetService.GetAssetAsync(asset, cancellationToken);

					return GenerateCryptoAssetMessage(foundAsset);
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
			List<string> availableAssets = await _assetService.GetAvailableAssetsAsync(cancellationToken);

			if (availableAssets.Any())
			{
				return string.Join(", ", availableAssets);
			}

			return "Unable to get available assets.";
		}

		private async Task<string> ProcessNotificationCommand(string[] assets, CancellationToken cancellationToken)
		{
			List<Asset> foundAssets = await _assetService.GetAssetsAsync(assets, cancellationToken);

			return GenerateCryptoAssetsPriceMessage(foundAssets);
		}

		private string GenerateCryptoAssetMessage(Asset asset)
		{
			StringBuilder stringBuilder = new();
			stringBuilder.AppendLine($"Asset: {asset.Name}");
			stringBuilder.AppendLine($"Price: {asset.PriceUsd.ToString("#,0.000", _numberFormatInfo)} USD");
			stringBuilder.AppendLine($"Rank: {asset.Rank}");
			stringBuilder.AppendLine($"Capitalization: {asset.MarketCapUsd.ToString("#,0.000", _numberFormatInfo)} USD");
			stringBuilder.AppendLine($"Volume 24 hours: {asset.VolumeUsd24Hr.ToString("#,0.000", _numberFormatInfo)} USD");

			return stringBuilder.ToString();
		}

		private string GenerateCryptoAssetsPriceMessage(List<Asset> assets)
		{
			StringBuilder stringBuilder = new();

			foreach (Asset asset in assets)
			{
				stringBuilder.AppendLine($"{asset.Name}: {asset.PriceUsd.ToString("#,0.000", _numberFormatInfo)} USD");
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region IDiagnosticService Implementation

		private readonly DateTime _serviceStartedTime = DateTime.UtcNow;
		private TimeSpan _serviceUptime => DateTime.UtcNow - _serviceStartedTime;

		public Dictionary<string, string> GetDiagnosticsInfo()
		{
			TimeZoneInfo mskTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Belarus Standard Time");

			return new Dictionary<string, string>
			{
				{ "Service Started Time MSK", TimeZoneInfo.ConvertTimeFromUtc(_serviceStartedTime, mskTimeZoneInfo).ToString() },
				{ "Service Uptime", _serviceUptime.ToString() },
				{ "Periodic Timer Initialized", (_periodicTimer is not null).ToString() },
				{ "Previous Timer Tick MSK", TimeZoneInfo.ConvertTimeFromUtc(_previousTimerTick, mskTimeZoneInfo).ToString() },
				{ "Next Timer Tick MSK", TimeZoneInfo.ConvertTimeFromUtc(_nextTimerTick, mskTimeZoneInfo).ToString() }
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
