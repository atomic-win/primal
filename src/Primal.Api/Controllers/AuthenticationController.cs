using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Primal.Application.Authentication;
using Primal.Contracts.Authentication;

namespace Primal.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]/[action]")]
public sealed class AuthenticationController : ApiController
{
	private readonly ISender mediator;
	private readonly IMapper mapper;

	public AuthenticationController(IMediator mediator, IMapper mapper)
	{
		this.mediator = mediator;
		this.mapper = mapper;
	}

	[HttpPost]
	public async Task<ActionResult<SignInResponse>> SignIn(SignInRequest request, CancellationToken cancellationToken)
	{
		SignInCommand registerCommand = this.mapper.Map<SignInCommand>(request);

		ErrorOr<Application.Authentication.SignInResult> authResult = await this.mediator.Send(registerCommand, cancellationToken);

		return authResult.Match(
			authenticationResult => this.Ok(this.mapper.Map<SignInResponse>(authenticationResult)),
			errors => this.Problem(errors));
	}
}
