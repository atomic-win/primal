using Primal.Api.Auth;

namespace Primal.Api.UnitTests.Api.Auth.GoogleLogin;

public sealed class GoogleLoginValidatorTests
{
	[Test]
	public async Task ValidateAsync_ReturnsValid_WhenIdTokenIsProvided()
	{
		var validator = new GoogleLoginValidator();
		var request = new GoogleLoginRequest("token-value");

		var result = await validator.ValidateAsync(request);

		await Verifier.Verify(result);
	}

	[Test]
	public async Task ValidateAsync_ReturnsError_WhenIdTokenIsEmpty()
	{
		var validator = new GoogleLoginValidator();
		var request = new GoogleLoginRequest(string.Empty);

		var result = await validator.ValidateAsync(request);

		await Verifier.Verify(result);
	}
}
