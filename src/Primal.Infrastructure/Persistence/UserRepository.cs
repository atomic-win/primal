using System.Net.Mail;
using Azure;
using Azure.Data.Tables;
using ErrorOr;
using Primal.Application.Common.Interfaces.Persistence;
using Primal.Domain.Users;

namespace Primal.Infrastructure.Persistence;

internal sealed class UserRepository : IUserRepository
{
	private readonly TableClient tableClient;

	internal UserRepository(TableClient tableClient)
	{
		this.tableClient = tableClient;
	}

	public async Task<ErrorOr<User>> GetUser(UserId userId, CancellationToken cancellationToken)
	{
		AsyncPageable<UserTableEntity> entities = this.tableClient.QueryAsync<UserTableEntity>(
			entity => entity.PartitionKey == userId.Value.ToString("N")
				&& entity.RowKey == "User",
			cancellationToken: cancellationToken);

		await foreach (UserTableEntity entity in entities.WithCancellation(cancellationToken))
		{
			return new User(new UserId(Guid.ParseExact(entity.PartitionKey, "N")), new MailAddress(entity.Email));
		}

		return Error.NotFound();
	}

	public async Task<ErrorOr<Success>> AddUser(User user, CancellationToken cancellationToken)
	{
		var tableEntity = new UserTableEntity
		{
			PartitionKey = user.Id.Value.ToString("N"),
			Email = user.Email.Address,
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

	private sealed class UserTableEntity : ITableEntity
	{
		public string PartitionKey { get; set; } = default!;

		public string RowKey { get; set; } = "User";

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Email { get; set; } = default!;
	}
}
