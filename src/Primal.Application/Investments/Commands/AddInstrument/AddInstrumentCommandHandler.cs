using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class AddInstrumentCommandHandler : IRequestHandler<AddInstrumentCommand, ErrorOr<InstrumentResult>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public AddInstrumentCommandHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<InstrumentResult>> Handle(AddInstrumentCommand request, CancellationToken cancellationToken)
	{
		var errorOrInstrument = await this.instrumentRepository.AddAsync(request.UserId, request.Name, request.Category, request.Type, cancellationToken);

		return errorOrInstrument.Match(
			instrument => new InstrumentResult(instrument.Id, instrument.Name, instrument.Category, instrument.Type),
			errors => (ErrorOr<InstrumentResult>)errors);
	}
}
