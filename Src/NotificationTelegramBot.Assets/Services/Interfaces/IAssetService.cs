using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.Assets.Services.Interfaces;
public interface IAssetService
{
	Task<Asset> GetAssetAsync(string asset, CancellationToken cancellationToken);

	Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken);

	Task<List<Asset>> GetAssetsAsync(string[] assets, CancellationToken cancellationToken);
}
