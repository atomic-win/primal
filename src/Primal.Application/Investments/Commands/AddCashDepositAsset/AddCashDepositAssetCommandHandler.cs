using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;
using Primal.Domain.Money;

namespace Primal.Application.Investments;

internal sealed class AddCashDepositAssetCommandHandler : IRequestHandler<AddCashDepositAssetCommand, ErrorOr<Asset>>
{
	private readonly IInstrumentRepository instrumentRepository;
	private readonly IAssetRepository assetRepository;

	public AddCashDepositAssetCommandHandler(IInstrumentRepository instrumentRepository, IAssetRepository assetRepository)
	{
		this.instrumentRepository = instrumentRepository;
		this.assetRepository = assetRepository;
	}

	public async Task<ErrorOr<Asset>> Handle(AddCashDepositAssetCommand request, CancellationToken cancellationToken)
	{
		var errorOrCashDeposit = await this.GetCashDepositAsync(
			request.Currency,
			cancellationToken);

		if (errorOrCashDeposit.IsError)
		{
			return errorOrCashDeposit.Errors;
		}

		var cashDeposit = errorOrCashDeposit.Value;

		return await this.assetRepository.AddAsync(
			request.UserId,
			request.Name,
			cashDeposit.Id,
			cancellationToken);
	}

	private async Task<ErrorOr<InvestmentInstrument>> GetCashDepositAsync(
		Currency currency,
		CancellationToken cancellationToken)
	{
		var errorOrInvestmentInstrument = await this.instrumentRepository.GetCashDepositAsync(
			currency,
			cancellationToken);

		if (!errorOrInvestmentInstrument.IsError)
		{
			return errorOrInvestmentInstrument;
		}

		if (errorOrInvestmentInstrument.IsError && errorOrInvestmentInstrument.FirstError is not { Type: ErrorType.NotFound })
		{
			return errorOrInvestmentInstrument.Errors;
		}

		return await this.instrumentRepository.AddCashDepositAsync(
			currency,
			cancellationToken);
	}
}
