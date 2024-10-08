using System.Text.Json;
using System.Text.Json.Serialization;

using NotificationTelegramBot.Assets.Clients.Interfaces;
using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.Assets.Clients;

public sealed class CoinApiClient : ICoinApiClient
{
	private readonly IHttpClientFactory _httpClientFactory;

	public CoinApiClient(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
	}

	public async Task<List<string>> GetAvailableAssetsAsync(CancellationToken cancellationToken)
	{
		HttpClient httpClient =
			_httpClientFactory.CreateClient(nameof(CoinApiClient));

		HttpResponseMessage httpResponse = await httpClient.GetAsync($"/v2/assets", cancellationToken);

		httpResponse.EnsureSuccessStatusCode();

		string jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		JsonDocument jsonDocument = JsonDocument.Parse(jsonResponse);
		Asset[]? assets = jsonDocument.RootElement
			.GetProperty("data")
			.Deserialize<Asset[]>(
				new JsonSerializerOptions
				{
					NumberHandling = JsonNumberHandling.AllowReadingFromString,
					PropertyNameCaseInsensitive = true
				});

		List<string> result = new();

		if (assets is not null && assets.Any())
		{
			result.AddRange(assets.Select(x => x.Id));
		}

		return result;
	}

	public async Task<Asset> GetAssetAsync(string asset, CancellationToken cancellationToken)
	{
		HttpClient httpClient =
			_httpClientFactory.CreateClient(nameof(CoinApiClient));

		HttpResponseMessage httpResponse = await httpClient.GetAsync($"/v2/assets/{asset}", cancellationToken);

		httpResponse.EnsureSuccessStatusCode();

		string jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		JsonDocument jsonDocument = JsonDocument.Parse(jsonResponse);
		Asset? result = jsonDocument.RootElement
			.GetProperty("data")
			.Deserialize<Asset>(
				new JsonSerializerOptions
				{
					NumberHandling = JsonNumberHandling.AllowReadingFromString,
					PropertyNameCaseInsensitive = true
				});

		if (result is null)
		{
			throw new InvalidOperationException("Unable to parse HTTP response.");
		}

		return result;
	}

	public async Task<List<Asset>> GetAssetsAsync(ICollection<string> assets, CancellationToken cancellationToken)
	{
		HttpClient httpClient =
			_httpClientFactory.CreateClient(nameof(CoinApiClient));

		HttpResponseMessage httpResponse =
			await httpClient.GetAsync(
				$"/v2/assets?ids={string.Join(',', assets)}",
				cancellationToken);

		httpResponse.EnsureSuccessStatusCode();

		string jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		JsonDocument jsonDocument = JsonDocument.Parse(jsonResponse);
		List<Asset>? result = jsonDocument.RootElement
			.GetProperty("data")
			.Deserialize<List<Asset>>(
				new JsonSerializerOptions
				{
					NumberHandling = JsonNumberHandling.AllowReadingFromString,
					PropertyNameCaseInsensitive = true
				});

		if (result is null)
		{
			throw new InvalidOperationException("Unable to parse HTTP response.");
		}

		return result;
	}
}
