using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;
using SequentialGuid;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdRepository : IUserIdRepository
{
	private readonly LiteDatabase liteDatabase;

	internal UserIdRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<UserIdTableEntity>("UserIds");
		collection.EnsureIndex(x => x.Id, unique: true);
	}

	public async Task<ErrorOr<UserId>> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserIdTableEntity>("UserIds");

		var userIdTableEntity = collection.FindById(identityProviderUserId.Value);

		if (userIdTableEntity == null)
		{
			return Error.NotFound(description: "Identity provider user does not have a user ID.");
		}

		return new UserId(userIdTableEntity.UserId);
	}

	public async Task<ErrorOr<UserId>> AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserIdTableEntity>("UserIds");

		if (collection.FindById(identityProviderUserId.Value) != null)
		{
			return Error.Conflict(description: "Identity provider user already has a user ID.");
		}

		var userIdTableEntity = new UserIdTableEntity
		{
			Id = identityProviderUserId.Value,
			IdentityProvider = identityProvider,
			UserId = SequentialGuidGenerator.Instance.NewGuid(),
		};

		collection.Insert(userIdTableEntity);
		return new UserId(userIdTableEntity.UserId);
	}

	private sealed class UserIdTableEntity
	{
		public string Id { get; init; }

		public IdentityProvider IdentityProvider { get; init; }

		public Guid UserId { get; init; }
	}
}
