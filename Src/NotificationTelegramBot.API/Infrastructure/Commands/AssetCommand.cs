using System.Text.RegularExpressions;

using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.Assets.Entities;
using NotificationTelegramBot.Assets.Services.Interfaces;

using Telegram.Bot;

namespace NotificationTelegramBot.API.Infrastructure.Commands;

public sealed partial class AssetCommand : ICommand
{
	#region Constants

	public const string NAME = "/asset";

	#endregion

	#region Public Fields

	[GeneratedRegex("(/asset )(.+)")]
	public static partial Regex AssetCommanRegex();

	#endregion

	#region Private Fields

	private readonly IAssetService _assetService;
	private readonly IMessageProvider _messageProvider;
	private readonly ITelegramBotClient _telegramBotClient;
	private readonly string _text;

	#endregion

	public AssetCommand(
		IAssetService assetService,
		IMessageProvider messageProvider,
		ITelegramBotClient telegramBotClient,
		string text)
	{
		_assetService = assetService ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_messageProvider = messageProvider ?? throw new ArgumentNullException(nameof(messageProvider));
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));

		_text = text;
	}

	public async Task ExecuteAsync(long chatId, CancellationToken cancellationToken)
	{
		string result = string.Empty;

		Match match = AssetCommanRegex().Match(_text);

		if (match.Success)
		{
			string asset = match.Groups[2].Value;

			try
			{
				Asset foundAsset = await _assetService.GetAssetAsync(asset, cancellationToken);

				result = _messageProvider.GenerateCryptoAssetMessage(foundAsset);
			}
			catch
			{
				result = $"Unable to found asset: '{asset}'.";
			}
		}

		await _telegramBotClient.SendTextMessageAsync(chatId, result, cancellationToken: cancellationToken);
	}
}
