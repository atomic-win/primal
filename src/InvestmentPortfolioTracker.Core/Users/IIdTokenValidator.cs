using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Core.Users;

public interface IIdTokenValidator
{
	Task<IdentityProviderUser> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
