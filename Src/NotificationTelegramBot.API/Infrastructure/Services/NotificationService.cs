using Microsoft.EntityFrameworkCore;

using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.API.Infrastructure.Services.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;
using NotificationTelegramBot.Database;
using NotificationTelegramBot.Database.Entities;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Services;

public sealed class NotificationService : INotificationService, IHostedService, IDisposable
{
	#region Private Fields

	private PeriodicTimer? _periodicTimer;
	private DateTime _nextTimerTick;
	private DateTime _previousTimerTick;

	private readonly ITelegramBotClient _telegramBotClient;
	private readonly IAssetService _assetService;
	private readonly IMessageProvider _messageProvider;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
	private readonly ILogger<NotificationService> _logger;

	#endregion

	#region Constructors

	public NotificationService(
		ITelegramBotClient telegramBotClient,
		IAssetService assetService,
		IMessageProvider messageProvider,
		IDbContextFactory<ApplicationDbContext> dbContextFactory,
		ILogger<NotificationService> logger)
	{
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
		_messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
		_dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
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
		bool initialPeriodChanged = false;
		DateTime midDay = DateTime.Today.AddHours(12);

		TimeSpan defaultPeriod = TimeSpan.FromHours(12);
		TimeSpan initialPeriod = DateTime.Now >= midDay
			? DateTime.Today.AddDays(1).Subtract(DateTime.Now)
			: midDay.Subtract(DateTime.Now);

		_periodicTimer = new(initialPeriod);

		while (await _periodicTimer.WaitForNextTickAsync(cancellationToken))
		{
			if (!initialPeriodChanged)
			{
				_periodicTimer.Period = defaultPeriod;

				initialPeriodChanged = true;
			}

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
		using ApplicationDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		List<User> subscribers = dbContext.Users
			.Where(x => x.NotifyAboutFavouriteAssets)
			.Include(x => x.FavouriteAssets)
			.ToList();

		List<string> assets = subscribers
			.SelectMany(x => x.FavouriteAssets)
			.Distinct()
			.ToList();

		if (!assets.Any())
		{
			assets = await _assetService.GetFavouriteAssetsAsync(0, cancellationToken);
		}

		List<Asset> foundAssets = await _assetService.GetAssetsAsync(assets, cancellationToken);

		foreach (User subscriber in subscribers)
		{
			if (!subscriber.FavouriteAssets.Any())
			{
				continue;
			}

			List<Asset> targetAssets = foundAssets
				.Where(x => subscriber.FavouriteAssets.Contains(x.Id))
				.ToList();

			string message = _messageProvider.GenerateCryptoAssetsPriceMessage(targetAssets);

			await client.SendTextMessageAsync(
				subscriber.ChatId, message, cancellationToken: cancellationToken);
		}
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
