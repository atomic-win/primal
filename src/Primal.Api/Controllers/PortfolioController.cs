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
	[Route("all")]
	public async Task<IActionResult> GetPortfolioAsync([FromQuery] Currency currency)
	{
		var userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getPortfolioQuery = new GetPortfolioQuery<string>(
			userId,
			currency,
			(transaction, asset, investmentInstrument) => string.Empty);

		var errorOrPortfolio = await this.mediator.Send(getPortfolioQuery);

		return errorOrPortfolio.Match(
			portfolios => this.Ok(this.mapper.Map<IEnumerable<Portfolio<string>>, IEnumerable<PortfolioResponse<string>>>(portfolios)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("instrument-type")]
	public async Task<IActionResult> GetPortfolioPerInstrumentTypeAsync([FromQuery] Currency currency)
	{
		var userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getPortfolioPerInstrumentTypeQuery = new GetPortfolioQuery<InstrumentType>(
			userId,
			currency,
			(transaction, asset, investmentInstrument) => investmentInstrument.Type);

		var errorOrPortfolioPerInstrumentTypes = await this.mediator.Send(getPortfolioPerInstrumentTypeQuery);

		return errorOrPortfolioPerInstrumentTypes.Match(
			portfolioPerInstrumentTypes => this.Ok(this.mapper.Map<IEnumerable<Portfolio<InstrumentType>>, IEnumerable<PortfolioResponse<string>>>(portfolioPerInstrumentTypes)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("instrument")]
	public async Task<IActionResult> GetPortfolioPerInstrumentAsync([FromQuery] Currency currency)
	{
		var userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getPortfolioPerInstrumentQuery = new GetPortfolioQuery<InstrumentId>(
			userId,
			currency,
			(transaction, asset, investmentInstrument) => investmentInstrument.Id);

		var errorOrPortfolioPerInstruments = await this.mediator.Send(getPortfolioPerInstrumentQuery);

		return errorOrPortfolioPerInstruments.Match(
			portfolioPerInstruments => this.Ok(this.mapper.Map<IEnumerable<Portfolio<InstrumentId>>, IEnumerable<PortfolioResponse<Guid>>>(portfolioPerInstruments)),
			errors => this.Problem(errors));
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
