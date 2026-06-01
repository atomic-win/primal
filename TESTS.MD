# Test Scenarios

Comprehensive listing of every scenario, input, and parameter combination required for full path coverage of the Primal codebase.

---

## 1. Domain Layer

### 1.1 Entity\<TId\> Equality

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Two entities with the same Id compared via `Equals(Entity<TId>)` | `true` |
| 2 | Two entities with different Ids compared via `Equals(Entity<TId>)` | `false` |
| 3 | `Equals(Entity<TId>)` with `null` | `false` |
| 4 | `Equals(object)` with a matching entity | `true` |
| 5 | `Equals(object)` with a non-entity object | `false` |
| 6 | `Equals(object)` with `null` | `false` |
| 7 | `==` operator: both non-null, same Id | `true` |
| 8 | `==` operator: both non-null, different Id | `false` |
| 9 | `==` operator: left is `null`, right is `null` | `true` |
| 10 | `==` operator: left is `null`, right is non-null | `false` |
| 11 | `==` operator: left is non-null, right is `null` | `false` |
| 12 | `!=` operator: same Id | `false` |
| 13 | `!=` operator: different Id | `true` |
| 14 | `GetHashCode()` returns same value for entities with same Id | Equal hash codes |
| 15 | `GetHashCode()` returns different value for entities with different Ids | Different hash codes (likely) |

### 1.2 Domain Model `Empty` Sentinel Values

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `User.Empty` has `UserId.Empty`, empty strings, `Currency.Unknown`, `Locale.Unknown` | All fields match sentinel values |
| 2 | `Transaction.Empty` has `TransactionId.Empty`, `DateOnly.MinValue`, empty name, `Buy`, `AssetItemId.Empty`, all zeros | All fields match sentinel values |
| 3 | `AssetItem.Empty` has `AssetItemId.Empty`, `AssetId.Empty`, empty name | All fields match |
| 4 | `Asset.Empty` has `AssetId.Empty`, empty strings, `Unknown` enums | All fields match |

### 1.3 Enum Coverage

Every enum value must be exercised at least once across all tests:

- **Currency**: `Unknown`, `USD`, `INR`
- **Locale**: `Unknown`, `EN_US`, `EN_IN`
- **IdentityProvider**: `Google`
- **AssetClass**: `Unknown`, `Equity`, `Debt`, `Commodities`, `EmergencyFund`
- **AssetType**: `Unknown`, `BankAccount`, `Wallet`, `TradingAccount`, `FixedDeposit`, `EPF`, `PPF`, `MutualFund`, `Stock`, `Bond`
- **TransactionType**: `Unknown`, `Buy`, `Sell`, `Deposit`, `Withdrawal`, `Dividend`, `Interest`, `SelfInterest`, `InterestPenalty`

---

## 2. Application Layer

### 2.1 TransactionAmountCalculator.CalculateAmountAsync

#### Branch: Transaction type is NOT Buy/Sell (returns `transaction.Amount` directly)

| # | TransactionType | date vs transaction.Date | Expected |
|---|-----------------|--------------------------|----------|
| 1 | `Deposit` | same date | `exchangeRate × transaction.Amount` |
| 2 | `Withdrawal` | different date | `exchangeRate × transaction.Amount` |
| 3 | `Interest` | any | `exchangeRate × transaction.Amount` |
| 4 | `SelfInterest` | any | `exchangeRate × transaction.Amount` |
| 5 | `Dividend` | any | `exchangeRate × transaction.Amount` |
| 6 | `InterestPenalty` | any | `exchangeRate × transaction.Amount` |

#### Branch: Transaction type IS Buy/Sell, date == transaction.Date

| # | TransactionType | Expected |
|---|-----------------|----------|
| 7 | `Buy`, same date | `exchangeRate × (units × price)` |
| 8 | `Sell`, same date | `exchangeRate × (units × price)` |

#### Branch: Transaction type IS Buy/Sell, date != transaction.Date → GetAssetRateAsync

| # | AssetType | Expected |
|---|-----------|----------|
| 9 | `MutualFund` | `exchangeRate × (units × mutualFundApiClient.GetOnOrBeforePriceAsync(symbol, date))` |
| 10 | `Stock` | `exchangeRate × (units × stockApiClient.GetOnOrBeforePriceAsync(symbol, date))` |
| 11 | `BankAccount` (not MF/Stock) | `exchangeRate × (units × 1m)` |
| 12 | `FixedDeposit` (not MF/Stock) | `exchangeRate × (units × 1m)` |

#### Branch: Exchange rate for same vs different currency

| # | Scenario | Expected |
|---|----------|----------|
| 13 | Asset currency == target currency | Exchange rate = 1 (from CachedExchangeRateApiClient) |
| 14 | Asset currency != target currency (e.g., INR → USD) | Exchange rate fetched from API |

#### ExternalId parsing

| # | Scenario | Expected |
|---|----------|----------|
| 15 | ExternalId = `"mf-123456"` for MutualFund | Symbol extracted as `"123456"` (split on `-`, index 1) |
| 16 | ExternalId = `"stock-aapl"` for Stock | Symbol extracted as `"aapl"` |

---

## 3. API Layer — Auth

### 3.1 POST /api/auth/login/google — GoogleLoginEndpoint

#### Validation (GoogleLoginValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `IdToken` = `""` (empty) | 400 — `ID_TOKEN_REQUIRED` |
| 2 | `IdToken` = `null` | 400 — `ID_TOKEN_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 3 | Valid Google ID token, user already exists (`GetUserId` returns non-empty) | 200 — TokenResponse with access/refresh tokens |
| 4 | Valid Google ID token, new user (`GetUserId` returns `UserId.Empty`) | Creates userId via `AddUserId`, creates user via `AddUserAsync`, returns 200 — TokenResponse |
| 5 | Expired Google ID token (`InvalidJwtException` with message "JWT has expired.") | 401 — `ID_TOKEN_EXPIRED` |
| 6 | Invalid Google ID token (other `InvalidJwtException`) | 401 — `ID_TOKEN_INVALID` |
| 7 | Unexpected exception during processing | 500 — `UNEXPECTED_ERROR` |

### 3.2 POST /api/auth/refresh-token — MyTokenService

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Valid refresh token | 200 — new TokenResponse |
| 2 | Invalid/expired refresh token | 401 |

---

## 4. API Layer — Users

### 4.1 GET /api/users/me — GetUserEndpoint

#### Validation (GetUserValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `UserId` = `Guid.Empty` (no/invalid JWT claim) | 400 — `USER_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 2 | Valid UserId, user exists | 200 — `UserResponse` with all fields |
| 3 | Valid UserId, user does not exist (`User.Empty`) | 404 — `USER_NOT_FOUND` |

#### Auth

| # | Scenario | Expected |
|---|----------|----------|
| 4 | No Authorization header | 401 Unauthorized |
| 5 | Invalid/expired JWT | 401 Unauthorized |

### 4.2 PATCH /api/users/me — UpdateUserEndpoint

#### Validation (UpdateUserValidator)

| # | PreferredCurrency | PreferredLocale | Expected |
|---|-------------------|-----------------|----------|
| 1 | `Unknown` | `Unknown` | 400 — `UPDATE_FIELDS_REQUIRED` |
| 2 | `USD` | `Unknown` | Passes validation |
| 3 | `Unknown` | `EN_IN` | Passes validation |
| 4 | `INR` | `EN_IN` | Passes validation |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 5 | User not found | 404 — `USER_NOT_FOUND` |
| 6 | Both fields same as existing (currency=Unknown matches existing, locale=Unknown matches existing) | 204 No Content (no DB update) |
| 7 | `PreferredCurrency` = `Unknown`, new `PreferredLocale` | 204 — updates locale only, preserves existing currency |
| 8 | New `PreferredCurrency`, `PreferredLocale` = `Unknown` | 204 — updates currency only, preserves existing locale |
| 9 | Both fields are new values | 204 — updates both |
| 10 | `PreferredCurrency` matches existing, `PreferredLocale` matches existing | 204 No Content (no DB update) |

---

## 5. API Layer — Asset Items

### 5.1 POST /api/asset-items — AddAssetItemEndpoint

#### Validation (AddAssetItemValidator)

| # | Field | Condition | Expected |
|---|-------|-----------|----------|
| 1 | `AssetType` | `Unknown` | 400 — `ASSET_TYPE_UNKNOWN` |
| 2 | `Name` | empty/null | 400 — `NAME_REQUIRED` |
| 3 | `AssetClass` | `Unknown` when AssetType is NOT Stock/Bond (e.g., BankAccount) | 400 — `ASSET_CLASS_REQUIRED` |
| 4 | `AssetClass` | non-Unknown when AssetType IS Stock | 400 — `ASSET_CLASS_NOT_ALLOWED` |
| 5 | `AssetClass` | non-Unknown when AssetType IS Bond | 400 — `ASSET_CLASS_NOT_ALLOWED` |
| 6 | `AssetClass` | `EmergencyFund` when AssetType is `MutualFund` | 400 — `ASSET_CLASS_INVALID` |
| 7 | `AssetClass` | `Unknown` when AssetType is `MutualFund` | 400 — `ASSET_CLASS_REQUIRED` |
| 8 | `AssetClass` | `Equity` when AssetType is `MutualFund` | Passes validation |
| 9 | `AssetClass` | `Debt` when AssetType is `MutualFund` | Passes validation |
| 10 | `AssetClass` | `Commodities` when AssetType is `MutualFund` | Passes validation |
| 11 | `ExternalId` | empty when AssetType is `MutualFund` | 400 — `EXTERNAL_ID_REQUIRED` |
| 12 | `ExternalId` | empty when AssetType is `Stock` | 400 — `EXTERNAL_ID_REQUIRED` |
| 13 | `ExternalId` | non-empty when AssetType is `BankAccount` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 14 | `ExternalId` | non-empty when AssetType is `Wallet` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 15 | `ExternalId` | non-empty when AssetType is `TradingAccount` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 16 | `ExternalId` | non-empty when AssetType is `FixedDeposit` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 17 | `ExternalId` | non-empty when AssetType is `EPF` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 18 | `ExternalId` | non-empty when AssetType is `PPF` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 19 | `ExternalId` | non-empty when AssetType is `Bond` | 400 — `EXTERNAL_ID_NOT_ALLOWED` |
| 20 | `Currency` | `Unknown` when AssetType is NOT MutualFund/Stock (e.g., BankAccount) | 400 — `CURRENCY_REQUIRED` |
| 21 | `Currency` | non-Unknown when AssetType is `MutualFund` | 400 — `CURRENCY_NOT_ALLOWED` |
| 22 | `Currency` | non-Unknown when AssetType is `Stock` | 400 — `CURRENCY_NOT_ALLOWED` |

#### Handler — MutualFund Path

| # | Scenario | Expected |
|---|----------|----------|
| 23 | Asset already exists by externalId `"mf-{ExternalId}"` | 201 — uses existing asset, creates asset item |
| 24 | Asset doesn't exist, mutual fund API returns valid fund | 201 — creates asset with `Currency.INR`, creates asset item |
| 25 | Asset doesn't exist, mutual fund API returns empty SchemeCode | 404 — `MUTUAL_FUND_NOT_FOUND` |

#### Handler — Stock Path

| # | Scenario | Expected |
|---|----------|----------|
| 26 | Asset already exists by externalId `"stock-{ExternalId.ToLower()}"` | 201 — uses existing asset, creates asset item |
| 27 | Asset doesn't exist, stock API returns valid stock | 201 — creates asset with `Currency.USD`, `AssetClass.Equity`, creates asset item |
| 28 | Asset doesn't exist, stock API returns empty Symbol | 404 — `STOCK_NOT_FOUND` |

#### Handler — Bond Path

| # | Scenario | Expected |
|---|----------|----------|
| 29 | Bond asset type | 201 — creates asset with `AssetClass.Debt` override (ignoring provided AssetClass) |

#### Handler — Default Path (BankAccount, Wallet, TradingAccount, FixedDeposit, EPF, PPF)

| # | Scenario | AssetType | Expected |
|---|----------|-----------|----------|
| 30 | Asset already exists by default externalId | BankAccount | 201 — uses existing asset |
| 31 | Asset doesn't exist | BankAccount | 201 — creates new asset with `"default-{AssetClass}-{AssetType}-{Currency}"` |
| 32 | Valid request | Wallet | 201 |
| 33 | Valid request | TradingAccount | 201 |
| 34 | Valid request | FixedDeposit | 201 |
| 35 | Valid request | EPF | 201 |
| 36 | Valid request | PPF | 201 |

### 5.2 GET /api/asset-items — GetAllAssetItemsEndpoint

#### Validation (GetAllAssetItemsValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `UserId` = `Guid.Empty` | 400 — `USER_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 2 | User has no asset items | 200 — empty array |
| 3 | User has multiple asset items | 200 — array of `AssetItemResponse` with correct AssetType/AssetClass/Currency |

### 5.3 GET /api/asset-items/{id} — GetAssetItemEndpoint

#### Validation (GetAssetItemValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `Id` = `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 2 | Asset item exists | 200 — `AssetItemResponse` |
| 3 | Asset item not found | 404 — `ASSET_ITEM_NOT_FOUND` |

### 5.4 DELETE /api/asset-items/{id} — DeleteAssetItemEndpoint

#### Validation (DeleteAssetItemValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `Id` = `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 2 | Asset item exists | 204 No Content |
| 3 | Asset item not found | 404 — `ASSET_ITEM_NOT_FOUND` |

### 5.5 GET /api/asset-items/valuations — GetValuationsEndpoint

#### Validation (GetValuationsValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `AssetItemIds` = empty/null | 400 — `ASSET_ITEM_IDS_REQUIRED` |
| 2 | `Currency` = `Unknown` | 400 — `CURRENCY_REQUIRED` |

#### Handler — ValidateRequestParameters

| # | Scenario | Expected |
|---|----------|----------|
| 3 | One or more AssetItemIds refer to non-existent items | 400 — `ASSET_ITEM_NOT_FOUND` (per missing item) |
| 4 | Duplicate AssetItemIds in request | Deduplicated (`.Distinct()`) before processing |
| 5 | All asset items exist | Proceeds to valuation calculation |

#### Handler — CalculateValuationsAsync / Valuation Dates

| # | Scenario | Expected |
|---|----------|----------|
| 6 | No transactions for any asset item on current date | Returns single entry `{Date: today, InvestedValue: 0, CurrentValue: 0, XirrPercent: 0}` |
| 7 | No transactions before a historical end-of-month date | Stops yielding (returns `DateOnly.MinValue` sentinel → `yield break`) |
| 8 | Transactions exist across multiple months | Returns valuation for today + each end-of-month going backwards until no transactions |

#### Handler — CalculateCurrentValueAsync (per transaction type)

| # | TransactionType | Expected effect on CurrentValue |
|---|-----------------|--------------------------------|
| 9 | `Buy` | `+CalculateCurrentAmountAsync` |
| 10 | `Deposit` | `+CalculateCurrentAmountAsync` |
| 11 | `SelfInterest` | `+CalculateCurrentAmountAsync` |
| 12 | `Sell` | `-CalculateInitialAmountAsync` |
| 13 | `Withdrawal` | `-CalculateCurrentAmountAsync` |
| 14 | `InterestPenalty` | `-CalculateCurrentAmountAsync` |
| 15 | Other (Interest, Dividend, Unknown) | `0` |

#### Handler — CalculateInvestedValueAsync (per asset type)

| # | AssetType | Expected |
|---|-----------|----------|
| 16 | `BankAccount`, `Wallet`, `TradingAccount`, `Bond` | InvestedValue = CurrentValue |
| 17 | `FixedDeposit`, `EPF`, `PPF`, `MutualFund`, `Stock` | Invested value computed from Buy/Deposit transactions minus withdrawals/sells |
| 18 | `Unknown` asset type | Throws `InvalidOperationException` |

#### Handler — CalculateInvestedValueAsync reduction logic

| # | Scenario | Expected |
|---|----------|----------|
| 19 | Buy transaction fully offset by sold units | Transaction skipped (continue) |
| 20 | Deposit fully offset by withdrawals | Transaction skipped (continue) |
| 21 | Partially offset Buy (some units sold) | Remaining units used for invested value |
| 22 | Partially offset Deposit (partial withdrawal) | Remaining amount used for invested value |
| 23 | No withdrawals/sells | Full Buy/Deposit amounts contribute to invested value |

#### Handler — CalculateInitialAmountAsync

| # | TransactionType | Date used |
|---|-----------------|-----------|
| 24 | `Deposit` | `valuationDate` (not `transaction.Date`) |
| 25 | `Withdrawal` | `valuationDate` (not `transaction.Date`) |
| 26 | `Buy` | `transaction.Date` |
| 27 | `Sell` | `transaction.Date` |
| 28 | Other types | `transaction.Date` |

#### Handler — XIRR TransactionAmount (per type)

| # | TransactionType | Expected sign |
|---|-----------------|---------------|
| 29 | `Buy` | `+` (positive / inflow) |
| 30 | `Deposit` | `+` |
| 31 | `InterestPenalty` | `+` |
| 32 | `Sell` | `-` (negative / outflow) |
| 33 | `Withdrawal` | `-` |
| 34 | `Interest` | `-` |
| 35 | `Dividend` | `-` |
| 36 | Other (`SelfInterest`, `Unknown`) | `0` |

#### Handler — XIRR BalanceAmount (per type)

| # | TransactionType | Expected sign |
|---|-----------------|---------------|
| 37 | `Buy` | `+` |
| 38 | `Deposit` | `+` |
| 39 | `SelfInterest` | `+` |
| 40 | `Sell` | `-` |
| 41 | `Withdrawal` | `-` |
| 42 | `InterestPenalty` | `-` |
| 43 | Other (`Interest`, `Dividend`, `Unknown`) | `0` |

#### Handler — CalculateXirr

| # | Scenario | Expected |
|---|----------|----------|
| 44 | Empty xirrInputs | Returns `0` |
| 45 | All transactions have `YearDiff < 1` and `balanceAmount != 0` | YearDiff normalized to `1.0` |
| 46 | Transactions span over a year | YearDiff used as-is |
| 47 | Multiple transactions on the same date | Grouped by `YearDiff`, amounts summed |
| 48 | Positive NPV scenario | XIRR converges to a positive return |
| 49 | Negative NPV scenario | XIRR converges to a negative return |

#### Handler — GetValuationDates

| # | Scenario | Expected |
|---|----------|----------|
| 50 | Current date is 2026-06-01 (frozen time) | First date = 2026-06-01, then 2026-05-31, 2026-04-30, etc. |

---

## 6. API Layer — Transactions

### 6.1 POST /api/asset-items/{assetItemId}/transactions — AddTransactionEndpoint

#### Validation (AddTransactionValidator)

| # | Field | Condition | Expected |
|---|-------|-----------|----------|
| 1 | `AssetItemId` | `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |
| 2 | `Date` | `default(DateOnly)` (0001-01-01) | 400 — `DATE_REQUIRED` |
| 3 | `Date` | Future date (> today) | 400 — `DATE_IN_FUTURE` |
| 4 | `Date` | Today's date | Passes |
| 5 | `Date` | Past date | Passes |
| 6 | `Name` | empty/null | 400 — `NAME_REQUIRED` |
| 7 | `Name` | 1–2 characters | 400 — `NAME_TOO_SHORT` |
| 8 | `Name` | > 1000 characters | 400 — `NAME_TOO_LONG` |
| 9 | `Name` | exactly 3 characters | Passes |
| 10 | `Name` | exactly 1000 characters | Passes |
| 11 | `TransactionType` | `Unknown` | 400 — `TRANSACTION_TYPE_REQUIRED` |

##### Amount rules — units-based types (Buy, Sell)

| # | TransactionType | Units | Price | Expected |
|---|-----------------|-------|-------|----------|
| 12 | `Buy` | `0` | `> 0` | 400 — `UNITS_REQUIRED` |
| 13 | `Buy` | `< 0` | `> 0` | 400 — `UNITS_REQUIRED` |
| 14 | `Buy` | `> 0` | `0` | 400 — `PRICE_REQUIRED` |
| 15 | `Buy` | `> 0` | `< 0` | 400 — `PRICE_REQUIRED` |
| 16 | `Buy` | `> 0` | `> 0` | Passes |
| 17 | `Sell` | `0` | `> 0` | 400 — `UNITS_REQUIRED` |
| 18 | `Sell` | `> 0` | `> 0` | Passes |

##### Amount rules — amount-based types (all others except Unknown)

| # | TransactionType | Amount | Expected |
|---|-----------------|--------|----------|
| 19 | `Deposit` | `0` | 400 — `AMOUNT_REQUIRED` |
| 20 | `Deposit` | `< 0` | 400 — `AMOUNT_REQUIRED` |
| 21 | `Deposit` | `> 0` | Passes |
| 22 | `Withdrawal` | `> 0` | Passes |
| 23 | `Interest` | `> 0` | Passes |
| 24 | `SelfInterest` | `> 0` | Passes |
| 25 | `Dividend` | `> 0` | Passes |
| 26 | `InterestPenalty` | `> 0` | Passes |

##### Transaction type validity per asset type

| # | AssetType | TransactionType | Expected |
|---|-----------|-----------------|----------|
| 27 | `BankAccount` | `Deposit` | Passes |
| 28 | `BankAccount` | `Withdrawal` | Passes |
| 29 | `BankAccount` | `Interest` | Passes |
| 30 | `BankAccount` | `SelfInterest` | Passes |
| 31 | `BankAccount` | `InterestPenalty` | Passes |
| 32 | `BankAccount` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 33 | `BankAccount` | `Sell` | 400 — `TRANSACTION_TYPE_INVALID` |
| 34 | `BankAccount` | `Dividend` | 400 — `TRANSACTION_TYPE_INVALID` |
| 35 | `FixedDeposit` | `Deposit` | Passes |
| 36 | `FixedDeposit` | `Withdrawal` | Passes |
| 37 | `FixedDeposit` | `Interest` | Passes |
| 38 | `FixedDeposit` | `SelfInterest` | Passes |
| 39 | `FixedDeposit` | `InterestPenalty` | Passes |
| 40 | `FixedDeposit` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 41 | `EPF` | `Deposit` | Passes |
| 42 | `EPF` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 43 | `PPF` | `Deposit` | Passes |
| 44 | `PPF` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 45 | `MutualFund` | `Buy` | Passes |
| 46 | `MutualFund` | `Sell` | Passes |
| 47 | `MutualFund` | `Deposit` | 400 — `TRANSACTION_TYPE_INVALID` |
| 48 | `MutualFund` | `Dividend` | 400 — `TRANSACTION_TYPE_INVALID` |
| 49 | `Stock` | `Buy` | Passes |
| 50 | `Stock` | `Sell` | Passes |
| 51 | `Stock` | `Dividend` | Passes |
| 52 | `Stock` | `Deposit` | 400 — `TRANSACTION_TYPE_INVALID` |
| 53 | `Wallet` | `Deposit` | Passes |
| 54 | `Wallet` | `Withdrawal` | Passes |
| 55 | `Wallet` | `Interest` | 400 — `TRANSACTION_TYPE_INVALID` |
| 56 | `TradingAccount` | `Deposit` | Passes |
| 57 | `TradingAccount` | `Withdrawal` | Passes |
| 58 | `TradingAccount` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 59 | `Bond` | `Deposit` | Passes |
| 60 | `Bond` | `Withdrawal` | Passes |
| 61 | `Bond` | `Interest` | Passes |
| 62 | `Bond` | `Buy` | 400 — `TRANSACTION_TYPE_INVALID` |
| 63 | `Bond` | `SelfInterest` | 400 — `TRANSACTION_TYPE_INVALID` |

##### Validation bypass when AssetItemId is Empty or TransactionType is Unknown

| # | Scenario | Expected |
|---|----------|----------|
| 64 | `AssetItemId` = `Guid.Empty` AND `TransactionType` = `Buy` | Skips type-validity check (`.When` guard) |
| 65 | `TransactionType` = `Unknown` AND `AssetItemId` valid | Skips type-validity check |
| 66 | Asset item not found during validation lookup | Returns `true` (skips type-validity check) |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 67 | Asset item not found | 404 — `ASSET_ITEM_NOT_FOUND` |
| 68 | Valid Buy transaction | 201 — units/price from request, amount = 0 |
| 69 | Valid Sell transaction | 201 — units/price from request, amount = 0 |
| 70 | Valid Deposit transaction | 201 — units = 0, price = 0, amount from request |
| 71 | Valid Withdrawal transaction | 201 — units = 0, price = 0, amount from request |
| 72 | Response body always has Units/Price/Amount = 0 (response is skeleton) | Verify response shape |

### 6.2 GET /api/asset-items/{assetItemId}/transactions — GetAllByAssetItemIdEndpoint

#### Validation (GetAllByAssetItemIdValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `AssetItemId` = `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 2 | Asset item not found | 404 — `ASSET_ITEM_NOT_FOUND` |
| 3 | Asset item exists, no transactions | 200 — empty array |
| 4 | Asset item exists, multiple transactions | 200 — each transaction includes calculated `Amount` via `TransactionAmountCalculator` |
| 5 | Currency parameter provided | Amount calculated in specified currency |
| 6 | Currency = `Unknown` (default) | Amount calculated with unknown currency behavior |

### 6.3 GET /api/asset-items/{assetItemId}/transactions/{transactionId} — GetTransactionByIdEndpoint

#### Validation (GetTransactionByIdValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `AssetItemId` = `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |
| 2 | `TransactionId` = `Guid.Empty` | 400 — `TRANSACTION_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 3 | Transaction not found | 404 — `TRANSACTION_NOT_FOUND` |
| 4 | Transaction found | 200 — `TransactionResponse` with calculated amount |

### 6.4 PATCH /api/asset-items/{assetItemId}/transactions/{transactionId} — EditTransactionEndpoint

#### Validation (EditTransactionValidator)

| # | Field | Condition | Expected |
|---|-------|-----------|----------|
| 1 | `AssetItemId` | `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |
| 2 | `Name` | 1–2 characters (non-whitespace) | 400 — `NAME_TOO_SHORT` |
| 3 | `Name` | > 1000 characters | 400 — `NAME_TOO_LONG` |
| 4 | `Name` | empty/whitespace | Skips min/max length checks (`.When` guard) |
| 5 | `Name` | 3–1000 characters | Passes |
| 6 | `Units` | `< 0` when type is Buy/Sell | 400 — `UNITS_INVALID` |
| 7 | `Units` | `0` when type is Buy/Sell | Passes (≥ 0 allowed for edit) |
| 8 | `Price` | `< 0` when type is Buy/Sell | 400 — `PRICE_INVALID` |
| 9 | `Price` | `0` when type is Buy/Sell | Passes |
| 10 | `Amount` | `< 0` when type is Deposit/Withdrawal/etc. | 400 — `AMOUNT_INVALID` |
| 11 | `Amount` | `0` when type is Deposit | Passes |
| 12 | Transaction type validity | Same rules as AddTransaction (per asset type) | 400 — `TRANSACTION_TYPE_INVALID` |
| 13 | `AssetItemId` = `Guid.Empty` or `TransactionType` = `Unknown` | Skips type-validity check |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 14 | Asset item not found | 404 — `ASSET_ITEM_NOT_FOUND` |
| 15 | Transaction not found | 404 — `TRANSACTION_NOT_FOUND` |

##### NormalizeRequest — name handling

| # | Scenario | Expected |
|---|----------|----------|
| 16 | `Name` is empty/whitespace in request | Uses existing transaction name |
| 17 | `Name` is non-empty in request | Uses new name |

##### NormalizeRequest — TransactionType handling

| # | Scenario | Expected |
|---|----------|----------|
| 18 | `TransactionType` = `Unknown` in request | Uses existing transaction type |
| 19 | `TransactionType` = valid value | Uses new transaction type |

##### NormalizeRequest — amount-based type (not Buy/Sell)

| # | Scenario | Expected |
|---|----------|----------|
| 20 | `Amount` = 0 in request | Uses existing amount |
| 21 | `Amount` > 0 in request | Uses new amount; units/price set to 0 |

##### NormalizeRequest — units-based type (Buy/Sell)

| # | Scenario | Expected |
|---|----------|----------|
| 22 | `Units` = 0 in request | Uses existing units |
| 23 | `Units` > 0 in request | Uses new units |
| 24 | `Price` = 0 in request | Uses existing price |
| 25 | `Price` > 0 in request | Uses new price; amount set to 0 |

##### Success

| # | Scenario | Expected |
|---|----------|----------|
| 26 | Valid edit request | 204 No Content, transaction updated in DB |

### 6.5 DELETE /api/asset-items/{assetItemId}/transactions/{transactionId} — DeleteTransactionEndpoint

#### Validation (DeleteTransactionValidator)

| # | Input | Expected |
|---|-------|----------|
| 1 | `AssetItemId` = `Guid.Empty` | 400 — `ASSET_ITEM_ID_REQUIRED` |
| 2 | `TransactionId` = `Guid.Empty` | 400 — `TRANSACTION_ID_REQUIRED` |

#### Handler Paths

| # | Scenario | Expected |
|---|----------|----------|
| 3 | Transaction not found | 404 — `TRANSACTION_NOT_FOUND` |
| 4 | Transaction found | 204 No Content |

---

## 7. Application — TransactionValidationExtensions

### 7.1 IsValidForAssetType

Full matrix of `(AssetType, TransactionType)` → `bool`. All combinations in section 6.1 (scenarios 27–63) cover this.

Additionally:

| # | AssetType | Expected |
|---|-----------|----------|
| 1 | `Unknown` | Throws `InvalidOperationException` |

### 7.2 IsUnitsRequired

| # | TransactionType | Expected |
|---|-----------------|----------|
| 1 | `Buy` | `true` |
| 2 | `Sell` | `true` |
| 3 | `Deposit` | `false` |
| 4 | `Withdrawal` | `false` |
| 5 | `Interest` | `false` |
| 6 | `SelfInterest` | `false` |
| 7 | `Dividend` | `false` |
| 8 | `InterestPenalty` | `false` |
| 9 | `Unknown` | `false` |

---

## 8. Infrastructure Layer

### 8.1 RatesExtensions.GetOnOrBeforeValue

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Exact date exists in dictionary | Returns that rate |
| 2 | Date-1 exists (weekend scenario) | Returns rate for date-1 |
| 3 | Date-6 exists (long holiday) | Returns rate for date-6 |
| 4 | No rate within 7-day lookback | Throws `InvalidOperationException` |
| 5 | Empty dictionary | Throws `InvalidOperationException` |

### 8.2 MutualFundApiClient

#### GetBySymbolAsync

| # | Scenario | Expected |
|---|----------|----------|
| 1 | API returns 404 | Returns empty `MutualFund` (SchemeCode = `""`) |
| 2 | API returns valid response | Returns `MutualFund` with SchemeCode, Name, SchemeType, SchemeCategory, `Currency.INR` |

#### GetPricesAsync

| # | Scenario | Expected |
|---|----------|----------|
| 3 | API returns 404 | Returns empty dictionary |
| 4 | API returns valid data | Returns `FrozenDictionary<DateOnly, decimal>` with parsed dates (dd-MM-yyyy) and NAV values |

#### GetOnOrBeforePriceAsync

| # | Scenario | Expected |
|---|----------|----------|
| 5 | Called directly | Throws `NotSupportedException` (delegated to `CachedAssetApiClient`) |

### 8.3 StockApiClient

#### GetBySymbolAsync

| # | Scenario | Expected |
|---|----------|----------|
| 1 | API returns 404 | Returns empty `Stock` (Symbol = `""`) |
| 2 | API returns empty array | Returns empty `Stock` |
| 3 | API returns result with unrecognized Currency string | Returns empty `Stock` |
| 4 | API returns valid result with recognized Currency (e.g., "USD") | Returns `Stock` with Symbol, Name, Currency |

#### GetPricesAsync

| # | Scenario | Expected |
|---|----------|----------|
| 5 | API returns 404 | Returns empty dictionary |
| 6 | API returns empty array | Returns empty dictionary |
| 7 | API returns valid data | Returns `FrozenDictionary<DateOnly, decimal>` |

#### GetOnOrBeforePriceAsync

| # | Scenario | Expected |
|---|----------|----------|
| 8 | Called directly | Throws `NotSupportedException` |

### 8.4 ExchangeRateApiClient

#### GetExchangeRatesAsync

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `from` == `to` | Returns empty dictionary |
| 2 | `from` != `to`, API returns CSV data | Returns `FrozenDictionary<DateOnly, decimal>` using `close` column |

#### GetOnOrBeforeExchangeRateAsync

| # | Scenario | Expected |
|---|----------|----------|
| 3 | Called directly | Throws `NotSupportedException` |

### 8.5 CachedExchangeRateApiClient

#### GetExchangeRatesAsync

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `from` == `to` | Returns empty dictionary immediately (no cache/API call) |
| 2 | `from` != `to`, first call | Fetches from API, caches result |
| 3 | `from` != `to`, subsequent call | Returns from cache |

#### GetOnOrBeforeExchangeRateAsync

| # | Scenario | Expected |
|---|----------|----------|
| 4 | `from` == `to` | Returns `1m` immediately |
| 5 | `from` != `to` | Fetches rates, calls `GetOnOrBeforeValue`, caches result |

### 8.6 CachedAssetApiClient\<T\>

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetBySymbolAsync` — first call | Fetches from inner client, caches |
| 2 | `GetBySymbolAsync` — repeat call | Returns cached value |
| 3 | `GetPricesAsync` — first call | Fetches from inner client, caches |
| 4 | `GetOnOrBeforePriceAsync` — first call | Fetches prices, uses `RatesExtensions.GetOnOrBeforeValue`, caches result |
| 5 | `GetOnOrBeforePriceAsync` — repeat call same date | Returns cached value |

### 8.7 Repository Tests (UserIdRepository, UserRepository, AssetRepository, AssetItemRepository, TransactionRepository)

For each repository, standard CRUD path coverage:

#### UserIdRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetUserId` — entry exists | Returns `UserId` |
| 2 | `GetUserId` — entry doesn't exist | Returns `UserId.Empty` |
| 3 | `AddUserId` — new entry | Inserts row, returns new `UserId` |

#### UserRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetUserAsync` — user exists | Returns `User` with correct fields |
| 2 | `GetUserAsync` — user doesn't exist | Returns `User.Empty` |
| 3 | `AddUserAsync` — new user | Inserts with defaults (`USD`, `EN_US`), returns `User` |
| 4 | `UpdateUserProfileAsync` | Updates currency/locale and `UpdatedAt` timestamp |

#### AssetRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetByIdAsync` — exists | Returns `Asset` |
| 2 | `GetByIdAsync` — doesn't exist | Returns `Asset.Empty` |
| 3 | `GetByExternalIdAsync` — exists | Returns `Asset` |
| 4 | `GetByExternalIdAsync` — doesn't exist | Returns `Asset.Empty` |
| 5 | `AddAsync` | Inserts row, returns `Asset` |

#### AssetItemRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetAllAsync` — user has items | Returns all items for that user |
| 2 | `GetAllAsync` — user has no items | Returns empty collection |
| 3 | `GetByIdAsync` — exists | Returns `AssetItem` |
| 4 | `GetByIdAsync` — doesn't exist | Returns `AssetItem.Empty` |
| 5 | `AddAsync` | Inserts row, returns `AssetItem` |
| 6 | `DeleteAsync` | Deletes row |

#### TransactionRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetByAssetItemIdAsync` — has transactions | Returns all transactions for that asset item |
| 2 | `GetByAssetItemIdAsync` — no transactions | Returns empty collection |
| 3 | `GetByIdAsync` — exists | Returns `Transaction` |
| 4 | `GetByIdAsync` — doesn't exist | Returns `Transaction.Empty` |
| 5 | `AddAsync` | Inserts row, returns `Transaction` |
| 6 | `UpdateAsync` | Updates all mutable fields and `UpdatedAt` |
| 7 | `DeleteAsync` | Deletes row |

### 8.8 Cached Repository Decorators (CachedUserRepository, CachedAssetRepository, CachedAssetItemRepository, CachedTransactionRepository)

#### CachedUserRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetUserAsync` — first call | Fetches from DB, caches |
| 2 | `GetUserAsync` — repeat call | Returns cached value |
| 3 | `AddUserAsync` | Delegates directly (no caching) |
| 4 | `UpdateUserProfileAsync` | Updates DB, then invalidates cache key `users/{userId}` |

#### CachedAssetRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetByIdAsync` — first call | Fetches from DB, caches |
| 2 | `GetByExternalIdAsync` — first call | Fetches from DB, caches |
| 3 | `AddAsync` | Delegates directly (no caching) |

#### CachedAssetItemRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetAllAsync` — caches list | First fetches, subsequent returns cache |
| 2 | `GetByIdAsync` — caches individual item | First fetches, subsequent returns cache |
| 3 | `AddAsync` — invalidates list cache | After add, `users/{id}/assetItems` cache removed |
| 4 | `DeleteAsync` — invalidates list AND item cache | Both keys removed |

#### CachedTransactionRepository

| # | Scenario | Expected |
|---|----------|----------|
| 1 | `GetByAssetItemIdAsync` — caches with tag | Cached under transactions tag |
| 2 | `GetByIdAsync` — caches with tag | Cached under transactions tag |
| 3 | `AddAsync` — invalidates transaction + valuation tags | Both tags cleared |
| 4 | `UpdateAsync` — invalidates transaction + valuation tags | Both tags cleared |
| 5 | `DeleteAsync` — invalidates transaction + valuation tags | Both tags cleared |

---

## 9. Cross-Cutting Concerns

### 9.1 Authentication / Authorization

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Request to any protected endpoint without `Authorization` header | 401 |
| 2 | Request with expired JWT | 401 |
| 3 | Request with invalid JWT signature | 401 |
| 4 | Request with valid JWT but wrong audience | 401 |
| 5 | Request with valid JWT but wrong issuer | 401 |
| 6 | `POST /api/auth/login/google` without auth | Allowed (AllowAnonymous) |
| 7 | UserId from JWT claim matches request routing (user isolation) | Only own data accessible |

### 9.2 Error Response Format

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Validation failure | ProblemDetails format with error codes |
| 2 | Multiple validation failures on same request | All errors returned in response |

### 9.3 JSON Serialization

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Enum values in responses | Serialized as strings (JsonStringEnumConverter) |
| 2 | `DateOnly` fields | Correctly serialized/deserialized |
| 3 | `Guid` fields | Correctly serialized/deserialized |
| 4 | `decimal` fields | Correctly serialized/deserialized |

---

## 10. Database Schema Integrity

| # | Scenario | Expected |
|---|----------|----------|
| 1 | Delete user → cascades to `user_ids`, `asset_items`, `transactions` | All child records deleted |
| 2 | Delete asset item → cascades to `transactions` | Child transactions deleted |
| 3 | Delete asset that has asset items → restricted | FK violation (ON DELETE RESTRICT) |
| 4 | Duplicate email in `users` table | Unique index violation |
| 5 | Duplicate `ExternalId` in `assets` table | Unique index violation |
