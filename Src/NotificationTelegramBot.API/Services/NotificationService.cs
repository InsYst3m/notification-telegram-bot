using NotificationTelegramBot.API.Services.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Services;

public sealed class NotificationService : INotificationService, IHostedService, IDisposable
{
	#region Private Fields

	private PeriodicTimer? _periodicTimer;
	private DateTime _nextTimerTick;
	private DateTime _previousTimerTick;

	/// <summary>
	/// TODO: Mode to the database.
	/// </summary>
	private string[] _predefinedCryptoAssets = new[]
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


	private readonly ITelegramBotClient _telegramBotClient;
	private readonly IAssetService _assetService;
	private readonly IMessageProvider _messageProvider;
	private readonly ILogger<NotificationService> _logger;

	#endregion

	#region Constructors

	public NotificationService(
		ITelegramBotClient telegramBotClient,
		IAssetService assetService,
		IMessageProvider messageProvider,
		ILogger<NotificationService> logger)
	{
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
		_messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region IHostedService Members

	public Task StartAsync(CancellationToken cancellationToken)
	{
		Task.Run(
			() => StartPeriodicNotificationsAsync(cancellationToken),
			cancellationToken);

		_logger.LogInformation("Periodic notifications was successfully initialized.");

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		Dispose();

		return Task.CompletedTask;
	}

	#endregion

	#region INotificationService Members

	public async Task StartPeriodicNotificationsAsync(CancellationToken cancellationToken)
	{
		const short interval = 6;

		_periodicTimer = new(TimeSpan.FromHours(6));
		_nextTimerTick = _nextTimerTick.AddHours(interval);

		while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
		{
			_previousTimerTick = _nextTimerTick;
			_nextTimerTick = _nextTimerTick.AddHours(interval);

			_ = SendNotificationAsync(_telegramBotClient, cancellationToken);
		}
	}

	#endregion

	#region Private Members

	/// <summary>
	/// Provides handler to periodicaly send notifications with asset prices.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	private async Task SendNotificationAsync(
		ITelegramBotClient client,
		CancellationToken cancellationToken)
	{
		// TODO: get assets based on the database data

		List<Asset> foundAssets = await _assetService.GetAssetsAsync(_predefinedCryptoAssets, cancellationToken);

		string message = _messageProvider.GenerateCryptoAssetsPriceMessage(foundAssets);

		//await client.SendTextMessageAsync(
		//	_options.ChatId, message, cancellationToken: cancellationToken);
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
				_periodicTimer?.Dispose();
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
