using NotificationTelegramBot.Assets.Clients.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

namespace NotificationTelegramBot.Assets.Services;
public sealed class AssetService : IAssetService
{
	#region Private Fields

	/// <summary>
	/// TODO: Mode to the database.
	/// </summary>
	private readonly List<string> _predefinedCryptoAssets =
	[
		"bitcoin",
		"ethereum",
		"polkadot",
		"near-protocol",
		"litecoin",
		"cosmos",
		"xrp",
		"solana",
		"dash",
		"cardano",
		"mina",
		"helium",
		"polygon"
	];

	private readonly ICoinApiClient _coinApiClient;

	#endregion

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

	public async Task<List<Asset>> GetAssetsAsync(ICollection<string> assets, CancellationToken cancellationToken)
	{
		return await _coinApiClient.GetAssetsAsync(assets, cancellationToken);
	}

	public Task<List<string>> GetFavouriteAssetsAsync(long chatId, CancellationToken cancellationToken)
	{
		return Task.FromResult(_predefinedCryptoAssets);
	}

	#endregion
}
