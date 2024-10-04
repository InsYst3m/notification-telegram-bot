using NotificationTelegramBot.API.Infrastructure.Commands;

namespace NotificationTelegramBot.API.Infrastructure.Providers.Interfaces;

public interface ICommandProvider
{
	ICommand NotFound { get; }

	ICommand GetOrCreate(string? text);
}
