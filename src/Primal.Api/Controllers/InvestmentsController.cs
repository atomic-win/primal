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

		var errorOrInstrumentsResult = await this.mediator.Send(getInstrumentsQuery);

		return errorOrInstrumentsResult.Match(
			instrumentsResult => this.Ok(this.mapper.Map<IEnumerable<InstrumentResult>, IEnumerable<InstrumentResponse>>(instrumentsResult)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("instruments/mutualfunds")]
	public async Task<IActionResult> AddMutualFundInstrumentAsync([FromBody] AddMutualFundInstrumentRequest addMutualFundInstrumentRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addMutualFundInstrumentCommand = this.mapper.Map<AddMutualFundInstrumentCommand>((userId, addMutualFundInstrumentRequest));

		var errorOrMutualFundInstrumentResult = await this.mediator.Send(addMutualFundInstrumentCommand);

		return errorOrMutualFundInstrumentResult.Match(
			mutualFundInstrumentResult => this.Created(
				$"{this.httpContextAccessor.HttpContext.Request.Scheme}://{this.httpContextAccessor.HttpContext.Request.Host}{this.httpContextAccessor.HttpContext.Request.Path}/{mutualFundInstrumentResult.Id.Value}",
				this.mapper.Map<MutualFundInstrumentResponse>(mutualFundInstrumentResult)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("instruments/{id:guid}")]
	public async Task<IActionResult> GetInstrumentByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getInstrumentByIdQuery = new GetInstrumentByIdQuery(userId, new InstrumentId(id));

		var errorOrInstrumentResult = await this.mediator.Send(getInstrumentByIdQuery);

		return errorOrInstrumentResult.Match(
			instrumentResult => this.Ok(this.mapper.Map<InstrumentResult, InstrumentResponse>(instrumentResult)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("metadata/mutualfunds/{id:guid}")]
	public async Task<IActionResult> GetMutualFundByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getMutualFundByIdQuery = new GetMutualFundByIdQuery(new MutualFundId(id));

		var errorOrMutualFundResult = await this.mediator.Send(getMutualFundByIdQuery);

		return errorOrMutualFundResult.Match(
			mutualFundResult => this.Ok(this.mapper.Map<MutualFundResult, MutualFundResponse>(mutualFundResult)),
			errors => this.Problem(errors));
	}
}
