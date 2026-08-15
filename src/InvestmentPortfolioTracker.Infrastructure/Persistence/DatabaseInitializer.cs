using Dapper;

namespace InvestmentPortfolioTracker.Infrastructure.Persistence;

internal static class DatabaseInitializer
{
	internal static void Initialize(DbConnectionFactory connectionFactory)
	{
		using var connection = connectionFactory.CreateConnection();
		connection.Open();

		CreateUserTables(connection);
		CreateAssetTables(connection);
		CreateTransactionTable(connection);
		CreateRateTable(connection);
	}

	private static void CreateUserTables(System.Data.IDbConnection connection)
	{
		connection.Execute("""
			CREATE TABLE IF NOT EXISTS users (
				Id TEXT NOT NULL PRIMARY KEY,
				Email TEXT NOT NULL,
				FirstName TEXT NOT NULL,
				LastName TEXT NOT NULL,
				FullName TEXT NOT NULL,
				PreferredCurrency TEXT NOT NULL DEFAULT 'USD',
				PreferredLocale TEXT NOT NULL DEFAULT 'EN_US',
				CreatedAt TEXT NOT NULL,
				UpdatedAt TEXT NOT NULL
			);

			CREATE UNIQUE INDEX IF NOT EXISTS IX_users_Email ON users (Email);

			CREATE TABLE IF NOT EXISTS user_ids (
				Id TEXT NOT NULL,
				IdentityProvider TEXT NOT NULL,
				UserId TEXT NOT NULL,
				CreatedAt TEXT NOT NULL,
				UpdatedAt TEXT NOT NULL,
				PRIMARY KEY (Id, IdentityProvider),
				FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
			);

			CREATE INDEX IF NOT EXISTS IX_user_ids_UserId ON user_ids (UserId);
			""");
	}

	private static void CreateAssetTables(System.Data.IDbConnection connection)
	{
		connection.Execute("""
			CREATE TABLE IF NOT EXISTS assets (
				Id TEXT NOT NULL PRIMARY KEY,
				Name TEXT NOT NULL,
				AssetClass TEXT NOT NULL,
				AssetType TEXT NOT NULL,
				Currency TEXT NOT NULL,
				ExternalId TEXT NOT NULL,
				CreatedAt TEXT NOT NULL,
				UpdatedAt TEXT NOT NULL
			);

			CREATE UNIQUE INDEX IF NOT EXISTS IX_assets_ExternalId ON assets (ExternalId);

			CREATE TABLE IF NOT EXISTS asset_items (
				Id TEXT NOT NULL PRIMARY KEY,
				Name TEXT NOT NULL,
				UserId TEXT NOT NULL,
				AssetId TEXT NOT NULL,
				CreatedAt TEXT NOT NULL,
				UpdatedAt TEXT NOT NULL,
				FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
				FOREIGN KEY (AssetId) REFERENCES assets(Id) ON DELETE RESTRICT
			);

			CREATE INDEX IF NOT EXISTS IX_asset_items_UserId ON asset_items (UserId);

			CREATE INDEX IF NOT EXISTS IX_asset_items_AssetId ON asset_items (AssetId);
			""");
	}

	private static void CreateTransactionTable(System.Data.IDbConnection connection)
	{
		connection.Execute("""
			CREATE TABLE IF NOT EXISTS transactions (
				Id TEXT NOT NULL PRIMARY KEY,
				Date TEXT NOT NULL,
				Name TEXT NOT NULL,
				TransactionType TEXT NOT NULL,
				AssetItemId TEXT NOT NULL,
				UserId TEXT NOT NULL,
				Units TEXT NOT NULL,
				Price TEXT NOT NULL,
				Amount TEXT NOT NULL,
				CreatedAt TEXT NOT NULL,
				UpdatedAt TEXT NOT NULL,
				FOREIGN KEY (AssetItemId) REFERENCES asset_items(Id) ON DELETE CASCADE,
				FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE
			);

			CREATE INDEX IF NOT EXISTS IX_transactions_UserId_AssetItemId ON transactions (UserId, AssetItemId);
			""");
	}

	private static void CreateRateTable(System.Data.IDbConnection connection)
	{
		connection.Execute("""
			CREATE TABLE IF NOT EXISTS rates (
				Symbol TEXT NOT NULL,
				RateType TEXT NOT NULL,
				Date TEXT NOT NULL,
				Price TEXT NOT NULL,
				CreatedAt TEXT NOT NULL,
				PRIMARY KEY (Symbol, RateType, Date)
			);
			""");
	}
}
