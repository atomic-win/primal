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
}
