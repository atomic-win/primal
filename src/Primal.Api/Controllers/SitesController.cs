using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Sites.Commands;
using Primal.Contracts.Sites;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

public sealed class SitesController : ApiController
{
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public SitesController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	public IActionResult All()
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();
		return this.Ok(userId);
	}

	[HttpPost]
	public async Task<IActionResult> AddSiteAsync([FromBody] AddSiteRequest request, CancellationToken cancellationToken)
	{
		HttpContext httpContext = this.httpContextAccessor.HttpContext;

		var addSiteCommand = new AddSiteCommand(
			httpContext.GetUserId(),
			request.Host.Host,
			request.DailyLimitInMinutes);

		var errorOrSiteResult = await this.mediator.Send(addSiteCommand, cancellationToken);

		return errorOrSiteResult.Match(
			siteResult => this.Created(
				$"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}/{siteResult.Id.Value}",
				new SiteResponse(siteResult.Id.Value, siteResult.Host, siteResult.DailyLimitInMinutes)),
			errors => this.Problem(errors));
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
