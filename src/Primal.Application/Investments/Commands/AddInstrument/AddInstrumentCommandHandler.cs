using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddInstrumentCommandHandler : IRequestHandler<AddInstrumentCommand, ErrorOr<InstrumentResult>>
{
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IMutualFundRepository mutualFundRepository;

	public AddInstrumentCommandHandler(IInstrumentRepository instrumentRepository, IMutualFundRepository mutualFundRepository)
	{
		this.instrumentRepository = instrumentRepository;
		this.mutualFundRepository = mutualFundRepository;
	}

	public async Task<ErrorOr<InstrumentResult>> Handle(AddInstrumentCommand request, CancellationToken cancellationToken)
	{
		if (request.Type == InvestmentType.MutualFunds)
		{
			if (!Guid.TryParse(request.Name, out Guid guid))
			{
				return Error.Validation();
			}

			var errorOrMutualFund = await this.mutualFundRepository.GetByIdAsync(new MutualFundId(guid), cancellationToken);

			if (errorOrMutualFund.IsError)
			{
				return errorOrMutualFund.FirstError switch
				{
					{ Type: ErrorType.NotFound } => Error.NotFound(),
					_ => errorOrMutualFund.Errors,
				};
			}
		}

		var errorOrInstrument = await this.instrumentRepository.AddAsync(request.UserId, request.Name, request.Category, request.Type, cancellationToken);

		return errorOrInstrument.Match(
			instrument => new InstrumentResult(instrument.Id, instrument.Name, instrument.Category, instrument.Type),
			errors => (ErrorOr<InstrumentResult>)errors);
	}
}
