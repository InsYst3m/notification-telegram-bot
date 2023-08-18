using System.ComponentModel.DataAnnotations;

namespace NotificationTelegramBot.API.Options
{
    public sealed class CoinApiOptions
    {
        /// <summary>
        /// Gets or sets the service url
        /// </summary>
        [Required]
        public required string ServiceUrl { get; set; }

        [Required]
        public required int RequestTimeoutInSec { get; set; }
    }
}
