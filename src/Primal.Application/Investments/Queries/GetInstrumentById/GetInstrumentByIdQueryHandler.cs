using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentByIdQueryHandler : IRequestHandler<GetInstrumentByIdQuery, ErrorOr<InstrumentResult>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentByIdQueryHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<InstrumentResult>> Handle(GetInstrumentByIdQuery request, CancellationToken cancellationToken)
	{
		var errorOrInstrument = await this.instrumentRepository.GetByIdAsync(request.UserId, request.InstrumentId, cancellationToken);

		return errorOrInstrument.Match(
			instrument => new InstrumentResult(instrument.Id, instrument.Name, instrument.Category, instrument.Type),
			errors => (ErrorOr<InstrumentResult>)errors);
	}
}
