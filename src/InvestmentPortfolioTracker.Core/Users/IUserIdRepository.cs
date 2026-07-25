using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Users;

public interface IUserIdRepository
{
	Task<UserId> GetUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		CancellationToken cancellationToken);

	Task AddUserId(
		IdentityProvider identityProvider,
		IdentityProviderUserId identityProviderUserId,
		UserId userId,
		CancellationToken cancellationToken);
}
