using NotificationTelegramBot.API.Models;

namespace NotificationTelegramBot.API.Clients.Interfaces
{
	public interface ICoinApiClient
	{
		Task<Asset> GetAssetAsync(string asset, CancellationToken cancellationToken);
		Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken);
		Task<List<Asset>> GetAssetsAsync(string[] assets, CancellationToken cancellationToken);
	}
}
