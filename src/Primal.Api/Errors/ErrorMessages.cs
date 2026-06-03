namespace Primal.Api.Errors;

internal static class ErrorMessages
{
	internal static class AssetItem
	{
		internal const string IdRequired = "Asset item ID must be provided";
		internal const string IdsRequired = "At least one asset item ID must be provided";
		internal const string NotFound = "Asset item not found";
		internal const string AssetTypeUnknown = "Asset type cannot be Unknown";
		internal const string AssetClassInvalid = "Asset class '{0}' is not valid for MutualFund asset type";
		internal const string NameRequired = "Name cannot be empty";
		internal const string NameTooShort = "Name must be at least 3 characters long";
		internal const string NameTooLong = "Name must not exceed 50 characters";
		internal const string MutualFundNotFound = "Mutual fund not found";
		internal const string StockNotFound = "Stock not found";
		internal const string CurrencyRequired = "Currency must be provided";
	}

	internal static class Transaction
	{
		internal const string IdRequired = "Transaction ID must be provided";
		internal const string NotFound = "Transaction not found";
		internal const string TypeRequired = "Transaction type must be provided";
		internal const string DateRequired = "Transaction date must be provided";
		internal const string DateInFuture = "Transaction date cannot be in the future";
		internal const string NameRequired = "Transaction name must be provided";
		internal const string NameTooShort = "Transaction name must be at least 3 characters long";
		internal const string NameTooLong = "Transaction name must not exceed 1000 characters";
		internal const string UnitsRequired = "Transaction units must be greater than zero";
		internal const string UnitsInvalid = "Transaction units must be greater than or equal to zero";
		internal const string PriceRequired = "Transaction price must be greater than zero";
		internal const string PriceInvalid = "Transaction price must be greater than or equal to zero";
		internal const string AmountRequired = "Transaction amount must be greater than zero";
		internal const string AmountInvalid = "Transaction amount must be greater than or equal to zero";
	}

	internal static class User
	{
		internal const string IdRequired = "User ID must be provided";
		internal const string NotFound = "User not found";
		internal const string UpdateFieldsRequired = "At least one field of preferred currency or preferred locale must be provided";
	}

	internal static class Auth
	{
		internal const string IdTokenRequired = "ID token must be provided";
		internal const string IdTokenExpired = "ID token has expired";
		internal const string IdTokenInvalid = "ID token is invalid";
		internal const string UnexpectedError = "An unexpected error occurred";
	}
}
