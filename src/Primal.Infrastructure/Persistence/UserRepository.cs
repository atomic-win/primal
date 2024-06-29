using System.Net.Mail;
using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserRepository : IUserRepository
{
	private readonly LiteDatabase liteDatabase;

	internal UserRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<UserTableEntity>("Users");
		collection.EnsureIndex(x => x.Id, unique: true);
	}

	public async Task<ErrorOr<Success>> AddUserAsync(User user, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserTableEntity>("Users");

		if (collection.FindById(user.Id.Value) != null)
		{
			return Error.Conflict(description: "User already exists.");
		}

		var userTableEntity = new UserTableEntity
		{
			Id = user.Id.Value,
			Email = user.Email.Address,
			FirstName = user.FirstName,
			LastName = user.LastName,
			FullName = user.FullName,
			ProfilePictureUrl = user.ProfilePictureUrl.AbsoluteUri,
		};

		collection.Insert(userTableEntity);

		return Result.Success;
	}

	public async Task<ErrorOr<User>> GetUserAsync(UserId userId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserTableEntity>("Users");

		var userTableEntity = collection.FindById(userId.Value);

		if (userTableEntity == null)
		{
			return Error.NotFound(description: "User does not exist.");
		}

		return new User(
			new UserId(userTableEntity.Id),
			new MailAddress(userTableEntity.Email),
			userTableEntity.FirstName,
			userTableEntity.LastName,
			userTableEntity.FullName,
			new Uri(userTableEntity.ProfilePictureUrl));
	}

	private sealed class UserTableEntity
	{
		public Guid Id { get; init; }

		public string Email { get; init; }

		public string FirstName { get; init; }

		public string LastName { get; init; }

		public string FullName { get; init; }

		public string ProfilePictureUrl { get; init; }
	}
}
