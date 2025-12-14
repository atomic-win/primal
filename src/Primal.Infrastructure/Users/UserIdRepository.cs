using Primal.Application.Users;
using Primal.Domain.Users;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.Users;

internal sealed class UserIdRepository : IUserIdRepository
{
	private readonly AppDbContext appDbContext;

	internal UserIdRepository(AppDbContext appDbContext)
	{
		this.appDbContext = appDbContext;
	}

	public async Task<UserId> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken)
	{
		var userIdTableEntity = await this.appDbContext.UserIds.FindAsync(identityProviderUserId.Value, identityProvider, cancellationToken);
		return userIdTableEntity is null ? UserId.Empty : new UserId(userIdTableEntity.UserId);
	}

	public async Task<UserId> AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken)
	{
		var userIdTableEntity = new UserIdTableEntity
		{
			IdentityProvider = identityProvider,
			Id = identityProviderUserId.Value,
			UserId = Guid.CreateVersion7(),
		};

		await this.appDbContext.UserIds.AddAsync(userIdTableEntity, cancellationToken);

		return new UserId(userIdTableEntity.UserId);
	}
}
