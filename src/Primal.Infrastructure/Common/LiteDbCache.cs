using ErrorOr;
using LiteDB;
using Newtonsoft.Json;
using Primal.Application.Common.Interfaces;

namespace Primal.Infrastructure.Common;

internal sealed class LiteDbCache : ICache
{
	private readonly LiteDatabase liteDatabase;

	internal LiteDbCache(LiteDatabase liteDatabase)
	{
		this.liteDatabase = liteDatabase;

		var collection = this.liteDatabase.GetCollection<CacheTableEntry>("Cache");
		collection.EnsureIndex(x => x.Id, unique: true);
		collection.EnsureIndex(x => x.ExpirationInTicks);
	}

	public async Task<ErrorOr<T>> GetAsync<T>(string key, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		var collection = this.liteDatabase.GetCollection<CacheTableEntry>("Cache");

		var cacheTableEntry = collection.FindOne(x => x.Id == key);

		if (cacheTableEntry == null)
		{
			return Error.NotFound(description: "Cache entry not found.");
		}

		if (cacheTableEntry.ExpirationInTicks < DateTime.UtcNow.Ticks)
		{
			collection.Delete(key);

			return Error.NotFound(description: "Cache entry expired.");
		}

		return JsonConvert.DeserializeObject<T>(cacheTableEntry.Value).ToErrorOr();
	}

	public async Task<ErrorOr<Success>> SetAsync<T>(string key, T value, TimeSpan relativeExpiration, CancellationToken cancellationToken)
	{
		await Task.CompletedTask;
		var collection = this.liteDatabase.GetCollection<CacheTableEntry>("Cache");

		var cacheTableEntry = new CacheTableEntry
		{
			Id = key,
			Value = JsonConvert.SerializeObject(value),
			ExpirationInTicks = DateTime.UtcNow.Add(relativeExpiration).Ticks,
		};

		collection.Upsert(cacheTableEntry);

		return Result.Success;
	}

	private sealed class CacheTableEntry
	{
		public string Id { get; set; }

		public string Value { get; set; }

		public long ExpirationInTicks { get; set; }
	}
}
