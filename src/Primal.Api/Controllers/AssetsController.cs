using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Primal.Api.Common;
using Primal.Application.Investments;
using Primal.Contracts.Investments;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.Controllers;

[Route("api/investments/[controller]")]
public sealed class AssetsController : ApiController
{
	private readonly IMapper mapper;
	private readonly ISender mediator;
	private readonly IHttpContextAccessor httpContextAccessor;

	public AssetsController(IMapper mapper, ISender mediator, IHttpContextAccessor httpContextAccessor)
	{
		this.mapper = mapper;
		this.mediator = mediator;
		this.httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetAllAssetsAsync()
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getAllAssetsQuery = new GetAllAssetsQuery(userId);

		var errorOrAssets = await this.mediator.Send(getAllAssetsQuery);

		return errorOrAssets.Match(
			assets => this.Ok(this.mapper.Map<IEnumerable<Asset>, IEnumerable<AssetResponse>>(assets)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}")]
	public async Task<IActionResult> GetAssetByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getAssetByIdQuery = new GetAssetByIdQuery(userId, new AssetId(id));

		var errorOrAsset = await this.mediator.Send(getAssetByIdQuery);

		return errorOrAsset.Match(
			asset => this.Ok(this.mapper.Map<Asset, AssetResponse>(asset)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[HttpPost]
	[Route("valuation")]
	public async Task<IActionResult> GetValuationAsync([FromBody] ValuationRequest valuationRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getValuationQuery = this.mapper.Map<(UserId, ValuationRequest), GetValuationQuery>((userId, valuationRequest));

		var errorOrValuationResult = await this.mediator.Send(getValuationQuery);

		return errorOrValuationResult.Match(
			valuationResult => this.Ok(this.mapper.Map<ValuationResult, ValuationResponse>(valuationResult)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}/transactions")]
	public async Task<IActionResult> GetTransactionsAsync(Guid id, [FromQuery] Currency currency)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getTransactionsByAssetIdQuery = new GetTransactionsByAssetIdQuery(
			userId,
			new AssetId(id),
			currency);

		var errorOrTransactionResults = await this.mediator.Send(getTransactionsByAssetIdQuery);

		return errorOrTransactionResults.Match(
			transactionResults => this.Ok(this.mapper.Map<IEnumerable<TransactionResult>, IEnumerable<TransactionResponse>>(transactionResults)),
			errors => this.Problem(errors));
	}

	[HttpGet]
	[Route("{id:guid}/transactions/{transactionId:guid}")]
	public async Task<IActionResult> GetTransactionByIdAsync(Guid id, Guid transactionId, [FromQuery] Currency currency)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var getTransactionByIdQuery = new GetTransactionByIdQuery(
			userId,
			new AssetId(id),
			new TransactionId(transactionId),
			currency);

		var errorOrTransactionResult = await this.mediator.Send(getTransactionByIdQuery);

		return errorOrTransactionResult.Match(
			transactionResult => this.Ok(this.mapper.Map<TransactionResult, TransactionResponse>(transactionResult)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("cash")]
	public async Task<IActionResult> AddCashAssetAsync([FromBody] AddCashAssetRequest addCashAssetRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addCashAssetCommand = this.mapper.Map<AddCashAssetCommand>((userId, addCashAssetRequest));

		var errorOrCashAsset = await this.mediator.Send(addCashAssetCommand);

		return errorOrCashAsset.Match(
			cashAsset => this.Ok(this.mapper.Map<AssetResponse>(cashAsset)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("mutualfunds")]
	public async Task<IActionResult> AddMutualFundAssetAsync([FromBody] AddMutualFundAssetRequest addMutualFundAssetRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addMutualFundAssetCommand = this.mapper.Map<AddMutualFundAssetCommand>((userId, addMutualFundAssetRequest));

		var errorOrMutualFundAsset = await this.mediator.Send(addMutualFundAssetCommand);

		return errorOrMutualFundAsset.Match(
			mutualFundAsset => this.Ok(this.mapper.Map<AssetResponse>(mutualFundAsset)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("stocks")]
	public async Task<IActionResult> AddStockAssetAsync([FromBody] AddStockAssetRequest addStockAssetRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addStockAssetCommand = this.mapper.Map<AddStockAssetCommand>((userId, addStockAssetRequest));

		var errorOrStockAsset = await this.mediator.Send(addStockAssetCommand);

		return errorOrStockAsset.Match(
			stockAsset => this.Ok(this.mapper.Map<AssetResponse>(stockAsset)),
			errors => this.Problem(errors));
	}

	[HttpPost]
	[Route("{id:guid}/transactions")]
	public async Task<IActionResult> AddTransactionAsync(Guid id, [FromBody] TransactionRequest transactionRequest)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var addTransactionCommand = this.mapper.Map<AddTransactionCommand>((userId, new AssetId(id), transactionRequest));

		var errorOrTransactionResult = await this.mediator.Send(addTransactionCommand);

		return errorOrTransactionResult.Match(
			transactionResult => this.Ok(this.mapper.Map<TransactionResult, TransactionResponse>(transactionResult)),
			errors => this.Problem(errors));
	}

	[HttpDelete]
	[Route("{id:guid}")]
	public async Task<IActionResult> DeleteAssetByIdAsync(Guid id)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var deleteAssetCommand = new DeleteAssetCommand(userId, new AssetId(id));

		var errorOrSuccess = await this.mediator.Send(deleteAssetCommand);

		return errorOrSuccess.Match(
			asset => this.Ok(),
			errors => this.Problem(errors));
	}

	[HttpDelete]
	[Route("{id:guid}/transactions/{transactionId:guid}")]
	public async Task<IActionResult> DeleteTransactionByIdAsync(Guid id, Guid transactionId)
	{
		UserId userId = this.httpContextAccessor.HttpContext.GetUserId();

		var deleteTransactionCommand = new DeleteTransactionCommand(
			userId,
			new AssetId(id),
			new TransactionId(transactionId));

		var errorOrSuccess = await this.mediator.Send(deleteTransactionCommand);

		return errorOrSuccess.Match(
			asset => this.Ok(),
			errors => this.Problem(errors));
	}
}
