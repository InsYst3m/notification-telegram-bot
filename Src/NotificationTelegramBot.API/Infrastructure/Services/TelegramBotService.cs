using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Constants;
using NotificationTelegramBot.API.Infrastructure.Commands;
using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services.Interfaces;
using NotificationTelegramBot.Assets.Entities;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NotificationTelegramBot.API.Infrastructure.Services;

public sealed class TelegramBotService : ITelegramBotService, IHostedService, IDisposable
{
	private readonly NotificationTelegramBotOptions _options;
	private readonly ITelegramBotClient _telegramClient;
	private readonly ICommandProvider _commandProvider;
	private readonly ILogger<TelegramBotService> _logger;
	private readonly CancellationTokenSource _tokenSource;

	public TelegramBotService(
		IOptions<NotificationTelegramBotOptions> options,
		ITelegramBotClient telegramClient,
		ICommandProvider commandProvider,
		ILogger<TelegramBotService> logger)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(telegramClient);
		ArgumentNullException.ThrowIfNull(commandProvider);
		ArgumentNullException.ThrowIfNull(logger);

		_telegramClient = telegramClient;
		_commandProvider = commandProvider;
		_logger = logger;

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
		ICommand command = update.Type switch
		{
			UpdateType.Message => OnMessageReceived(update.Message!, cancellationToken),
			_ => _commandProvider.NotFound
		};

		try
		{
			await command.ExecuteAsync(update.Message!.Chat.Id);
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
	/// <param name="message">The telegram message.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	private ICommand OnMessageReceived(Message message, CancellationToken cancellationToken)
	{
		if (message.Type is MessageType.Text)
		{
			return _commandProvider.GetOrCreate(message.Text);
		}
		else
		{
			return _commandProvider.NotFound;
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
