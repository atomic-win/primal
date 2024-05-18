using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;

namespace Primal.Api.Controllers;

[Route("api/investments/[controller]")]
public sealed class InstrumentsController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public InstrumentsController(IMapper mapper, ISender mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mapper = mapper;
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
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
}
