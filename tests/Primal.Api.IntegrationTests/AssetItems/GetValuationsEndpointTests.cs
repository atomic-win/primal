using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Primal.Api.AssetItems;
using Primal.Domain.Investments;
using Primal.Domain.Money;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.AssetItems;

public sealed class GetValuationsEndpointTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() },
	};

	[Test]
	public async Task GetValuations_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.GetAsync("/api/asset-items/valuations?currency=USD&assetItemIds=00000000-0000-0000-0000-000000000001");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task GetValuations_MissingCurrency_Returns400()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/valuations?assetItemIds={assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
	}

	[Test]
	public async Task GetValuations_MissingAssetItemIds_Returns400()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync("/api/asset-items/valuations?currency=USD");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
	}

	[Test]
	public async Task GetValuations_AssetItemNotFound_Returns400()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(AssetItem.Empty);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/valuations?currency=USD&assetItemIds={assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
	}

	[Test]
	public async Task GetValuations_ValidRequest_ReturnsValuations()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetId = new AssetId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, assetId, "My Bank");
		var asset = new Asset(assetId, "Bank", AssetClass.EmergencyFund, AssetType.BankAccount, Currency.INR, "default-EmergencyFund-BankAccount-INR");

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		factory.AssetRepository
			.GetByIdAsync(assetId, Arg.Any<CancellationToken>())
			.Returns(asset);

		factory.TransactionRepository
			.GetByAssetItemIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(Enumerable.Empty<Transaction>());

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.GetAsync($"/api/asset-items/valuations?currency=INR&assetItemIds={assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

		var valuations = await response.Content.ReadFromJsonAsync<List<ValuationResponse>>(JsonOptions);
		await Assert.That(valuations).IsNotNull();
		await Assert.That(valuations!.Count).IsGreaterThanOrEqualTo(1);
		await Assert.That(valuations[0].InvestedValue).IsEqualTo(0m);
		await Assert.That(valuations[0].CurrentValue).IsEqualTo(0m);
	}
}
