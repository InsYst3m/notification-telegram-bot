using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NotificationTelegramBot.Assets.Clients;
using NotificationTelegramBot.Assets.Clients.Interfaces;
using NotificationTelegramBot.Assets.Options;
using NotificationTelegramBot.Assets.Services;
using NotificationTelegramBot.Assets.Services.Interfaces;

namespace NotificationTelegramBot.Assets.Extensions;

public static class DependencyInjection
{
	public static IServiceCollection AddAssetsModule(this IServiceCollection services)
	{
		services
			.AddOptions<CoinApiOptions>()
			.BindConfiguration(nameof(CoinApiOptions));

		services.AddHttpClient(nameof(CoinApiClient), (serviceProvider, httpClient) =>
		{
			CoinApiOptions options =
				serviceProvider.GetRequiredService<IOptions<CoinApiOptions>>().Value;

			httpClient.BaseAddress = new Uri(options.ServiceUrl);
			httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutInSec);
		});

		services.AddSingleton<ICoinApiClient, CoinApiClient>();
		services.AddSingleton<IAssetService, AssetService>();

		return services;
	}
}
