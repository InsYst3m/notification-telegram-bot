using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationTelegramBot.Database.Extensions;

/// <summary>
/// Provides database related extensions.
/// </summary>
public static class DependencyInjection
{
	/// <summary>
	/// Adds database layer.
	/// </summary>
	public static IServiceCollection AddDatabaseLayer(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddPooledDbContextFactory<ApplicationDbContext>(
			optionsBuilder =>
			{
				optionsBuilder.UseSqlServer(
					configuration.GetConnectionString("SqlDatabase"));

#if DEBUG

				optionsBuilder.LogTo(message =>
				{
					Console.WriteLine(message);
				});

#endif
			});

		return services;
	}
}
