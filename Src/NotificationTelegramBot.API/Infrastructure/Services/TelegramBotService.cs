using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Constants;
using NotificationTelegramBot.API.Infrastructure.Commands;
using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

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
	private readonly IAssetService _assetService;
	private readonly IMessageProvider _messageProvider;
	private readonly ILogger<TelegramBotService> _logger;
	private readonly CancellationTokenSource _tokenSource;

	public TelegramBotService(
		IOptions<NotificationTelegramBotOptions> options,
		ITelegramBotClient telegramClient,
		ICommandProvider commandProvider,
		IAssetService assetService,
		IMessageProvider messageProvider,
		ILogger<TelegramBotService> logger)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(telegramClient);
		ArgumentNullException.ThrowIfNull(assetService);
		ArgumentNullException.ThrowIfNull(commandProvider);
		ArgumentNullException.ThrowIfNull(messageProvider);
		ArgumentNullException.ThrowIfNull(logger);

		_telegramClient = telegramClient;
		_assetService = assetService;
		_commandProvider = commandProvider;
		_messageProvider = messageProvider;
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


		//string result = string.Empty;

		//if (!string.IsNullOrWhiteSpace(message.Text))
		//{
		//	result = message.Text switch
		//	{
		//		string text when text.Equals(Commands.ASSETS_COMMAND, StringComparison.OrdinalIgnoreCase) =>
		//			await ProcessAssetsCommandAsync(cancellationToken),
		//		string text when text.StartsWith(Commands.GET_COMMAND) =>
		//			await ProcessGetCommandAsync(text, cancellationToken),
		//		_ => $"Unable to parse command: '{message.Text}'."
		//	};
		//}

		//await _telegramClient.SendTextMessageAsync(_options.ChatId, result, cancellationToken: cancellationToken);
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

				return _messageProvider.GenerateCryptoAssetMessage(foundAsset);
			}
			catch
			{
				return $"Unable to found asset: '{asset}'.";
			}
		}

		return $"Unable to parse command: '{getCommand}'.";
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
