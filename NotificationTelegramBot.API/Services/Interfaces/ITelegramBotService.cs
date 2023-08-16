using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationTelegramBot.API.Services.Interfaces
{
    public interface ITelegramBotService : IHostedService, IDiagnosticService
    {
        /// <summary>
        /// Handles errors occurs in the telegram <paramref name="client"/>.
        /// </summary>
        /// <param name="client">The telegram client.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken);

        /// <summary>
        /// Handles messages coming from the the telegram <paramref name="client"/>.
        /// </summary>
        /// <param name="client">The telegram client.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
    }
}
