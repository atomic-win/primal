using System.Net.Mail;
using Primal.Application.Users;
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
		var userTableEntity = await this.appDbContext.Users.FindAsync(userId.Value, cancellationToken);
		if (userTableEntity is null)
		{
			return User.Empty;
		}

		return this.MapToUser(userTableEntity);
	}

	public async Task<User> AddUserAsync(
		UserId userId,
		MailAddress email,
		string fullName,
		CancellationToken cancellationToken)
	{
		var userTableEntity = new UserTableEntity
		{
			Id = userId.Value,
			Email = email.Address,
			FullName = fullName,
		};

		await this.appDbContext.Users.AddAsync(userTableEntity, cancellationToken);

		return this.MapToUser(userTableEntity);
	}

	private User MapToUser(UserTableEntity userTableEntity)
	{
		return new User(
			new UserId(userTableEntity.Id),
			new MailAddress(userTableEntity.Email),
			userTableEntity.FullName);
	}
}
