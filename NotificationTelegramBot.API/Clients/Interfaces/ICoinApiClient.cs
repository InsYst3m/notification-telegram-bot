using NotificationTelegramBot.API.Entities;

namespace NotificationTelegramBot.API.Clients.Interfaces
{
    public interface ICoinApiClient
    {
        Task<Asset> GetCryptoAssetAsync(string asset, CancellationToken cancellationToken);
        Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken);
    }
}
