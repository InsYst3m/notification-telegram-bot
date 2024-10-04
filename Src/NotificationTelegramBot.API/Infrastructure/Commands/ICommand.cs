namespace NotificationTelegramBot.API.Infrastructure.Commands;

public interface ICommand
{
	Task ExecuteAsync(long chatId, CancellationToken cancellationToken = default);
}
