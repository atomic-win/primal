using ErrorOr;
using MapsterMapper;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentByIdQueryHandler : IRequestHandler<GetInstrumentByIdQuery, ErrorOr<InstrumentResult>>
{
	private readonly IMapper mapper;
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentByIdQueryHandler(IMapper mapper, IInstrumentRepository instrumentRepository)
	{
		this.mapper = mapper;
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<InstrumentResult>> Handle(GetInstrumentByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(request.UserId, request.InstrumentId, cancellationToken);

		return errorOrInstrument.Match(
			instrument => this.mapper.Map<Instrument, InstrumentResult>(instrument),
			errors => (ErrorOr<InstrumentResult>)errors);
	}
}
