namespace Primal.Api.Errors;

internal static class ErrorCodes
{
	internal static class AssetItem
	{
		internal const string IdRequired = "ASSET_ITEM_ID_REQUIRED";
		internal const string IdsRequired = "ASSET_ITEM_IDS_REQUIRED";
		internal const string NotFound = "ASSET_ITEM_NOT_FOUND";
		internal const string AssetTypeUnknown = "ASSET_TYPE_UNKNOWN";
		internal const string AssetTypeETFNotAllowed = "ASSET_TYPE_ETF_NOT_ALLOWED";
		internal const string AssetClassRequired = "ASSET_CLASS_REQUIRED";
		internal const string AssetClassNotAllowed = "ASSET_CLASS_NOT_ALLOWED";
		internal const string AssetClassInvalid = "ASSET_CLASS_INVALID";
		internal const string ExternalIdRequired = "EXTERNAL_ID_REQUIRED";
		internal const string ExternalIdNotAllowed = "EXTERNAL_ID_NOT_ALLOWED";
		internal const string CurrencyRequired = "CURRENCY_REQUIRED";
		internal const string CurrencyNotAllowed = "CURRENCY_NOT_ALLOWED";
		internal const string NameRequired = "NAME_REQUIRED";
		internal const string NameTooShort = "NAME_TOO_SHORT";
		internal const string NameTooLong = "NAME_TOO_LONG";
		internal const string MutualFundNotFound = "MUTUAL_FUND_NOT_FOUND";
		internal const string StockNotFound = "STOCK_NOT_FOUND";
	}

	internal static class Transaction
	{
		internal const string IdRequired = "TRANSACTION_ID_REQUIRED";
		internal const string NotFound = "TRANSACTION_NOT_FOUND";
		internal const string TypeRequired = "TRANSACTION_TYPE_REQUIRED";
		internal const string TypeInvalid = "TRANSACTION_TYPE_INVALID";
		internal const string DateRequired = "DATE_REQUIRED";
		internal const string DateInFuture = "DATE_IN_FUTURE";
		internal const string NameRequired = "NAME_REQUIRED";
		internal const string NameTooShort = "NAME_TOO_SHORT";
		internal const string NameTooLong = "NAME_TOO_LONG";
		internal const string UnitsRequired = "UNITS_REQUIRED";
		internal const string UnitsInvalid = "UNITS_INVALID";
		internal const string PriceRequired = "PRICE_REQUIRED";
		internal const string PriceInvalid = "PRICE_INVALID";
		internal const string AmountRequired = "AMOUNT_REQUIRED";
		internal const string AmountInvalid = "AMOUNT_INVALID";
	}

	internal static class User
	{
		internal const string IdRequired = "USER_ID_REQUIRED";
		internal const string NotFound = "USER_NOT_FOUND";
		internal const string UpdateFieldsRequired = "UPDATE_FIELDS_REQUIRED";
	}

	internal static class Auth
	{
		internal const string IdTokenRequired = "ID_TOKEN_REQUIRED";
		internal const string IdTokenExpired = "ID_TOKEN_EXPIRED";
		internal const string IdTokenInvalid = "ID_TOKEN_INVALID";
		internal const string UnexpectedError = "UNEXPECTED_ERROR";
	}
}
