using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class GetAllInstrumentsQueryHandler : IRequestHandler<GetAllInstrumentsQuery, ErrorOr<IEnumerable<InstrumentResult>>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public GetAllInstrumentsQueryHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<IEnumerable<InstrumentResult>>> Handle(GetAllInstrumentsQuery request, CancellationToken cancellationToken)
	{
		var errorOrInstruments = await this.instrumentRepository.GetAllAsync(request.UserId, cancellationToken);

		return errorOrInstruments.Match(
			instruments => instruments.Select(instrument => new InstrumentResult(instrument.Id, instrument.Name, instrument.Category, instrument.Type, instrument.AccountId)).ToArray(),
			errors => (ErrorOr<IEnumerable<InstrumentResult>>)errors);
	}
}
