using FluentValidation.Results;
using Primal.Api.Auth;
using Primal.Api.Errors;

namespace Primal.Api.UnitTests.Api.Auth.GoogleLogin;

public sealed class GoogleLoginValidatorTests
{
	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenIdTokenIsProvided()
	{
		var validator = new GoogleLoginValidator();
		var request = new GoogleLoginRequest("token-value");

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsTrue();
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenIdTokenIsEmpty()
	{
		var validator = new GoogleLoginValidator();
		var request = new GoogleLoginRequest(string.Empty);

		var result = await validator.ValidateAsync(request);

		await Assert.That(result.IsValid).IsFalse();
		await AssertHasError(result, ErrorMessages.Auth.IdTokenRequired);
	}

	private static async Task AssertHasError(ValidationResult result, string errorMessage)
	{
		await Assert.That(result.Errors.Any(x => string.Equals(x.ErrorMessage, errorMessage, StringComparison.Ordinal))).IsTrue();
	}
}
