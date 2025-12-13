using Microsoft.EntityFrameworkCore;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
	internal DbSet<UserIdTableEntity> UserIds { get; set; } = null!;

	internal DbSet<UserTableEntity> Users { get; set; } = null!;

	internal DbSet<AssetTableEntity> Assets { get; set; } = null!;

	internal DbSet<AssetItemTableEntity> AssetItems { get; set; } = null!;

	internal DbSet<TransactionTableEntity> Transactions { get; set; } = null!;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "EF constructor pattern")]
	public AppDbContext(DbContextOptions<AppDbContext> options)
		  : base(options)
	{
	}

	public override int SaveChanges()
	{
		this.UpdateTimestamps();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		this.UpdateTimestamps();
		return base.SaveChangesAsync(cancellationToken);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		this.ConfigureUserIdEntity(modelBuilder);
		this.ConfigureUserEntity(modelBuilder);
		this.ConfigureAssetEntity(modelBuilder);
		this.ConfigureAssetItemEntity(modelBuilder);
		this.ConfigureTransactionEntity(modelBuilder);
	}

	private void ConfigureUserIdEntity(ModelBuilder modelBuilder)
	{
		this.ConfigureCommonEntities<UserIdTableEntity>(modelBuilder);

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
	}

	private void ConfigureUserEntity(ModelBuilder modelBuilder)
	{
		this.ConfigureCommonEntities<UserTableEntity>(modelBuilder);

		modelBuilder.Entity<UserTableEntity>(entity =>
		{
			entity.ToTable("users");

			entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
			entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
			entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
			entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
			entity.Property(e => e.PreferredCurrency).IsRequired().HasConversion<string>().HasDefaultValue(Currency.USD);
			entity.Property(e => e.PreferredLocale).IsRequired().HasConversion<string>().HasDefaultValue(Locale.EN_US);

			entity.HasKey(e => e.Id);
			entity.HasIndex(e => e.Email).IsUnique();
		});
	}

	private void ConfigureAssetEntity(ModelBuilder modelBuilder)
	{
		this.ConfigureCommonEntities<AssetTableEntity>(modelBuilder);

		modelBuilder.Entity<AssetTableEntity>(entity =>
		{
			entity.ToTable("assets");

			entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
			entity.Property(e => e.AssetClass).IsRequired().HasConversion<string>();
			entity.Property(e => e.AssetType).IsRequired().HasConversion<string>();
			entity.Property(e => e.Currency).IsRequired().HasConversion<string>();
			entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(100);

			entity.HasKey(e => e.Id);
			entity.HasIndex(e => e.ExternalId).IsUnique();
		});
	}

	private void ConfigureAssetItemEntity(ModelBuilder modelBuilder)
	{
		this.ConfigureCommonEntities<AssetItemTableEntity>(modelBuilder);

		modelBuilder.Entity<AssetItemTableEntity>(entity =>
		{
			entity.ToTable("asset_items");

			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

			entity.HasOne<AssetTableEntity>()
				  .WithMany()
				  .HasForeignKey(e => e.AssetId)
				  .OnDelete(DeleteBehavior.Restrict);

			entity.HasOne<UserTableEntity>()
				  .WithMany()
				  .HasForeignKey(e => e.UserId)
				  .OnDelete(DeleteBehavior.Cascade);
		});
	}

	private void ConfigureTransactionEntity(ModelBuilder modelBuilder)
	{
		this.ConfigureCommonEntities<TransactionTableEntity>(modelBuilder);

		modelBuilder.Entity<TransactionTableEntity>(entity =>
		{
			entity.ToTable("transactions");

			entity.HasKey(e => e.Id);
			entity.Property(e => e.Date).IsRequired();
			entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
			entity.Property(e => e.TransactionType).IsRequired().HasConversion<string>();
			entity.Property(e => e.Units).IsRequired();

			entity.HasOne<AssetItemTableEntity>()
				  .WithMany()
				  .HasForeignKey(e => e.AssetItemId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasOne<UserTableEntity>()
				  .WithMany()
				  .HasForeignKey(e => e.UserId)
				  .OnDelete(DeleteBehavior.Cascade);
		});
	}

	private void ConfigureCommonEntities<T>(ModelBuilder modelBuilder)
		  where T : TableEntity
	{
		modelBuilder.Entity<T>(entity =>
		{
			entity.Property(e => e.CreatedAt).IsRequired();
			entity.Property(e => e.UpdatedAt).IsRequired();
		});
	}

	private void UpdateTimestamps()
	{
		var now = DateTimeOffset.UtcNow;

		foreach (var entry in this.ChangeTracker.Entries<TableEntity>())
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = now;
			}

			entry.Entity.UpdatedAt = now;
		}
	}
}
