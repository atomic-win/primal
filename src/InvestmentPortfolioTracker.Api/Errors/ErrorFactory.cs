using FluentValidation.Results;

namespace InvestmentPortfolioTracker.Api.Errors;

internal static class ErrorFactory
{
	internal static ValidationFailure AssetItemNotFound(string propertyName = "id")
		=> new(propertyName, ErrorMessages.AssetItem.NotFound) { ErrorCode = ErrorCodes.AssetItem.NotFound };

	internal static ValidationFailure MutualFundNotFound()
		=> new("externalId", ErrorMessages.AssetItem.MutualFundNotFound) { ErrorCode = ErrorCodes.AssetItem.MutualFundNotFound };

	internal static ValidationFailure StockNotFound()
		=> new("externalId", ErrorMessages.AssetItem.StockNotFound) { ErrorCode = ErrorCodes.AssetItem.StockNotFound };

	internal static ValidationFailure TransactionNotFound()
		=> new("transactionId", ErrorMessages.Transaction.NotFound) { ErrorCode = ErrorCodes.Transaction.NotFound };

	internal static ValidationFailure UserNotFound()
		=> new("userId", ErrorMessages.User.NotFound) { ErrorCode = ErrorCodes.User.NotFound };
}
