using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Sites;
using Primal.Contracts.Sites;
using Primal.Domain.Sites;
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
			request.Url,
			request.DailyLimitInMinutes);

		var errorOrSiteResult = await this.mediator.Send(addSiteCommand, cancellationToken);

		return errorOrSiteResult.Match(
			siteResult => this.Created(
				$"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}/{siteResult.Id.Value}",
				new SiteResponse(siteResult.Id.Value, siteResult.Url, siteResult.DailyLimitInMinutes)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}")]
	public async Task<IActionResult> GetSiteByIdAsync([FromRoute] Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getSiteQuery = new GetSiteByIdQuery(userId, new SiteId(id));

		var errorOrSiteResult = await this.mediator.Send(getSiteQuery);

		return errorOrSiteResult.Match(
			siteResult => this.Ok(new SiteResponse(siteResult.Id.Value, siteResult.Url, siteResult.DailyLimitInMinutes)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("byurl")]
	public async Task<IActionResult> GetSiteByUrlAsync([FromQuery] Uri url)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getSiteByUrlQuery = new GetSiteByUrlQuery(userId, url);

		var errorOrSiteResult = await this.mediator.Send(getSiteByUrlQuery);

		return errorOrSiteResult.Match(
			siteResult => this.Ok(new SiteResponse(siteResult.Id.Value, siteResult.Url, siteResult.DailyLimitInMinutes)),
			errors => this.Problem(errors));
	}
}
