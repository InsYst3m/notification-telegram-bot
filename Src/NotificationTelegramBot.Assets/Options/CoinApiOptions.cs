using System.ComponentModel.DataAnnotations;

namespace NotificationTelegramBot.Assets.Options;

public sealed class CoinApiOptions
{
	/// <summary>
	/// Gets or sets the service url
	/// </summary>
	[Required]
	public required string ServiceUrl { get; set; }

	[Required]
	public required int TimeoutInSec { get; set; }
}

