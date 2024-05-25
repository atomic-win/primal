using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetAllTransactionsQueryHandler : IRequestHandler<GetAllTransactionsQuery, ErrorOr<IEnumerable<Transaction>>>
{
	private readonly ITransactionRepository transactionRepository;

	public GetAllTransactionsQueryHandler(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<IEnumerable<Transaction>>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await this.transactionRepository.GetAllAsync(request.UserId, cancellationToken);
	}
}
