using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Contracts.Sites;
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

	[HttpPost]
	public IActionResult AddSite([FromBody] AddSiteRequest request)
	{
		HttpContext httpContext = this.httpContextAccessor.HttpContext;

		Guid siteId = Guid.NewGuid();

		string resourceUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}/{siteId}";

		return this.Created(resourceUrl, new SiteResponse(siteId, request.Url, request.DailyLimitInMinutes));
	}

	[HttpGet]
	[Route("{id}")]
	public IActionResult GetSite(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();
		return this.Ok(id);
	}

	[HttpPatch]
	[Route("{id}")]
	public IActionResult UpdateSite(Guid id, [FromBody] PatchSiteRequest request)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();
		return this.Ok(id);
	}

	[HttpDelete]
	[Route("{id}")]
	public IActionResult DeleteSite(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();
		return this.Ok(id);
	}
}
