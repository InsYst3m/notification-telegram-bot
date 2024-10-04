using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Commands;

public sealed class AssetsCommand : ICommand
{
	#region Constants

	public const string NAME = "assets";

	#endregion

	#region Private Fields

	private readonly IAssetService _assetService;
	private readonly ITelegramBotClient _telegramBotClient;

	#endregion

	public AssetsCommand(
		IAssetService assetService,
		ITelegramBotClient telegramBotClient)
	{
		_assetService = assetService ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
	}

	public async Task ExecuteAsync(long chatId, CancellationToken cancellationToken)
	{
		string result = string.Empty;

		List<string> availableAssets = await _assetService.GetAvailableAssetsAsync(cancellationToken);

		if (availableAssets.Any())
		{
			result = string.Join(", ", availableAssets);
		}
		else
		{
			result = "Unable to get available assets.";
		}

		await _telegramBotClient.SendTextMessageAsync(chatId, result, cancellationToken: cancellationToken);
	}
}
