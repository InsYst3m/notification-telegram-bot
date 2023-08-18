using System.Text.Json;
using System.Text.Json.Serialization;

using NotificationTelegramBot.API.Clients.Interfaces;
using NotificationTelegramBot.API.Entities;

namespace NotificationTelegramBot.API.Clients
{
    public sealed class CoinApiClient : ICoinApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CoinApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Asset> GetCryptoAssetAsync(string asset, CancellationToken cancellationToken)
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
                throw new BadHttpRequestException("Unable to parse HTTP response.");
            }

            return result;
        }
    }
}
