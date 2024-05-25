using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Investments;

namespace Primal.Application.Investments;

internal sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, ErrorOr<Transaction>>
{
	private readonly ITransactionRepository transactionRepository;

	public GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<Transaction>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		return await this.transactionRepository.GetByIdAsync(request.UserId, request.TransactionId, cancellationToken);
	}
}
