using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;

public interface IMessageProvider
{
    string GenerateCryptoAssetsPriceMessage(List<Asset> assets);
    string GenerateCryptoAssetMessage(Asset asset);
}
