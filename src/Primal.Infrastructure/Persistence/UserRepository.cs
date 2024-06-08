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
		try
		{
			UserTableEntity entity = await this.tableClient.GetEntityAsync<UserTableEntity>(
				partitionKey: userId.Value.ToString("N"),
				rowKey: string.Empty,
				cancellationToken: cancellationToken);

			return new User(
				new UserId(Guid.ParseExact(entity.PartitionKey, "N")),
				new MailAddress(entity.Email),
				entity.FirstName,
				entity.LastName,
				entity.FullName,
				new Uri(entity.ProfilePictureUrl));
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			return Error.NotFound();
		}
		catch (Exception ex)
		{
			return Error.Failure(ex.Message);
		}
	}

	public async Task<ErrorOr<Success>> AddUser(User user, CancellationToken cancellationToken)
	{
		var tableEntity = new UserTableEntity
		{
			PartitionKey = user.Id.Value.ToString("N"),
			Email = user.Email.Address,
			FirstName = user.FirstName,
			LastName = user.LastName,
			FullName = user.FullName,
			ProfilePictureUrl = user.ProfilePictureUrl.ToString(),
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

		public string RowKey { get; set; } = string.Empty;

		public DateTimeOffset? Timestamp { get; set; }

		public ETag ETag { get; set; }

		public string Email { get; set; } = default!;

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string FullName { get; set; } = string.Empty;

		public string ProfilePictureUrl { get; set; } = string.Empty;
	}
}
