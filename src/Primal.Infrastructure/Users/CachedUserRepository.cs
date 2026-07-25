using Microsoft.Extensions.Caching.Hybrid;
using Primal.Application.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Users;

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
			$"users/{userId.Value}",
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
			$"users/{userId.Value}",
			cancellationToken);
	}
}
