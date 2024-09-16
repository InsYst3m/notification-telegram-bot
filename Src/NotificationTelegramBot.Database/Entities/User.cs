using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace NotificationTelegramBot.Database.Entities;

/// <summary>
/// Describes the user entity.
/// </summary>
[PrimaryKey(nameof(Id))]
public sealed class User
{
	/// <summary>
	/// The user unique identifier.
	/// </summary>
	[Key]
	public required string Id { get; set; }

	/// <summary>
	/// The user name.
	/// </summary>
	[MinLength(1)]
	public required string Name { get; set; }

	/// <summary>
	/// The user telegram chat identifier.
	/// </summary>
	public long ChatId { get; set; }
}
