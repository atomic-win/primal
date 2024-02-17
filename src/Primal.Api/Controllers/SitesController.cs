using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

public sealed class SitesController : ApiController
{
	private readonly IHttpContextAccessor httpContextAccessor;

	public SitesController(IHttpContextAccessor httpContextAccessor)
	{
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	public IActionResult All()
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();
		return this.Ok(userId);
	}
}
