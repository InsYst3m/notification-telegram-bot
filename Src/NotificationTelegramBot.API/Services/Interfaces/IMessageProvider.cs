using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.API.Services.Interfaces;

public interface IMessageProvider
{
	string GenerateCryptoAssetsPriceMessage(List<Asset> assets);
	string GenerateCryptoAssetMessage(Asset asset);
}
