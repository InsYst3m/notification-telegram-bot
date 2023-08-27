namespace NotificationTelegramBot.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendDailyNotificationAsync(CancellationToken cancellationToken);
    }
}
