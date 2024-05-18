using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetInstrumentByIdQueryHandler : IRequestHandler<GetInstrumentByIdQuery, ErrorOr<InvestmentInstrument>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public GetInstrumentByIdQueryHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<InvestmentInstrument>> Handle(GetInstrumentByIdQuery request, CancellationToken cancellationToken)
	{
		return await this.instrumentRepository.GetByIdAsync(request.InstrumentId, cancellationToken);
	}
}
