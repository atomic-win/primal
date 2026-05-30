using System.Collections.Concurrent;
using System.Data;
using Primal.Infrastructure.Persistence;

namespace Primal.Infrastructure.IntegrationTests;

internal static class TestDbHelper
{
	private static readonly ConcurrentBag<IDbConnection> KeepAliveConnections = [];

	internal static DbConnectionFactory CreateTestDatabase()
	{
		var connectionFactory = new DbConnectionFactory($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared;Foreign Keys=False");
		var keepAliveConnection = connectionFactory.CreateConnection();
		keepAliveConnection.Open();
		KeepAliveConnections.Add(keepAliveConnection);
		DatabaseInitializer.Initialize(connectionFactory);
		return connectionFactory;
	}
}
