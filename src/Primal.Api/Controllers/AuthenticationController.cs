using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Application.Authentication.Commands;
using Primal.Application.Authentication.Common;
using Primal.Application.Authentication.Queries;
using Primal.Contracts.Authentication;

namespace Primal.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class AuthenticationController : ControllerBase
{
	private readonly ISender mediator;
	private readonly IMapper mapper;

	public AuthenticationController(IMediator mediator, IMapper mapper)
	{
		this.mediator = mediator;
		this.mapper = mapper;
	}

	[HttpPost]
	public async Task<ActionResult<AuthenticationResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
	{
		RegisterCommand registerCommand = this.mapper.Map<RegisterCommand>(request);

		ErrorOr<AuthenticationResult> authResult = await this.mediator.Send(registerCommand, cancellationToken);

		return authResult.Match(
			authenticationResult => this.Ok(this.mapper.Map<AuthenticationResponse>(authenticationResult)),
			error => this.Ok(error));
	}

	[HttpGet]
	public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		LoginQuery loginQuery = this.mapper.Map<LoginQuery>(request);

		ErrorOr<AuthenticationResult> authResult = await this.mediator.Send(loginQuery, cancellationToken);

		return authResult.Match(
			authenticationResult => this.Ok(this.mapper.Map<AuthenticationResponse>(authenticationResult)),
			error => this.Ok(error));
	}
}
