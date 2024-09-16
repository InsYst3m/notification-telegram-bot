namespace NotificationTelegramBot.API.Models;

public sealed class Asset
{
	public required string Id { get; set; }
	public required int Rank { get; set; }
	public required string Symbol { get; set; }
	public required string Name { get; set; }
	public required decimal PriceUsd { get; set; }
	public required decimal VolumeUsd24Hr { get; set; }
	public required decimal MarketCapUsd { get; set; }
}
