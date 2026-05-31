using FluentValidation.Results;
using Primal.Api.Users;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.UnitTests.Api.Users.UpdateUser;

public sealed class UpdateUserValidatorTests
{
	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenPreferredCurrencyIsProvided()
	{
		var validator = new UpdateUserValidator();
		var request = new UpdateUserRequest(Guid.Empty, Currency.USD, Locale.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenPreferredLocaleIsProvided()
	{
		var validator = new UpdateUserValidator();
		var request = new UpdateUserRequest(Guid.Empty, Currency.Unknown, Locale.EN_IN);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenNoFieldsAreProvided()
	{
		var validator = new UpdateUserValidator();
		var request = new UpdateUserRequest(Guid.Empty, Currency.Unknown, Locale.Unknown);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, "At least one field of preferred currency or preferred locale must be provided");
	}

	private static async Task AssertHasError(ValidationResult result, string errorMessage)
	{
		await Assert.That(result.Errors.Any(x => string.Equals(x.ErrorMessage, errorMessage, StringComparison.Ordinal))).IsTrue();
	}
}
