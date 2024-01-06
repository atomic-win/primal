using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
	private readonly IIdentityTokenValidator identityTokenValidator;
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;
	private readonly ITokenIssuer tokenIssuer;

	public RegisterCommandHandler(IIdentityTokenValidator identityTokenValidator, IUserIdRepository userIdRepository, IUserRepository userRepository, ITokenIssuer tokenIssuer)
	{
		this.identityTokenValidator = identityTokenValidator;
		this.userIdRepository = userIdRepository;
		this.userRepository = userRepository;
		this.tokenIssuer = tokenIssuer;
	}

	public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		var errorOrIdentityUser = await this.identityTokenValidator.Validate(request.IdToken);

		return await errorOrIdentityUser.MatchAsync(
			identityProviderUser => this.Handle(identityProviderUser, cancellationToken),
			errors => Task.FromResult((ErrorOr<AuthenticationResult>)errors));
	}

	private async Task<ErrorOr<AuthenticationResult>> Handle(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUserId = await this.userIdRepository.GetUserId(identityProviderUser, cancellationToken);

		return await errorOrUserId.MatchAsync(
			userId => this.Handle(userId, identityProviderUser, cancellationToken),
			errors => Task.FromResult((ErrorOr<AuthenticationResult>)errors));
	}

	private async Task<ErrorOr<AuthenticationResult>> Handle(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUser = await this.userRepository.GetUser(userId, identityProviderUser, cancellationToken);

		return await errorOrUser.MatchAsync(
			user => this.IssueToken(user, cancellationToken),
			errors => Task.FromResult((ErrorOr<AuthenticationResult>)errors));
	}

	private async Task<ErrorOr<AuthenticationResult>> IssueToken(User user, CancellationToken cancellationToken)
	{
		var errorOrToken = await this.tokenIssuer.IssueToken(user, cancellationToken);

		return errorOrToken.Match(
			token => new AuthenticationResult(token),
			errors => (ErrorOr<AuthenticationResult>)errors);
	}
}
