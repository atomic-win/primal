using ErrorOr;
using MediatR;
using Primal.Application.Authentication.Common;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Application.Authentication.Commands;

internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
{
	private readonly IIdTokenValidator identityTokenValidator;
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;
	private readonly ITokenIssuer tokenIssuer;

	public RegisterCommandHandler(IIdTokenValidator identityTokenValidator, IUserIdRepository userIdRepository, IUserRepository userRepository, ITokenIssuer tokenIssuer)
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
			identityProviderUser => this.HandleTokenValidationSuccess(identityProviderUser, cancellationToken),
			errors => Task.FromResult((ErrorOr<AuthenticationResult>)errors));
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleTokenValidationSuccess(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUserId = await this.userIdRepository.GetUserId(identityProviderUser, cancellationToken);

		if (!errorOrUserId.IsError)
		{
			return await this.HandleGetUserIdSuccess(errorOrUserId.Value, identityProviderUser, cancellationToken);
		}

		if (errorOrUserId.FirstError is { Type: ErrorType.NotFound })
		{
			return await this.HandleUserIdNotFound(identityProviderUser, cancellationToken);
		}

		return (ErrorOr<AuthenticationResult>)errorOrUserId.Errors;
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleGetUserIdSuccess(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUser = await this.userRepository.GetUser(userId, cancellationToken);

		if (!errorOrUser.IsError)
		{
			return await this.HandleGetUserSuccess(errorOrUser.Value, cancellationToken);
		}

		if (errorOrUser.FirstError is { Type: ErrorType.NotFound })
		{
			return await this.HandleUserNotFound(userId, identityProviderUser, cancellationToken);
		}

		return (ErrorOr<AuthenticationResult>)errorOrUser.Errors;
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleUserIdNotFound(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var userId = UserId.New();

		var errorOrAddUserId = await this.userIdRepository.AddUserId(identityProviderUser, userId, cancellationToken);

		if (!errorOrAddUserId.IsError)
		{
			return await this.HandleGetUserIdSuccess(userId, identityProviderUser, cancellationToken);
		}

		if (errorOrAddUserId.FirstError is { Type: ErrorType.Conflict })
		{
			return await this.HandleTokenValidationSuccess(identityProviderUser, cancellationToken);
		}

		return (ErrorOr<AuthenticationResult>)errorOrAddUserId.Errors;
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleGetUserSuccess(User user, CancellationToken cancellationToken)
	{
		var errorOrToken = await this.tokenIssuer.IssueToken(user, cancellationToken);

		return errorOrToken.Match(
			token => new AuthenticationResult(token),
			errors => (ErrorOr<AuthenticationResult>)errors);
	}

	private async Task<ErrorOr<AuthenticationResult>> HandleUserNotFound(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var user = new User(userId, identityProviderUser.Email);

		var errorOrAddUser = await this.userRepository.AddUser(user, cancellationToken);

		if (!errorOrAddUser.IsError)
		{
			return await this.HandleGetUserSuccess(user, cancellationToken);
		}

		if (errorOrAddUser.FirstError is { Type: ErrorType.Conflict })
		{
			return await this.HandleGetUserIdSuccess(userId, identityProviderUser, cancellationToken);
		}

		return (ErrorOr<AuthenticationResult>)errorOrAddUser.Errors;
	}
}
