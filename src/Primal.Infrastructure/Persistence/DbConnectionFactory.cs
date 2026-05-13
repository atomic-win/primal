using System.Data;
using Microsoft.Data.Sqlite;

namespace Primal.Infrastructure.Persistence;

public sealed class DbConnectionFactory
{
	private readonly string connectionString;

	public DbConnectionFactory(string connectionString)
	{
		this.connectionString = connectionString;
	}

	public IDbConnection CreateConnection()
	{
		return new SqliteConnection(this.connectionString);
	}
}
