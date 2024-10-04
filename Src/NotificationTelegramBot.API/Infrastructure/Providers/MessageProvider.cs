using System.Globalization;
using System.Text;
using NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;
using NotificationTelegramBot.Assets.Entities;

namespace NotificationTelegramBot.API.Infrastructure.Providers;

public sealed class MessageProvider : IMessageProvider
{
    private readonly NumberFormatInfo _numberFormatInfo;

    public MessageProvider()
    {
        _numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _numberFormatInfo.NumberGroupSeparator = " ";
    }

    #region IMessageProvider Members

    public string GenerateCryptoAssetsPriceMessage(List<Asset> assets)
    {
        StringBuilder stringBuilder = new();

        foreach (Asset asset in assets)
        {
            stringBuilder.AppendLine($"{asset.Name}: {asset.PriceUsd.ToString("#,0.000", _numberFormatInfo)} USD");
        }

        return stringBuilder.ToString();
    }

    public string GenerateCryptoAssetMessage(Asset asset)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"Asset: {asset.Name}");
        stringBuilder.AppendLine($"Price: {asset.PriceUsd.ToString("#,0.000", _numberFormatInfo)} USD");
        stringBuilder.AppendLine($"Rank: {asset.Rank}");
        stringBuilder.AppendLine($"Capitalization: {asset.MarketCapUsd.ToString("#,0.000", _numberFormatInfo)} USD");
        stringBuilder.AppendLine($"Volume 24 hours: {asset.VolumeUsd24Hr.ToString("#,0.000", _numberFormatInfo)} USD");

        return stringBuilder.ToString();
    }

    #endregion
}
