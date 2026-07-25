using Microsoft.Extensions.Caching.Hybrid;

using InvestmentPortfolioTracker.Core.Users;
using InvestmentPortfolioTracker.Domain.Money;
using InvestmentPortfolioTracker.Domain.Users;

namespace InvestmentPortfolioTracker.Infrastructure.Users;

internal sealed class CachedUserRepository : IUserRepository
{
	private readonly HybridCache cache;
	private readonly IUserRepository userRepository;

	internal CachedUserRepository(
		HybridCache cache,
		IUserRepository userRepository)
	{
		this.cache = cache;
		this.userRepository = userRepository;
	}

	public async Task<User> GetUserAsync(
		UserId userId,
		CancellationToken cancellationToken)
	{
		return await this.cache.GetOrCreateAsync(
			userId.UserKey(),
			async entry => await this.userRepository.GetUserAsync(userId, cancellationToken),
			cancellationToken: cancellationToken);
	}

	public async Task<User> AddUserAsync(
		string email,
		string firstName,
		string lastName,
		string fullName,
		CancellationToken cancellationToken)
	{
		return await this.userRepository.AddUserAsync(
			email,
			firstName,
			lastName,
			fullName,
			cancellationToken);
	}

	public async Task UpdateUserProfileAsync(
		UserId userId,
		Currency preferredCurrency,
		Locale preferredLocale,
		CancellationToken cancellationToken)
	{
		await this.userRepository.UpdateUserProfileAsync(
			userId,
			preferredCurrency,
			preferredLocale,
			cancellationToken);

		await this.cache.RemoveAsync(
			userId.UserKey(),
			cancellationToken);
	}
}
