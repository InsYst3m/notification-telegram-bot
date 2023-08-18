using System.Text.RegularExpressions;

namespace NotificationTelegramBot.API.Constants
{
    public static partial class Regexes
    {
        [GeneratedRegex("(/get )(\\w+)")]
        public static partial Regex GetCommandRegex();
    }
}
