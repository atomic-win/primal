using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Authentication;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Application.Authentication;

internal sealed class SignInCommandHandler : IRequestHandler<SignInCommand, ErrorOr<SignInResult>>
{
	private readonly IIdTokenValidator identityTokenValidator;
	private readonly IUserIdRepository userIdRepository;
	private readonly IUserRepository userRepository;
	private readonly ITokenIssuer tokenIssuer;

	public SignInCommandHandler(IIdTokenValidator identityTokenValidator, IUserIdRepository userIdRepository, IUserRepository userRepository, ITokenIssuer tokenIssuer)
	{
		this.identityTokenValidator = identityTokenValidator;
		this.userIdRepository = userIdRepository;
		this.userRepository = userRepository;
		this.tokenIssuer = tokenIssuer;
	}

	public async Task<ErrorOr<SignInResult>> Handle(SignInCommand request, CancellationToken cancellationToken)
	{
		var errorOrIdentityUser = await this.identityTokenValidator.Validate(request.IdToken);

		return await errorOrIdentityUser.MatchAsync(
			identityProviderUser => this.HandleTokenValidationSuccess(identityProviderUser, cancellationToken),
			errors => Task.FromResult((ErrorOr<SignInResult>)errors));
	}

	private async Task<ErrorOr<SignInResult>> HandleTokenValidationSuccess(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUserId = await this.userIdRepository.GetUserId(
			identityProviderUser.IdentityProvider,
			identityProviderUser.Id,
			cancellationToken);

		if (!errorOrUserId.IsError)
		{
			return await this.HandleGetUserIdSuccess(errorOrUserId.Value, identityProviderUser, cancellationToken);
		}

		if (errorOrUserId.FirstError is { Type: ErrorType.NotFound })
		{
			return await this.HandleUserIdNotFound(identityProviderUser, cancellationToken);
		}

		return (ErrorOr<SignInResult>)errorOrUserId.Errors;
	}

	private async Task<ErrorOr<SignInResult>> HandleGetUserIdSuccess(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUser = await this.userRepository.GetUserAsync(userId, cancellationToken);

		if (!errorOrUser.IsError)
		{
			return await this.HandleGetUserSuccess(errorOrUser.Value, cancellationToken);
		}

		if (errorOrUser.FirstError is { Type: ErrorType.NotFound })
		{
			return await this.HandleUserNotFound(userId, identityProviderUser, cancellationToken);
		}

		return (ErrorOr<SignInResult>)errorOrUser.Errors;
	}

	private async Task<ErrorOr<SignInResult>> HandleUserIdNotFound(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUserId = await this.userIdRepository.AddUserId(
			identityProviderUser.IdentityProvider,
			identityProviderUser.Id,
			cancellationToken);

		if (!errorOrUserId.IsError)
		{
			return await this.HandleGetUserIdSuccess(errorOrUserId.Value, identityProviderUser, cancellationToken);
		}

		if (errorOrUserId.FirstError is { Type: ErrorType.Conflict })
		{
			return await this.HandleTokenValidationSuccess(identityProviderUser, cancellationToken);
		}

		return (ErrorOr<SignInResult>)errorOrUserId.Errors;
	}

	private async Task<ErrorOr<SignInResult>> HandleGetUserSuccess(User user, CancellationToken cancellationToken)
	{
		var errorOrToken = await this.tokenIssuer.IssueToken(user, cancellationToken);

		return errorOrToken.Match(
			token => new SignInResult(token),
			errors => (ErrorOr<SignInResult>)errors);
	}

	private async Task<ErrorOr<SignInResult>> HandleUserNotFound(UserId userId, IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		var errorOrUser = await this.userRepository.AddUserAsync(
			identityProviderUser.Email,
			identityProviderUser.FirstName,
			identityProviderUser.LastName,
			identityProviderUser.FullName,
			identityProviderUser.ProfilePictureUrl,
			cancellationToken);

		if (!errorOrUser.IsError)
		{
			return await this.HandleGetUserSuccess(errorOrUser.Value, cancellationToken);
		}

		if (errorOrUser.FirstError is { Type: ErrorType.Conflict })
		{
			return await this.HandleGetUserIdSuccess(userId, identityProviderUser, cancellationToken);
		}

		return (ErrorOr<SignInResult>)errorOrUser.Errors;
	}
}
