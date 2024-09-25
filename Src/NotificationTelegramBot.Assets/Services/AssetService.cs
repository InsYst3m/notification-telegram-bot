using NotificationTelegramBot.Assets.Clients.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

namespace NotificationTelegramBot.Assets.Services;
public sealed class AssetService : IAssetService
{
	private readonly ICoinApiClient _coinApiClient;

	public AssetService(ICoinApiClient coinApiClient)
	{
		_coinApiClient = coinApiClient ?? throw new ArgumentNullException(nameof(coinApiClient));
	}

	#region IAssetService Members

	public async Task<Asset> GetAssetAsync(string asset, CancellationToken cancellationToken)
	{
		return await _coinApiClient.GetAssetAsync(asset, cancellationToken);
	}

	public async Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken)
	{
		return await _coinApiClient.GetAvailableAssetsAsync(cancellationToken);
	}

	public async Task<List<Asset>> GetAssetsAsync(string[] assets, CancellationToken cancellationToken)
	{
		return await _coinApiClient.GetAssetsAsync(assets, cancellationToken);
	}

	#endregion
}
