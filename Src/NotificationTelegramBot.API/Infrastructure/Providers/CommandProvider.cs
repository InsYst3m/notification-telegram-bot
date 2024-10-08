using NotificationTelegramBot.API.Infrastructure.Commands;
using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Providers;

public sealed class CommandProvider : ICommandProvider
{
	private readonly IServiceProvider _serviceProvider;

	public CommandProvider(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public ICommand NotFound => new NotFoundCommand(
		_serviceProvider.GetRequiredService<ITelegramBotClient>());

	public ICommand GetOrCreate(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return NotFound;
		}

		ICommand? result = text switch
		{
			string value when text.Equals(AssetsCommand.NAME, StringComparison.OrdinalIgnoreCase) =>
				new AssetsCommand(
					_serviceProvider.GetRequiredService<IAssetService>(),
					_serviceProvider.GetRequiredService<ITelegramBotClient>()),
			string value when text.StartsWith(AssetCommand.NAME, StringComparison.OrdinalIgnoreCase) =>
				new AssetCommand(
					_serviceProvider.GetRequiredService<IAssetService>(),
					_serviceProvider.GetRequiredService<IMessageProvider>(),
					_serviceProvider.GetRequiredService<ITelegramBotClient>(),
					value),
			string value when text.StartsWith(FavouriteAssetsCommand.NAME, StringComparison.OrdinalIgnoreCase) =>
				new FavouriteAssetsCommand(
					_serviceProvider.GetRequiredService<IAssetService>(),
					_serviceProvider.GetRequiredService<IMessageProvider>(),
					_serviceProvider.GetRequiredService<ITelegramBotClient>()),
			_ => NotFound
		};

		return result;
	}
}
