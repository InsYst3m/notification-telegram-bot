using NotificationTelegramBot.Database;
using NotificationTelegramBot.Database.Entities;

namespace NotificationTelegramBot.API.Services.Interfaces;

/// <summary>
/// Describes methods to manipulate users data.
/// </summary>
public interface IUserService
{
	/// <summary>
	/// Gets user by the provided <paramref name="chatId"/> telegram chat identifier.
	/// </summary>
	/// <param name="dbContext">The application database context.</param>
	/// <param name="chatId">The telegram chat identifier.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>
	/// User if found otherwise null.
	/// </returns>
	Task<User?> GetAsync(ApplicationDbContext dbContext, long chatId, CancellationToken cancellationToken);

	/// <summary>
	/// Creates user based on the provided model <paramref name="user"/>.
	/// </summary>
	/// <param name="dbContext">The application database context.</param>
	/// <param name="user">The user.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<User> CreateAsync(ApplicationDbContext dbContext, User user, CancellationToken cancellationToken);

	/// <summary>
	/// Gets or creates user by the provided <paramref name="chatId"/> telegram chat identifier.
	/// </summary>
	/// <param name="dbContext">The application database context.</param>
	/// <param name="chatId">The telegram chat identifier.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<User> GetOrCreateAsync(ApplicationDbContext dbContext, long chatId, CancellationToken cancellationToken);
}
