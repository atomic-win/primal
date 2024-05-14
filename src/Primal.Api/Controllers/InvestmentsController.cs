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
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public InvestmentsController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
	{
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
			instrumentsResult => this.Ok(instrumentsResult.Select(instrumentResult => new InstrumentResponse(instrumentResult.Id.Value, instrumentResult.Name, instrumentResult.Category.ToString(), instrumentResult.Type.ToString()))),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("instruments")]
	public async Task<IActionResult> AddInstrumentAsync([FromBody] AddInstrumentRequest addInstrumentRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addInstrumentCommand = new AddInstrumentCommand(
			userId,
			addInstrumentRequest.Name,
			Enum.Parse<InvestmentCategory>(addInstrumentRequest.Category),
			Enum.Parse<InvestmentType>(addInstrumentRequest.Type));

		var errorOrInstrumentResult = await this.mediator.Send(addInstrumentCommand);

		return errorOrInstrumentResult.Match(
			instrumentResult => this.Created(
				$"{this.httpContextAccessor.HttpContext.Request.Scheme}://{this.httpContextAccessor.HttpContext.Request.Host}{this.httpContextAccessor.HttpContext.Request.Path}/{instrumentResult.Id.Value}",
				new InstrumentResponse(instrumentResult.Id.Value, instrumentResult.Name, instrumentResult.Category.ToString(), instrumentResult.Type.ToString())),
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
			instrumentResult => this.Ok(new InstrumentResponse(instrumentResult.Id.Value, instrumentResult.Name, instrumentResult.Category.ToString(), instrumentResult.Type.ToString())),
			errors => this.Problem(errors));
	}
}
