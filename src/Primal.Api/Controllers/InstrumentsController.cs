using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Api.Controllers;

[Route("api/investments/[controller]")]
public sealed class InstrumentsController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;

	public InstrumentsController(IMapper mapper, ISender mediator)
	{
		this.mapper = mapper;
		this.mediator = mediator;
	}

	[HttpGet]
	[Route("{id:guid}")]
	public async Task<IActionResult> GetInstrumentByIdAsync(Guid id)
	{
		var getInstrumentByIdQuery = new GetInstrumentByIdQuery(new InstrumentId(id));

		var errorOrInstrument = await this.mediator.Send(getInstrumentByIdQuery);

		return errorOrInstrument.Match(
			instrument => this.Ok(this.mapper.Map<InvestmentInstrument, InstrumentResponse>(instrument)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}/historical")]
	public async Task<IActionResult> GetInstrumentHistoricalAsync(Guid id, DateOnly startDate, DateOnly endDate)
	{
		var getInstrumentValueQuery = new GetInstrumentValueQuery(new InstrumentId(id), startDate, endDate);

		var errorOrInstrumentValues = await this.mediator.Send(getInstrumentValueQuery);

		return errorOrInstrumentValues.Match(
			instrumentValues => this.Ok(instrumentValues),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("exchangerates")]
	public async Task<IActionResult> GetExchangeRatesAsync(Currency from, Currency to, DateOnly startDate, DateOnly endDate)
	{
		var getExchangeRatesQuery = new GetExchangeRatesQuery(from, to, startDate, endDate);

		var errorOrExchangeRates = await this.mediator.Send(getExchangeRatesQuery);

		return errorOrExchangeRates.Match(
			exchangeRates => this.Ok(exchangeRates),
			errors => this.Problem(errors));
	}
}
