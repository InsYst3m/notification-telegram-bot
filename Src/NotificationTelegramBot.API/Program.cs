using Azure.Identity;

using Microsoft.Extensions.Options;

using NotificationTelegramBot.API.Options;
using NotificationTelegramBot.API.Services;
using NotificationTelegramBot.API.Services.Interfaces;
using NotificationTelegramBot.Assets.Extensions;
using NotificationTelegramBot.Database.Extensions;

using Telegram.Bot;

try
{
	#region Base Configuration Setup

	WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

	IServiceCollection services = builder.Services;
	IConfiguration configuration = builder.Configuration;

	#endregion

	#region Configure Logging

	services.AddLogging(builder =>
	{
		builder.AddConsole();
	});

	#endregion

	#region Configure Services

	services
		.AddOptions<NotificationTelegramBotOptions>()
		.BindConfiguration(nameof(NotificationTelegramBotOptions))
		.ValidateDataAnnotations()
		.ValidateOnStart();

	services
		.AddSingleton<DefaultAzureCredential>();

	services.AddDatabaseLayer(configuration);
	services.AddAssetsModule();

	services.AddSingleton<ITelegramBotClient, TelegramBotClient>(
		serviceProvider =>
		{
			NotificationTelegramBotOptions options =
				serviceProvider.GetRequiredService<IOptions<NotificationTelegramBotOptions>>().Value;

			return new TelegramBotClient(options.Token);
		});

	services.AddSingleton<IUserService, UserService>();
	services.AddSingleton<ITelegramBotService, TelegramBotService>();
	services.AddSingleton<IDiagnosticService>(x => x.GetRequiredService<ITelegramBotService>());

	services.AddHostedService(x => x.GetRequiredService<ITelegramBotService>());

	#endregion

	#region Configure Middleware Pipeline

	WebApplication app = builder.Build();

	#endregion

	Console.WriteLine("Starting application host.");

	await app.RunAsync();
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
}
finally
{
	Console.WriteLine("Stopped application host.");
}

/// <summary>
/// The main entry point for the application.
/// </summary>
public partial class Program { }
