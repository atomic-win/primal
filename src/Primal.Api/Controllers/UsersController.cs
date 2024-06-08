using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Users;
using Primal.Contracts.Users;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

public sealed class UsersController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public UsersController(IMapper mapper, IMediator mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mapper = mapper;
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	[Route("me/profile")]
	[Route("{id:guid}/profile")]
	public async Task<IActionResult> GetUserProfileAsync([FromRoute] Guid id)
	{
		if (id == Guid.Empty)
		{
			id = this.httpContextAccessor.HttpContext.GetUserId().Value;
		}

		var getUserQuery = new GetUserQuery(this.mapper.Map<Guid, UserId>(id));

		var errorOrUserResult = await this.mediator.Send(getUserQuery);

		return errorOrUserResult.Match(
			userResult => this.Ok(this.mapper.Map<User, UserProfileResponse>(userResult)),
			errors => this.Problem(errors));
	}
}
