using System.ComponentModel.DataAnnotations;

namespace NotificationTelegramBot.API.Options
{
    public sealed class NotificationTelegramBotOptions
    {
        /// <summary>
        /// Gets or sets the telegram bot token.
        /// </summary>
        [Required]
        public required string Token { get; set; }

        /// <summary>
        /// Gets or sets the main telegram chat identifier.
        /// </summary>
        [Required]
        public required long ChatId { get; set; }
    }
}
