using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

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
	[Route("asset")]
	public async Task<IActionResult> GetPortfolioPerAssetAsync([FromQuery] Currency currency)
	{
		var userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getPortfolioPerAssetQuery = new GetPortfolioQuery<AssetId>(
			userId,
			currency,
			(transaction, asset, investmentInstrument) => asset.Id);

		var errorOrPortfolioPerAssets = await this.mediator.Send(getPortfolioPerAssetQuery);

		return errorOrPortfolioPerAssets.Match(
			portfolioPerAssets => this.Ok(this.mapper.Map<IEnumerable<Portfolio<AssetId>>, IEnumerable<PortfolioResponse<Guid>>>(portfolioPerAssets)),
			errors => this.Problem(errors));
	}
}
