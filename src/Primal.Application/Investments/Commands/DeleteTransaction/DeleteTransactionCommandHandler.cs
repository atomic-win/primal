using ErrorOr;
using MediatR;
using Primal.Application.Common.Interfaces.Persistence;

namespace Primal.Application.Investments;

internal sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, ErrorOr<Success>>
{
	private readonly ITransactionRepository transactionRepository;

	public DeleteTransactionCommandHandler(ITransactionRepository transactionRepository)
	{
		this.transactionRepository = transactionRepository;
	}

	public async Task<ErrorOr<Success>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
	{
		return await this.transactionRepository.DeleteAsync(
			request.UserId,
			request.AssetId,
			request.TransactionId,
			cancellationToken);
	}
}
