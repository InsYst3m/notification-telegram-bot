using Microsoft.EntityFrameworkCore;

using NotificationTelegramBot.Database.Entities;

namespace NotificationTelegramBot.Database;

public sealed class ApplicationDbContext : DbContext
{
	public DbSet<User> Users { get; set; }

	public ApplicationDbContext() : base() { }
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>();
	}
}
