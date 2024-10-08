using Microsoft.EntityFrameworkCore;

using NotificationTelegramBot.API.Infrastructure.Services.Interfaces;
using NotificationTelegramBot.Database;
using NotificationTelegramBot.Database.Entities;

namespace NotificationTelegramBot.API.Infrastructure.Services;

/// <summary>
/// Provides methods to manipulate users data.
/// </summary>
/// <seealso cref="IUserService" />
public sealed class UserService : IUserService
{
	#region IUserService Members

	/// <inheritdoc cref="IUserService.GetAsync" />
	public Task<User?> GetAsync(ApplicationDbContext dbContext, long chatId, CancellationToken cancellationToken)
	{
		return dbContext.Users.FirstOrDefaultAsync(x => x.ChatId == chatId, cancellationToken);
	}

	/// <inheritdoc cref="IUserService.CreateAsync" />
	public Task<User> CreateAsync(ApplicationDbContext dbContext, User user, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc cref="IUserService.GetOrCreateAsync" />
	public async Task<User> GetOrCreateAsync(ApplicationDbContext dbContext, long chatId, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	#endregion
}
