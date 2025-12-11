using Microsoft.EntityFrameworkCore;

namespace Primal.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
	internal DbSet<UserIdTableEntity> UserIds { get; set; } = null!;

	internal DbSet<UserTableEntity> Users { get; set; } = null!;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "EF constructor pattern")]
	public AppDbContext(DbContextOptions<AppDbContext> options)
		  : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// --- UserId ---
		modelBuilder.Entity<UserIdTableEntity>(entity =>
		{
			entity.ToTable("user_ids");
			entity.HasKey(e => new { e.Id, e.IdentityProvider }); // Composite PK
			entity.Property(e => e.IdentityProvider).HasConversion<string>();

			entity.HasOne<UserTableEntity>()
				  .WithMany()
				  .HasForeignKey(e => e.UserId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		// --- User ---
		modelBuilder.Entity<UserTableEntity>(entity =>
		{
			entity.ToTable("users");
			entity.HasKey(e => e.Id);
			entity.HasIndex(e => e.Email).IsUnique();
		});
	}
}
