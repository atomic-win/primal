using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

[Route("api/investments/[controller]")]
public sealed class TransactionsController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public TransactionsController(IMapper mapper, ISender mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mapper = mapper;
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetAllTransactionsAsync()
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getAllTransactionsQuery = new GetAllTransactionsQuery(userId);

		var errorOrTransactions = await this.mediator.Send(getAllTransactionsQuery);

		return errorOrTransactions.Match(
			transactions => this.Ok(this.mapper.Map<IEnumerable<Transaction>, IEnumerable<TransactionResponse>>(transactions)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}")]
	public async Task<IActionResult> GetTransactionByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getTransactionByIdQuery = new GetTransactionByIdQuery(userId, new TransactionId(id));

		var errorOrTransaction = await this.mediator.Send(getTransactionByIdQuery);

		return errorOrTransaction.Match(
			transaction => this.Ok(this.mapper.Map<Transaction, TransactionResponse>(transaction)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("")]
	public async Task<IActionResult> AddTransactionAsync([FromBody] Transaction transactionRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addTransactionCommand = this.mapper.Map<AddTransactionCommand>((userId, transactionRequest));

		var errorOrTransaction = await this.mediator.Send(addTransactionCommand);

		return errorOrTransaction.Match(
			transaction => this.Ok(this.mapper.Map<Transaction, TransactionResponse>(transaction)),
			errors => this.Problem(errors));
	}

	[HttpDelete]
	[Route("{id:guid}")]
	public async Task<IActionResult> DeleteTransactionByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var deleteTransactionCommand = new DeleteTransactionCommand(userId, new TransactionId(id));

		var errorOrSuccess = await this.mediator.Send(deleteTransactionCommand);

		return errorOrSuccess.Match(
			asset => this.Ok(),
			errors => this.Problem(errors));
	}
}
