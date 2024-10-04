using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Services.Interfaces;

public interface INotificationService
{
    Task StartPeriodicNotificationsAsync(CancellationToken cancellationToken);
}
