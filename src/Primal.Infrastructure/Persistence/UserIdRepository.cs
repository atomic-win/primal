using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserIdRepository : IUserIdRepository
{
	private readonly TableClient tableClient;

	internal UserIdRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<UserId>> GetUserId(IdentityProviderUser identityProviderUser, CancellationToken cancellationToken)
	{
		AsyncPageable<UserIdTableEntity> entities = this.tableClient.QueryAsync<UserIdTableEntity>(
			entity => entity.PartitionKey == identityProviderUser.Id.Value
				&& entity.IdentityProvider == identityProviderUser.IdentityProvider.ToString(),
			cancellationToken: cancellationToken);

		await foreach (UserIdTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			return new UserId(entity.UserId);
		}

		return Error.NotFound();
	}

	public async Task<ErrorOr<Success>> AddUserId(IdentityProviderUser identityProviderUser, UserId userId, CancellationToken cancellationToken)
	{
		var tableEntity = new UserIdTableEntity
		{
			PartitionKey = identityProviderUser.Id.Value,
			IdentityProvider = identityProviderUser.IdentityProvider.ToString(),
			UserId = userId.Value,
		};

		try
		{
			await this.tableClient.AddEntityAsync(tableEntity, cancellationToken: cancellationToken);
			return Result.Success;
		}
		catch (RequestFailedException ex) when (ex.Status == 409)
		{
			return Error.Conflict();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	private sealed class UserIdTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = default!;

		public string RowKey { get; set; } = string.Empty;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string IdentityProvider { get; set; }

		public Guid UserId { get; set; }
	}
}
