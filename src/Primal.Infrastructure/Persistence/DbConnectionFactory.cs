using System.Data;
using Microsoft.Data.Sqlite;

namespace Primal.Infrastructure.Persistence;

internal sealed class DbConnectionFactory
{
	private readonly string connectionString;

	internal DbConnectionFactory(string connectionString)
	{
		this.connectionString = connectionString;
	}

	internal IDbConnection CreateConnection()
	{
		return new SqliteConnection(this.connectionString);
	}
}
