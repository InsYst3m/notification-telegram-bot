using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Commands;

public sealed class NotFoundCommand : ICommand
{
	private readonly ITelegramBotClient _telegramBotClient;

	public NotFoundCommand(ITelegramBotClient telegramBotClient)
	{
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
	}

	public async Task ExecuteAsync(long chatId, CancellationToken cancellationToken)
	{
		await _telegramBotClient.SendTextMessageAsync(chatId, "Unable to parse command.", cancellationToken: cancellationToken);
	}
}
