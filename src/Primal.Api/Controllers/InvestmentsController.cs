using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

public sealed class InvestmentsController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public InvestmentsController(IMapper mapper, ISender mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mapper = mapper;
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	[Route("instruments")]
	public async Task<IActionResult> GetAllInstrumentsAsync()
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getInstrumentsQuery = new GetAllInstrumentsQuery(userId);

		var errorOrInstruments = await this.mediator.Send(getInstrumentsQuery);

		return errorOrInstruments.Match(
			instruments => this.Ok(this.mapper.Map<IEnumerable<InvestmentInstrument>, IEnumerable<InstrumentResponse>>(instruments)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("instruments/mutualfunds")]
	public async Task<IActionResult> AddMutualFundInstrumentAsync([FromBody] AddMutualFundInstrumentRequest addMutualFundInstrumentRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addMutualFundInstrumentCommand = this.mapper.Map<AddMutualFundInstrumentCommand>((userId, addMutualFundInstrumentRequest));

		var errorOrMutualFundInstrument = await this.mediator.Send(addMutualFundInstrumentCommand);

		return errorOrMutualFundInstrument.Match(
			mutualFundInstrument => this.Created(
				$"{this.httpContextAccessor.HttpContext.Request.Scheme}://{this.httpContextAccessor.HttpContext.Request.Host}{this.httpContextAccessor.HttpContext.Request.Path}/{mutualFundInstrument.Id.Value}",
				this.mapper.Map<MutualFundInstrumentResponse>(mutualFundInstrument)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("instruments/{id:guid}")]
	public async Task<IActionResult> GetInstrumentByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getInstrumentByIdQuery = new GetInstrumentByIdQuery(userId, new InstrumentId(id));

		var errorOrInstrument = await this.mediator.Send(getInstrumentByIdQuery);

		return errorOrInstrument.Match(
			instrument => this.Ok(this.mapper.Map<InvestmentInstrument, InstrumentResponse>(instrument)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("metadata/mutualfunds/{id:guid}")]
	public async Task<IActionResult> GetMutualFundByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getMutualFundByIdQuery = new GetMutualFundByIdQuery(new MutualFundId(id));

		var errorOrMutualFund = await this.mediator.Send(getMutualFundByIdQuery);

		return errorOrMutualFund.Match(
			mutualFund => this.Ok(this.mapper.Map<MutualFund, MutualFundResponse>(mutualFund)),
			errors => this.Problem(errors));
	}
}
