using Microsoft.Extensions.DependencyInjection;

using NotificationTelegramBot.Steam.Options;

namespace NotificationTelegramBot.Steam.Extensions;
public static class DependencyInjection
{
	public static IServiceCollection AddSteamApi(this IServiceCollection services, SteamOptions steamOptions)
	{
		return services;
	}
}
