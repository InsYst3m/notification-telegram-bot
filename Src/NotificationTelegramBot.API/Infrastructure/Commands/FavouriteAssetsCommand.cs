using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Commands;

public sealed class FavouriteAssetsCommand : ICommand
{
	#region Constants

	public const string NAME = "/favouriteassets";

	#endregion

	#region Private Fields

	private readonly IAssetService _assetService;
	private readonly IMessageProvider _messageProvider;
	private readonly ITelegramBotClient _telegramBotClient;

	#endregion

	public FavouriteAssetsCommand(
		IAssetService assetService,
		IMessageProvider messageProvider,
		ITelegramBotClient telegramBotClient)
	{
		_assetService = assetService ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
	}

	public async Task ExecuteAsync(long chatId, CancellationToken cancellationToken)
	{
		string result = string.Empty;

		List<string> favouriteAssets = await _assetService.GetFavouriteAssetsAsync(chatId, cancellationToken);

		if (favouriteAssets.Any())
		{
			List<Asset> assets = await _assetService.GetAssetsAsync(favouriteAssets, cancellationToken);

			result = _messageProvider.GenerateCryptoAssetsPriceMessage(assets);
		}
		else
		{
			result = "Unable to get favourite assets.";
		}

		await _telegramBotClient.SendTextMessageAsync(chatId, result, cancellationToken: cancellationToken);
	}
}
