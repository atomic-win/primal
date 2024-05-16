using ErrorOr;
using MapsterMapper;
using MediatR;
using Primal.Application.Common.Interfaces.Investments;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class AddMutualFundInstrumentCommandHandler : IRequestHandler<AddMutualFundInstrumentCommand, ErrorOr<MutualFundInstrumentResult>>
{
	private readonly IMapper mapper;
	private readonly IMutualFundApiClient mutualFundApiClient;
	private readonly IMutualFundRepository mutualFundRepository;
	private readonly IInstrumentRepository instrumentRepository;

	public AddMutualFundInstrumentCommandHandler(
		IMapper mapper,
		IMutualFundApiClient mutualFundApiClient,
		IMutualFundRepository mutualFundRepository,
		IInstrumentRepository instrumentRepository)
	{
		this.mapper = mapper;
		this.mutualFundApiClient = mutualFundApiClient;
		this.mutualFundRepository = mutualFundRepository;
		this.instrumentRepository = instrumentRepository;
	}

	public async Task<ErrorOr<MutualFundInstrumentResult>> Handle(AddMutualFundInstrumentCommand request, CancellationToken cancellationToken)
	{
		var errorOrMutualFund = await this.GetMutualFundAsync(request.SchemeCode, cancellationToken);

		if (errorOrMutualFund.IsError)
		{
			return errorOrMutualFund.Errors;
		}

		var mutualFund = errorOrMutualFund.Value;

		var errorOrMutualFundInstrument = await this.instrumentRepository.AddMutualFundAsync(request.UserId, request.Name, request.Category, mutualFund.Id, cancellationToken);

		return errorOrMutualFundInstrument.Match(
			mutualFundInstrument => this.mapper.Map<MutualFundInstrumentResult>(mutualFundInstrument),
			errors => (ErrorOr<MutualFundInstrumentResult>)errors);
	}

	private async Task<ErrorOr<MutualFund>> GetMutualFundAsync(int schemeCode, CancellationToken cancellationToken)
	{
		var errorOrMutualFund = await this.mutualFundRepository.GetBySchemeCodeAsync(schemeCode, cancellationToken);

		if (!errorOrMutualFund.IsError)
		{
			return errorOrMutualFund;
		}

		if (errorOrMutualFund.IsError && errorOrMutualFund.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrMutualFund.Errors;
		}

		errorOrMutualFund = await this.mutualFundApiClient.GetBySchemeCodeAsync(schemeCode, cancellationToken);

		if (errorOrMutualFund.IsError)
		{
			return errorOrMutualFund.Errors;
		}

		var mutualFund = errorOrMutualFund.Value;

		return await this.mutualFundRepository.AddAsync(
			mutualFund.SchemeName,
			mutualFund.FundHouse,
			mutualFund.SchemeType,
			mutualFund.SchemeCategory,
			mutualFund.SchemeCode,
			mutualFund.Currency,
			cancellationToken);
	}
}
