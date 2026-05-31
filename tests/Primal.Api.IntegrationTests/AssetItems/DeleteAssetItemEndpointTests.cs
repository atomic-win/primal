using System.Net;
using System.Net.Http.Headers;
using NSubstitute;
using Primal.Domain.Investments;
using Primal.Domain.Users;

namespace Primal.Api.IntegrationTests.AssetItems;

public sealed class DeleteAssetItemEndpointTests
{
	[Test]
	public async Task DeleteAssetItem_Unauthenticated_Returns401()
	{
		await using var factory = new PrimalApiFactory();
		var client = factory.CreateClient();

		var response = await client.DeleteAsync($"/api/asset-items/{Guid.NewGuid()}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task DeleteAssetItem_NotFound_Returns404()
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

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task DeleteAssetItem_Exists_Returns204AndDeletes()
	{
		await using var factory = new PrimalApiFactory();
		var userId = new UserId(Guid.NewGuid());
		var assetItemId = new AssetItemId(Guid.NewGuid());
		var assetItem = new AssetItem(assetItemId, new AssetId(Guid.NewGuid()), "My Bank");

		factory.AssetItemRepository
			.GetByIdAsync(userId, assetItemId, Arg.Any<CancellationToken>())
			.Returns(assetItem);

		var client = factory.CreateClient();
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));

		var response = await client.DeleteAsync($"/api/asset-items/{assetItemId.Value}");

		await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
		await factory.AssetItemRepository.Received(1).DeleteAsync(userId, assetItemId, Arg.Any<CancellationToken>());
	}
}
