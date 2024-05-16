using ErrorOr;
using MapsterMapper;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetAllInstrumentsQueryHandler : IRequestHandler<GetAllInstrumentsQuery, ErrorOr<IEnumerable<InstrumentResult>>>
{
	private readonly IMapper mapper;
	private readonly IInstrumentRepository instrumentRepository;

	public GetAllInstrumentsQueryHandler(IMapper mapper, IInstrumentRepository instrumentRepository)
	{
		this.mapper = mapper;
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<IEnumerable<InstrumentResult>>> Handle(GetAllInstrumentsQuery request, CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.instrumentRepository.GetAllAsync(request.UserId, cancellationToken);

		return errorOrInstruments.Match(
			instruments => instruments.Select(instrument => this.mapper.Map<Instrument, InstrumentResult>(instrument)).ToArray(),
			errors => (ErrorOr<IEnumerable<InstrumentResult>>)errors);
	}
}
