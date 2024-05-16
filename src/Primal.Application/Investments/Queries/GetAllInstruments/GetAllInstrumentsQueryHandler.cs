using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetAllInstrumentsQueryHandler : IRequestHandler<GetAllInstrumentsQuery, ErrorOr<IEnumerable<Instrument>>>
{
	private readonly IInstrumentRepository instrumentRepository;

	public GetAllInstrumentsQueryHandler(IInstrumentRepository instrumentRepository)
	{
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<IEnumerable<Instrument>>> Handle(GetAllInstrumentsQuery request, CancellationToken cancellationToken)
	{
		return await this.instrumentRepository.GetAllAsync(request.UserId, cancellationToken);
	}
}
