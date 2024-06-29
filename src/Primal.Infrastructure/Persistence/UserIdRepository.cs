using ErrorOr;
using LiteDB;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdRepository : IUserIdRepository
{
	private readonly LiteDatabase liteDatabase;

	internal UserIdRepository(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;
	}

	public async Task<ErrorOr<Success>> AddUserId(IdentityProviderUser identityProviderUser, UserId userId, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserIdTableEntity>("UserIds");

		if (collection.FindById(identityProviderUser.Id.Value) != null)
		{
			return Error.Conflict(description: "Identity provider user already has a user ID.");
		}

		var userIdTableEntity = new UserIdTableEntity
		{
			Id = identityProviderUser.Id.Value,
			IdentityProvider = identityProviderUser.IdentityProvider,
			UserId = userId.Value,
		};

		collection.Insert(userIdTableEntity);
		return Result.Success;
	}

	public async Task<ErrorOr<UserId>> GetUserId(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<UserIdTableEntity>("UserIds");

		var userIdTableEntity = collection.FindById(identityProviderUser.Id.Value);

		if (userIdTableEntity == null)
		{
			return Error.NotFound(description: "Identity provider user does not have a user ID.");
		}

		return new UserId(userIdTableEntity.UserId);
	}

	private sealed class UserIdTableEntity
	{
		public string Id { get; init; }

		public IdentityProvider IdentityProvider { get; init; }

		public Guid UserId { get; init; }
	}
}
