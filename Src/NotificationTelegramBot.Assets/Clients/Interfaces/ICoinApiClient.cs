using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.Assets.Clients.Interfaces;

public interface ICoinApiClient
{
	Task<Asset> GetAssetAsync(string asset, CancellationToken cancellationToken);
	Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken);
	Task<List<Asset>> GetAssetsAsync(ICollection<string> assets, CancellationToken cancellationToken);
}

