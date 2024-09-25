using Telegram.Bot;

namespace NotificationTelegramBot.API.Services.Interfaces;

public interface INotificationService
{
	Task StartPeriodicNotificationsAsync(CancellationToken cancellationToken);
}
