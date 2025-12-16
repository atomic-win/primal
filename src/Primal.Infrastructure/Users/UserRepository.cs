using Microsoft.EntityFrameworkCore;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Users;

internal sealed class UserRepository : IUserRepository
{
	private readonly AppDbContext appDbContext;

	internal UserRepository(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		var userTableEntity = await this.appDbContext.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

		if (userTableEntity is null)
		{
			return User.Empty;
		}

		return this.MapToUser(userTableEntity);
	}

	public async Task<User> AddUserAsync(
		UserId userId,
		string email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken)
	{
		var userTableEntity = new UserTableEntity
		{
			Id = userId.Value,
			Email = email,
			FirstName = firstName,
			LastName = lastName,
			FullName = fullName,
			PreferredCurrency = Currency.USD,
			PreferredLocale = Locale.EN_US,
		};

		await this.appDbContext.Users.AddAsync(userTableEntity, cancellationToken);

		return this.MapToUser(userTableEntity);
	}

	public async Task UpdateUserProfileAsync(
		UserId userId,
		Currency preferredCurrency,
		Locale preferredLocale,
		CancellationToken cancellationToken)
	{
		await this.appDbContext.Users.Where(u => u.Id == userId.Value)
			.ExecuteUpdateAsync(
				setters => setters
				.SetProperty(u => u.PreferredCurrency, preferredCurrency)
				.SetProperty(u => u.PreferredLocale, preferredLocale),
				cancellationToken);
	}

	private User MapToUser(UserTableEntity userTableEntity)
	{
		return new User(
			new UserId(userTableEntity.Id),
			userTableEntity.Email,
			userTableEntity.FirstName,
			userTableEntity.LastName,
			userTableEntity.FullName,
			userTableEntity.PreferredCurrency,
			userTableEntity.PreferredLocale);
	}
}
