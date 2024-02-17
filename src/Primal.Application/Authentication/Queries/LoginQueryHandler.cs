using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Application.Authentication.Queries;

internal sealed class LoginQueryHandler : IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
	private readonly IIdTokenValidator identityTokenValidator;
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;
	private readonly ITokenIssuer tokenIssuer;

	public LoginQueryHandler(IIdTokenValidator identityTokenValidator, IUserIdRepository userIdRepository, IUserRepository userRepository, ITokenIssuer tokenIssuer)
	{
		this.identityTokenValidator = identityTokenValidator;
		this.userIdRepository = userIdRepository;
		this.userRepository = userRepository;
		this.tokenIssuer = tokenIssuer;
	}

	public async Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery request, CancellationToken cancellationToken)
	{
		var errorOrIdentityUser = await this.identityTokenValidator.Validate(request.IdToken);

		return await errorOrIdentityUser.MatchAsync(
			identityProviderUser => this.HandleTokenValidationSuccess(identityProviderUser, cancellationToken),
			errors => Task.FromResult((ErrorOr<AuthenticationResult>)errors));
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleTokenValidationSuccess(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUserId = await this.userIdRepository.GetUserId(identityProviderUser, cancellationToken);

		if (errorOrUserId.IsError)
		{
			return (ErrorOr<AuthenticationResult>)errorOrUserId.Errors;
		}

		return await this.HandleGetUserIdSuccess(errorOrUserId.Value, cancellationToken);
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleGetUserIdSuccess(UserId userId, CancellationToken cancellationToken)
	{
		var errorOrUser = await this.userRepository.GetUser(userId, cancellationToken);

		if (errorOrUser.IsError)
		{
			return (ErrorOr<AuthenticationResult>)errorOrUser.Errors;
		}

		return await this.HandleGetUserSuccess(errorOrUser.Value, cancellationToken);
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleGetUserSuccess(User user, CancellationToken cancellationToken)
	{
		var errorOrToken = await this.tokenIssuer.IssueToken(user, cancellationToken);

		return errorOrToken.Match(
			token => new AuthenticationResult(token),
			errors => (ErrorOr<AuthenticationResult>)errors);
	}
}
