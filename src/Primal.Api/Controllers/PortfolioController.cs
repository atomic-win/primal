using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

[Route("api/investments/[controller]")]
public sealed class PortfolioController : ApiController
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly IMapper mapper;
	private readonly ISender mediator;

	public PortfolioController(IHttpContextAccessor httpContextAccessor, IMapper mapper, ISender mediator)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.mapper = mapper;
		this.mediator = mediator;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetPortfolioAsync([FromBody] PortfolioRequest portfolioRequest)
	{
		var userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getPortfolioQuery = this.mapper.Map<(UserId, PortfolioRequest), GetPortfolioQuery>((userId, portfolioRequest));

		var errorOrPortfolio = await this.mediator.Send(getPortfolioQuery);

		return errorOrPortfolio.Match(
			portfolios => this.Ok(this.mapper.Map<IEnumerable<Portfolio>, IEnumerable<PortfolioResponse>>(portfolios)),
			errors => this.Problem(errors));
	}
}
