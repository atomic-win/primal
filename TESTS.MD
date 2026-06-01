# Test Scenarios

All scenarios grouped by endpoint. Columns: Test, Input, Expected, Type (E2E/Integration/Unit), Present (✅/❌).

---

## 1. POST /api/auth/login/google

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects empty IdToken | `{ "IdToken": "" }` | 400 `ID_TOKEN_REQUIRED` | E2E | ✅ |
| 2 | Rejects null IdToken | `{ "IdToken": null }` | 400 `ID_TOKEN_REQUIRED` | E2E | ✅ |
| 3 | Rejects missing body | No request body | 400 `ID_TOKEN_REQUIRED` | E2E | ✅ |
| 4 | Rejects empty JSON body | `{}` | 400 `ID_TOKEN_REQUIRED` | E2E | ✅ |
| 5 | Existing user logs in | Valid Google token; user already in DB | 200 TokenResponse (access + refresh) | E2E | ❌ |
| 6 | New user registers and logs in | Valid Google token; user not in DB | 200 TokenResponse; user + userId rows created | E2E | ❌ |
| 7 | Rejects expired Google token | Expired Google ID token | 401 `ID_TOKEN_EXPIRED` | E2E | ❌ |
| 8 | Rejects invalid Google token | Malformed Google ID token | 401 `ID_TOKEN_INVALID` | E2E | ❌ |
| 9 | Returns 500 on unexpected error | Google validation throws non-JWT exception | 500 `UNEXPECTED_ERROR` | E2E | ❌ |
| 10 | Accessible without auth | No Authorization header | Endpoint reachable (AllowAnonymous) | E2E | ✅ |

> **Note:** Unit tests exist for GoogleLoginValidator (`ValidateAsync_ReturnsError_WhenIdTokenIsEmpty`, `ValidateAsync_ReturnsValid_WhenIdTokenIsProvided`) but no E2E tests for this endpoint.

---

## 2. POST /api/auth/refresh-token

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Refreshes valid token | Valid refresh token in body | 200 new TokenResponse | E2E | ✅ |
| 2 | Rejects invalid refresh token | Invalid/expired refresh token | 401 | E2E | ❌ |
| 3 | Rejects missing body | No request body | 401/400 | E2E | ✅ |
| 4 | Rejects empty JSON body | `{}` | 401/400 | E2E | ✅ |

---

## 3. GET /api/users/me

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Returns user profile | Valid JWT; user exists | 200 UserResponse with all fields | E2E | ✅ |
| 2 | Returns 404 for missing user | Valid JWT; user not in DB | 404 `USER_NOT_FOUND` | E2E | ✅ |
| 3 | Rejects missing auth | No Authorization header | 401 | E2E | ✅ |
| 4 | Rejects expired JWT | Expired JWT | 401 | E2E | ✅ |
| 5 | Rejects invalid JWT signature | Tampered JWT | 401 | E2E | ✅ |
| 6 | Rejects wrong audience | JWT with wrong audience | 401 | E2E | ✅ |
| 7 | Rejects wrong issuer | JWT with wrong issuer | 401 | E2E | ✅ |

---

## 4. PATCH /api/users/me

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects both fields unknown | `{ PreferredCurrency: Unknown, PreferredLocale: Unknown }` | 400 `UPDATE_FIELDS_REQUIRED` | E2E | ✅ |
| 2 | Rejects empty body | `{}` (both default to Unknown) | 400 `UPDATE_FIELDS_REQUIRED` | E2E | ✅ |
| 3 | Rejects no body | No request body | 400 `UPDATE_FIELDS_REQUIRED` | E2E | ✅ |
| 4 | Returns 404 for missing user | Valid JWT; user not in DB; body `{ PreferredCurrency: "INR" }` | 404 `USER_NOT_FOUND` | E2E | ✅ |
| 5 | Updates currency only | `{ PreferredCurrency: "INR" }` (locale key missing → Unknown) | 204; currency=INR, locale preserved | E2E | ✅ |
| 6 | Updates locale only | `{ PreferredLocale: "EN_IN" }` (currency key missing → Unknown) | 204; locale=EN_IN, currency preserved | E2E | ✅ |
| 7 | Updates both fields | `{ PreferredCurrency: "INR", PreferredLocale: "EN_IN" }` | 204; both updated | E2E | ✅ |
| 8 | No-op when values match existing | Currency=existing, Locale=existing | 204; no DB update | E2E | ✅ |
| 9 | No-op when both unknown and existing match | Currency=Unknown (existing=USD), Locale=Unknown (existing=EN_US) | 204; no DB update | E2E | ❌ |
| 10 | Rejects missing auth | No Authorization header | 401 | E2E | ✅ |

---

## 5. POST /api/asset-items

### Validation

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects empty body | `{}` | 400 multiple errors (ASSET_TYPE_UNKNOWN, NAME_REQUIRED, etc.) | E2E | ✅ |
| 2 | Rejects no body | No request body | 400 multiple errors | E2E | ✅ |
| 3 | Rejects Unknown AssetType | `AssetType: "Unknown"` | 400 `ASSET_TYPE_UNKNOWN` | E2E | ✅ |
| 4 | Rejects missing AssetType | AssetType key absent (defaults to Unknown) | 400 `ASSET_TYPE_UNKNOWN` | E2E | ❌ |
| 5 | Rejects empty Name | `Name: ""` | 400 `NAME_REQUIRED` | E2E | ✅ |
| 6 | Rejects missing Name | Name key absent | 400 `NAME_REQUIRED` | E2E | ❌ |
| 7 | Rejects Unknown AssetClass for BankAccount | `AssetType: BankAccount, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 8 | Rejects Unknown AssetClass for Wallet | `AssetType: Wallet, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 9 | Rejects Unknown AssetClass for TradingAccount | `AssetType: TradingAccount, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 10 | Rejects Unknown AssetClass for FixedDeposit | `AssetType: FixedDeposit, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 11 | Rejects Unknown AssetClass for EPF | `AssetType: EPF, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 12 | Rejects Unknown AssetClass for PPF | `AssetType: PPF, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 13 | Rejects Unknown AssetClass for MutualFund | `AssetType: MutualFund, AssetClass: Unknown` | 400 `ASSET_CLASS_REQUIRED` | E2E | ✅ |
| 14 | Rejects non-Unknown AssetClass for Stock | `AssetType: Stock, AssetClass: Equity` | 400 `ASSET_CLASS_NOT_ALLOWED` | E2E | ✅ |
| 15 | Rejects non-Unknown AssetClass for Bond | `AssetType: Bond, AssetClass: Debt` | 400 `ASSET_CLASS_NOT_ALLOWED` | E2E | ✅ |
| 16 | Rejects EmergencyFund class for MutualFund | `AssetType: MutualFund, AssetClass: EmergencyFund` | 400 `ASSET_CLASS_INVALID` | E2E | ✅ |
| 17 | Accepts Equity class for MutualFund | `AssetType: MutualFund, AssetClass: Equity` | Passes | E2E | ✅ |
| 18 | Accepts Debt class for MutualFund | `AssetType: MutualFund, AssetClass: Debt` | Passes | E2E | ✅ |
| 19 | Accepts Commodities class for MutualFund | `AssetType: MutualFund, AssetClass: Commodities` | Passes | E2E | ✅ |
| 20 | Rejects empty ExternalId for MutualFund | `AssetType: MutualFund, ExternalId: ""` | 400 `EXTERNAL_ID_REQUIRED` | E2E | ✅ |
| 21 | Rejects empty ExternalId for Stock | `AssetType: Stock, ExternalId: ""` | 400 `EXTERNAL_ID_REQUIRED` | E2E | ✅ |
| 22 | Rejects non-empty ExternalId for BankAccount | `AssetType: BankAccount, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 23 | Rejects non-empty ExternalId for Wallet | `AssetType: Wallet, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 24 | Rejects non-empty ExternalId for TradingAccount | `AssetType: TradingAccount, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 25 | Rejects non-empty ExternalId for FixedDeposit | `AssetType: FixedDeposit, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 26 | Rejects non-empty ExternalId for EPF | `AssetType: EPF, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 27 | Rejects non-empty ExternalId for PPF | `AssetType: PPF, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 28 | Rejects non-empty ExternalId for Bond | `AssetType: Bond, ExternalId: "x"` | 400 `EXTERNAL_ID_NOT_ALLOWED` | E2E | ✅ |
| 29 | Rejects Unknown Currency for BankAccount | `AssetType: BankAccount, Currency: Unknown` | 400 `CURRENCY_REQUIRED` | E2E | ✅ |
| 30 | Rejects non-Unknown Currency for MutualFund | `AssetType: MutualFund, Currency: INR` | 400 `CURRENCY_NOT_ALLOWED` | E2E | ✅ |
| 31 | Rejects non-Unknown Currency for Stock | `AssetType: Stock, Currency: USD` | 400 `CURRENCY_NOT_ALLOWED` | E2E | ✅ |

### Handler

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 32 | Creates MutualFund — new asset | Valid MF request; asset not in DB; MF API returns fund | 201 AssetItemResponse; asset created with Currency=INR | E2E | ✅ |
| 33 | Creates MutualFund — existing asset | Valid MF request; asset already in DB by externalId | 201; reuses existing asset | E2E | ✅ |
| 34 | MutualFund not found in API | Valid MF request; MF API returns empty SchemeCode | 404 `MUTUAL_FUND_NOT_FOUND` | E2E | ✅ |
| 35 | Creates Stock — new asset | Valid Stock request; asset not in DB; Stock API returns stock | 201; asset created with Currency=USD, AssetClass=Equity | E2E | ✅ |
| 36 | Creates Stock — existing asset | Valid Stock request; asset already in DB | 201; reuses existing asset | E2E | ✅ |
| 37 | Stock not found in API | Valid Stock request; Stock API returns empty Symbol | 404 `STOCK_NOT_FOUND` | E2E | ✅ |
| 38 | Creates Bond asset item | Valid Bond request | 201; asset created with AssetClass=Debt override | E2E | ✅ |
| 39 | Creates BankAccount — new asset | Valid BankAccount request; asset not in DB | 201; asset externalId = `default-{class}-BankAccount-{currency}` | E2E | ✅ |
| 40 | Creates BankAccount — existing asset | Valid BankAccount request; matching default asset exists | 201; reuses existing asset | E2E | ✅ |
| 41 | Creates Wallet asset item | Valid Wallet request | 201 | E2E | ✅ |
| 42 | Creates TradingAccount asset item | Valid TradingAccount request | 201 | E2E | ✅ |
| 43 | Creates FixedDeposit asset item | Valid FixedDeposit request | 201 | E2E | ✅ |
| 44 | Creates EPF asset item | Valid EPF request | 201 | E2E | ✅ |
| 45 | Creates PPF asset item | Valid PPF request | 201 | E2E | ✅ |

---

## 6. GET /api/asset-items

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Returns empty list | User has no asset items | 200 `[]` | E2E | ✅ |
| 2 | Returns all items with asset details | User has multiple asset items | 200 array of AssetItemResponse (Id, Name, AssetType, AssetClass, Currency) | E2E | ✅ |

---

## 7. GET /api/asset-items/{id}

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Returns asset item | Valid GUID; item exists | 200 AssetItemResponse | E2E | ✅ |
| 2 | Returns 404 for missing item | Valid GUID; item not in DB | 404 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 3 | Rejects non-GUID route param | `/api/asset-items/not-a-guid` | 404 route not matched | E2E | ✅ |

---

## 8. DELETE /api/asset-items/{id}

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Deletes existing item | Valid GUID; item exists | 204 | E2E | ✅ |
| 2 | Returns 404 for missing item | Valid GUID; item not in DB | 404 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 3 | Cascades to transactions | Delete item that has transactions | 204; child transactions also deleted | E2E | ✅ |
| 4 | Rejects non-GUID route param | `/api/asset-items/not-a-guid` | 404 route not matched | E2E | ✅ |

---

## 9. GET /api/asset-items/valuations

### Validation

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects empty AssetItemIds | `AssetItemIds: []` or missing | 400 `ASSET_ITEM_IDS_REQUIRED` | E2E | ✅ |
| 2 | Rejects Unknown Currency | `Currency: Unknown` or missing | 400 `CURRENCY_REQUIRED` | E2E | ✅ |
| 3 | Rejects no query params | No query params at all | 400 both errors | E2E | ✅ |
| 4 | Rejects non-existent asset item IDs | `AssetItemIds: [<non-existent-guid>]` | 400 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 5 | Rejects malformed GUID | `AssetItemIds: ["not-a-guid"]` | 400 binding error | E2E | ❌ |
| 6 | Deduplicates AssetItemIds | Same GUID passed twice | Processes only once; valid response | E2E | ✅ |
| 7 | Rejects missing auth | No Authorization header | 401 | E2E | ✅ |

### Valuations — basic

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 8 | No transactions → zero valuation | Asset item with no transactions; Currency=USD | 200 single entry: `{Date: today, Invested: 0, Current: 0, Xirr: 0}` | E2E | ✅ |
| 9 | Stops at month with no transactions | Transactions only in current month | Response yields today + end-of-last-month (zero) then stops | E2E | ✅ |
| 10 | Multi-month history | Transactions across 3+ months | Returns today + descending end-of-month entries | E2E | ✅ |

### Valuations — CurrentValue by TransactionType

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 11 | Buy adds to CurrentValue | MutualFund with Buy transaction | CurrentValue includes units × current price | E2E | ✅ |
| 12 | Deposit adds to CurrentValue | BankAccount with Deposit | CurrentValue includes deposit amount | E2E | ✅ |
| 13 | SelfInterest adds to CurrentValue | BankAccount with SelfInterest | CurrentValue includes amount | E2E | ✅ |
| 14 | Sell subtracts from CurrentValue | MutualFund with Buy then Sell | CurrentValue reduced by sell (at original price) | E2E | ✅ |
| 15 | Withdrawal subtracts from CurrentValue | BankAccount with Deposit then Withdrawal | CurrentValue reduced | E2E | ✅ |
| 16 | InterestPenalty subtracts from CurrentValue | BankAccount with Deposit then InterestPenalty | CurrentValue reduced | E2E | ✅ |
| 17 | Interest does not affect CurrentValue | BankAccount with Deposit + Interest | Interest contributes 0 to CurrentValue | E2E | ✅ |
| 18 | Dividend does not affect CurrentValue | Stock with Buy + Dividend | Dividend contributes 0 to CurrentValue | E2E | ✅ |

### Valuations — InvestedValue by AssetType

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 19 | BankAccount: Invested = Current | BankAccount with deposits/withdrawals | InvestedValue equals CurrentValue | E2E | ✅ |
| 20 | Wallet: Invested = Current | Wallet with deposits/withdrawals | InvestedValue equals CurrentValue | E2E | ✅ |
| 21 | TradingAccount: Invested = Current | TradingAccount with deposits | InvestedValue equals CurrentValue | E2E | ✅ |
| 22 | Bond: Invested = Current | Bond with deposits | InvestedValue equals CurrentValue | E2E | ✅ |
| 23 | MutualFund: Invested from Buy minus Sell | MF with Buy(10u@100) then Sell(3u) | InvestedValue = remaining 7 units × buy price | E2E | ✅ |
| 24 | FixedDeposit: Invested from Deposit minus Withdrawal | FD with Deposit then partial Withdrawal | InvestedValue = remaining deposit amount | E2E | ✅ |
| 25 | Stock: Invested from Buy minus Sell | Stock with Buy then partial Sell | InvestedValue = remaining units × buy price | E2E | ✅ |
| 26 | Full offset → zero invested | Buy 10u then Sell 10u | InvestedValue = 0 | E2E | ✅ |
| 27 | Partial offset | Buy 10u@100, Sell 3u | InvestedValue = 7u × buy price (adjusted) | E2E | ❌ |

### Valuations — XIRR

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 28 | No transactions → XIRR = 0 | Empty transactions | XirrPercent = 0 | E2E | ❌ |
| 29 | All < 1 year: YearDiff normalized | Single Buy transaction from 3 months ago | XIRR calculated with YearDiff=1.0 | E2E | ❌ |
| 30 | Transactions > 1 year | Buy from 2 years ago | XIRR uses actual YearDiff | E2E | ❌ |
| 31 | Positive return scenario | Buy low, current price higher | XirrPercent > 0 | E2E | ❌ |
| 32 | Negative return scenario | Buy high, current price lower | XirrPercent < 0 | E2E | ❌ |
| 33 | Same-date transactions grouped | Two Buy transactions on same date | YearDiff grouped, amounts summed | E2E | ❌ |

### Valuations — cross-currency

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 34 | Same currency → no exchange rate | INR asset, Currency=INR | Values computed without exchange rate conversion | E2E | ✅ |
| 35 | Cross-currency conversion | INR asset, Currency=USD | Values converted using exchange rate | E2E | ✅ |

### Valuations — CalculateInitialAmount date handling

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 36 | Deposit uses valuationDate | Deposit transaction | Amount calculated at valuation date exchange rate | E2E | ❌ |
| 37 | Buy uses transaction.Date | Buy transaction | Amount calculated at transaction date price | E2E | ❌ |

---

## 10. POST /api/asset-items/{assetItemId}/transactions

### Validation — field rules

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects empty body | `{}` or no body | 400 multiple errors (DATE_REQUIRED, NAME_REQUIRED, TRANSACTION_TYPE_REQUIRED) | E2E | ✅ |
| 2 | Rejects default Date | `Date: "0001-01-01"` or Date key missing | 400 `DATE_REQUIRED` | E2E | ✅ |
| 3 | Rejects future Date | `Date: tomorrow` | 400 `DATE_IN_FUTURE` | E2E | ✅ |
| 4 | Accepts today's Date | `Date: today` | Passes | E2E | ✅ |
| 5 | Accepts past Date | `Date: yesterday` | Passes | E2E | ✅ |
| 6 | Rejects empty Name | `Name: ""` or Name key missing | 400 `NAME_REQUIRED` | E2E | ✅ |
| 7 | Rejects short Name | `Name: "ab"` (2 chars) | 400 `NAME_TOO_SHORT` | E2E | ✅ |
| 8 | Rejects long Name | `Name: "a" × 1001` | 400 `NAME_TOO_LONG` | E2E | ✅ |
| 9 | Accepts min-length Name | `Name: "abc"` (3 chars) | Passes | E2E | ✅ |
| 10 | Accepts max-length Name | `Name: "a" × 1000` | Passes | E2E | ✅ |
| 11 | Rejects Unknown TransactionType | `TransactionType: "Unknown"` or key missing | 400 `TRANSACTION_TYPE_REQUIRED` | E2E | ✅ |
| 12 | Rejects non-GUID route param | `/api/asset-items/not-a-guid/transactions` | 404 route not matched | E2E | ✅ |

### Validation — amount rules (Buy/Sell)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 13 | Rejects zero Units for Buy | `TransactionType: Buy, Units: 0` | 400 `UNITS_REQUIRED` | E2E | ✅ |
| 14 | Rejects negative Units for Buy | `TransactionType: Buy, Units: -1` | 400 `UNITS_REQUIRED` | E2E | ✅ |
| 15 | Rejects zero Price for Buy | `TransactionType: Buy, Units: 10, Price: 0` | 400 `PRICE_REQUIRED` | E2E | ✅ |
| 16 | Rejects negative Price for Buy | `TransactionType: Buy, Units: 10, Price: -1` | 400 `PRICE_REQUIRED` | E2E | ✅ |
| 17 | Accepts valid Buy | `Units: 10, Price: 100` | Passes | E2E | ✅ |
| 18 | Rejects zero Units for Sell | `TransactionType: Sell, Units: 0` | 400 `UNITS_REQUIRED` | E2E | ✅ |
| 19 | Accepts valid Sell | `Units: 5, Price: 100` | Passes | E2E | ✅ |
| 20 | Buy ignores Amount field | `Buy, Units: 10, Price: 100, Amount: 500` | Passes; stored Amount=0 | E2E | ✅ |

### Validation — amount rules (non-Buy/Sell)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 21 | Rejects zero Amount for Deposit | `TransactionType: Deposit, Amount: 0` | 400 `AMOUNT_REQUIRED` | E2E | ✅ |
| 22 | Rejects negative Amount for Deposit | `TransactionType: Deposit, Amount: -100` | 400 `AMOUNT_REQUIRED` | E2E | ✅ |
| 23 | Accepts valid Deposit | `Amount: 1000` | Passes | E2E | ✅ |
| 24 | Accepts valid Withdrawal | `Amount: 500` | Passes | E2E | ✅ |
| 25 | Accepts valid Interest | `Amount: 50` | Passes | E2E | ✅ |
| 26 | Accepts valid SelfInterest | `Amount: 50` | Passes | E2E | ✅ |
| 27 | Accepts valid Dividend | `Amount: 25` | Passes | E2E | ✅ |
| 28 | Accepts valid InterestPenalty | `Amount: 10` | Passes | E2E | ✅ |
| 29 | Deposit ignores Units/Price | `Deposit, Amount: 1000, Units: 5, Price: 100` | Passes; stored Units=0, Price=0 | E2E | ✅ |

### Validation — TransactionType per AssetType

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 30 | BankAccount + Deposit | BankAccount asset item, Deposit | Passes | E2E | ✅ |
| 31 | BankAccount + Withdrawal | BankAccount, Withdrawal | Passes | E2E | ✅ |
| 32 | BankAccount + Interest | BankAccount, Interest | Passes | E2E | ✅ |
| 33 | BankAccount + SelfInterest | BankAccount, SelfInterest | Passes | E2E | ✅ |
| 34 | BankAccount + InterestPenalty | BankAccount, InterestPenalty | Passes | E2E | ✅ |
| 35 | BankAccount + Buy | BankAccount, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 36 | BankAccount + Sell | BankAccount, Sell | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 37 | BankAccount + Dividend | BankAccount, Dividend | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 38 | FixedDeposit + Deposit | FixedDeposit, Deposit | Passes | E2E | ✅ |
| 39 | FixedDeposit + Interest | FixedDeposit, Interest | Passes | E2E | ✅ |
| 40 | FixedDeposit + SelfInterest | FixedDeposit, SelfInterest | Passes | E2E | ✅ |
| 41 | FixedDeposit + InterestPenalty | FixedDeposit, InterestPenalty | Passes | E2E | ✅ |
| 42 | FixedDeposit + Buy | FixedDeposit, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 43 | EPF + Deposit | EPF, Deposit | Passes | E2E | ✅ |
| 44 | EPF + Buy | EPF, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 45 | PPF + Deposit | PPF, Deposit | Passes | E2E | ✅ |
| 46 | PPF + Buy | PPF, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 47 | MutualFund + Buy | MutualFund, Buy | Passes | E2E | ✅ |
| 48 | MutualFund + Sell | MutualFund, Sell | Passes | E2E | ✅ |
| 49 | MutualFund + Deposit | MutualFund, Deposit | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 50 | MutualFund + Dividend | MutualFund, Dividend | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 51 | Stock + Buy | Stock, Buy | Passes | E2E | ✅ |
| 52 | Stock + Sell | Stock, Sell | Passes | E2E | ✅ |
| 53 | Stock + Dividend | Stock, Dividend | Passes | E2E | ✅ |
| 54 | Stock + Deposit | Stock, Deposit | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 55 | Wallet + Deposit | Wallet, Deposit | Passes | E2E | ✅ |
| 56 | Wallet + Withdrawal | Wallet, Withdrawal | Passes | E2E | ✅ |
| 57 | Wallet + Interest | Wallet, Interest | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 58 | TradingAccount + Deposit | TradingAccount, Deposit | Passes | E2E | ✅ |
| 59 | TradingAccount + Withdrawal | TradingAccount, Withdrawal | Passes | E2E | ✅ |
| 60 | TradingAccount + Buy | TradingAccount, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 61 | Bond + Deposit | Bond, Deposit | Passes | E2E | ✅ |
| 62 | Bond + Withdrawal | Bond, Withdrawal | Passes | E2E | ✅ |
| 63 | Bond + Interest | Bond, Interest | Passes | E2E | ✅ |
| 64 | Bond + Buy | Bond, Buy | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 65 | Bond + SelfInterest | Bond, SelfInterest | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |

> **Note:** An existing test `Returns_400_When_Buy_Invalid_For_Wallet` covers Wallet + Buy (invalid) which is not listed above but is an additional valid scenario.

### Validation — guard bypass

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 66 | Empty AssetItemId skips type check | `AssetItemId: Guid.Empty, TransactionType: Buy` | 400 `ASSET_ITEM_ID_REQUIRED` only (no type error) | E2E | ✅ |
| 67 | Unknown type skips type check | `TransactionType: Unknown, valid AssetItemId` | 400 `TRANSACTION_TYPE_REQUIRED` only | E2E | ✅ |
| 68 | Missing asset item skips type check | AssetItemId not in DB | Validator passes type check; handler returns 404 | E2E | ✅ |

### Handler

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 69 | Returns 404 for missing asset item | Valid request; AssetItemId not in DB | 404 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 70 | Creates Buy transaction | MutualFund, Buy, Units=10, Price=100 | 201; stored Units=10, Price=100, Amount=0 | E2E | ✅ |
| 71 | Creates Sell transaction | MutualFund, Sell, Units=5, Price=120 | 201; stored Units=5, Price=120, Amount=0 | E2E | ✅ |
| 72 | Creates Deposit transaction | BankAccount, Deposit, Amount=5000 | 201; stored Units=0, Price=0, Amount=5000 | E2E | ✅ |
| 73 | Creates Withdrawal transaction | BankAccount, Withdrawal, Amount=1000 | 201; stored Units=0, Price=0, Amount=1000 | E2E | ✅ |
| 74 | Response has zeroed amounts | Any valid creation | 201; response Units=0, Price=0, Amount=0 | E2E | ✅ |

---

## 11. GET /api/asset-items/{assetItemId}/transactions

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Returns 404 for missing asset item | Valid GUID; item not in DB | 404 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 2 | Returns empty list | Asset item exists; no transactions | 200 `[]` | E2E | ✅ |
| 3 | Returns transactions with calculated amounts | Asset item with multiple transactions; Currency=INR | 200 array with Amount calculated per transaction | E2E | ✅ |
| 4 | Cross-currency amount calculation | INR asset; Currency=USD query param | 200; amounts converted to USD | E2E | ✅ |
| 5 | Missing Currency defaults to Unknown | Currency query param absent | 200; amounts calculated with Unknown currency | E2E | ❌ |
| 6 | Rejects non-GUID route param | `/api/asset-items/not-a-guid/transactions` | 404 route not matched | E2E | ✅ |

---

## 12. GET /api/asset-items/{assetItemId}/transactions/{transactionId}

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Returns transaction with calculated amount | Valid IDs; transaction exists | 200 TransactionResponse | E2E | ✅ |
| 2 | Returns 404 for missing transaction | Valid asset item; transaction not in DB | 404 `TRANSACTION_NOT_FOUND` | E2E | ✅ |
| 3 | Cross-currency amount — Buy | INR MutualFund Buy; Currency=USD | 200; amount = units × price × exchange rate | E2E | ✅ |
| 4 | Cross-currency amount — Deposit | INR BankAccount Deposit; Currency=USD | 200; amount = deposit × exchange rate | E2E | ✅ |
| 5 | Same-currency amount unchanged | INR asset; Currency=INR | 200; amount unchanged | E2E | ✅ |
| 6 | Non-MF/Stock asset rate = 1 | BankAccount Buy; cross-currency | 200; units × 1 × exchange rate | E2E | ✅ |
| 7 | Missing Currency defaults to Unknown | Currency query param absent | 200 | E2E | ❌ |
| 8 | Rejects non-GUID transactionId | Non-GUID in route | 404 route not matched | E2E | ✅ |
| 9 | Missing transactionId hits list endpoint | `/api/asset-items/{id}/transactions/` | Matches GET list endpoint instead | E2E | ✅ |

---

## 13. PATCH /api/asset-items/{assetItemId}/transactions/{transactionId}

### Validation

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Rejects short Name | `Name: "ab"` (2 chars, non-whitespace) | 400 `NAME_TOO_SHORT` | E2E | ✅ |
| 2 | Rejects long Name | `Name: "a" × 1001` | 400 `NAME_TOO_LONG` | E2E | ✅ |
| 3 | Accepts empty/whitespace Name | `Name: ""` or key missing | Passes; uses existing name | E2E | ❌ |
| 4 | Rejects negative Units for Buy | `TransactionType: Buy, Units: -1` | 400 `UNITS_INVALID` | E2E | ✅ |
| 5 | Accepts zero Units for Buy | `TransactionType: Buy, Units: 0` | Passes; uses existing units | E2E | ❌ |
| 6 | Rejects negative Price for Buy | `TransactionType: Buy, Price: -1` | 400 `PRICE_INVALID` | E2E | ✅ |
| 7 | Accepts zero Price for Buy | `TransactionType: Buy, Price: 0` | Passes; uses existing price | E2E | ❌ |
| 8 | Rejects negative Amount for Deposit | `TransactionType: Deposit, Amount: -1` | 400 `AMOUNT_INVALID` | E2E | ✅ |
| 9 | Accepts zero Amount for Deposit | `TransactionType: Deposit, Amount: 0` | Passes; uses existing amount | E2E | ❌ |
| 10 | Rejects invalid type for asset | `MutualFund asset, TransactionType: Deposit` | 400 `TRANSACTION_TYPE_INVALID` | E2E | ✅ |
| 11 | Unknown type skips type check | `TransactionType: Unknown` | Passes type validation | E2E | ✅ |
| 12 | Rejects non-GUID route params | Non-GUID in assetItemId or transactionId | 404 route not matched | E2E | ✅ |

### Handler

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 13 | Returns 404 for missing asset item | AssetItemId not in DB | 404 `ASSET_ITEM_NOT_FOUND` | E2E | ✅ |
| 14 | Returns 404 for missing transaction | TransactionId not in DB | 404 `TRANSACTION_NOT_FOUND` | E2E | ✅ |

### Normalize — Name

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 15 | Empty Name preserves existing | `Name: ""` or key missing; existing="Old Name" | Transaction name = "Old Name" | E2E | ✅ |
| 16 | New Name replaces existing | `Name: "New Name"` | Transaction name = "New Name" | E2E | ✅ |

### Normalize — TransactionType

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 17 | Unknown type preserves existing | `TransactionType: Unknown`; existing=Deposit | Transaction type = Deposit | E2E | ✅ |
| 18 | New type replaces existing | `TransactionType: Withdrawal`; existing=Deposit | Transaction type = Withdrawal | E2E | ✅ |

### Normalize — amounts (non-Buy/Sell type)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 19 | Zero Amount preserves existing | `Amount: 0`; existing=5000 | Amount = 5000, Units=0, Price=0 | E2E | ✅ |
| 20 | New Amount replaces existing | `Amount: 3000`; existing=5000 | Amount = 3000, Units=0, Price=0 | E2E | ✅ |

### Normalize — amounts (Buy/Sell type)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 21 | Zero Units preserves existing | `Units: 0`; existing Units=10 | Units = 10 | E2E | ✅ |
| 22 | New Units replaces existing | `Units: 15`; existing Units=10 | Units = 15 | E2E | ✅ |
| 23 | Zero Price preserves existing | `Price: 0`; existing Price=100 | Price = 100 | E2E | ✅ |
| 24 | New Price replaces existing | `Price: 120`; existing Price=100 | Price = 120, Amount = 0 | E2E | ✅ |

### Empty body

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 25 | No body preserves all fields | No body or `{}`; existing transaction unchanged | 204; all fields preserved | E2E | ✅ |
| 26 | Valid edit returns 204 | Any valid partial update | 204 No Content | E2E | ✅ |

---

## 14. DELETE /api/asset-items/{assetItemId}/transactions/{transactionId}

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Deletes existing transaction | Valid IDs; transaction exists | 204 | E2E | ✅ |
| 2 | Returns 404 for missing transaction | Valid asset item; transaction not in DB | 404 `TRANSACTION_NOT_FOUND` | E2E | ✅ |
| 3 | Rejects non-GUID route params | Non-GUID in route | 404 route not matched | E2E | ✅ |

> **Note:** Existing test `Returns_500_When_Transaction_Does_Not_Exist` returns 500 (not 404) — potential bug in production code.

---

## 15. Cross-Cutting (E2E)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Auth required on all protected endpoints | Any protected endpoint without Authorization header | 401 | E2E | ✅ |
| 2 | Expired JWT rejected | Any protected endpoint with expired JWT | 401 | E2E | ❌ |
| 3 | Wrong audience rejected | JWT with wrong audience | 401 | E2E | ❌ |
| 4 | Wrong issuer rejected | JWT with wrong issuer | 401 | E2E | ❌ |
| 5 | Invalid signature rejected | Tampered JWT | 401 | E2E | ❌ |
| 6 | Enums serialized as strings | Any response with enum field | Enum rendered as string, not integer | E2E | ✅ |
| 7 | ProblemDetails format on validation error | Any validation failure | Response body is ProblemDetails with errorCode | E2E | ✅ |
| 8 | Multiple validation errors returned | Request triggering 2+ validation rules | All errors present in response | E2E | ✅ |
| 9 | User isolation | User A's JWT used to access User B's asset item | 404 (item not found for User A) | E2E | ✅ |

> **Note:** Auth tests exist for GET /api/asset-items (`Returns_401_When_Unauthenticated`), GET /api/asset-items/valuations, GET /api/users/me, and PATCH /api/users/me — covering #1 partially.

---

## 16. Domain Model — Entity\<TId\> (Unit)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Equals(Entity) — same Id | Two entities, same Id | `true` | Unit | ✅ |
| 2 | Equals(Entity) — different Id | Two entities, different Ids | `false` | Unit | ✅ |
| 3 | Equals(Entity) — null | Entity vs null | `false` | Unit | ✅ |
| 4 | Equals(object) — matching entity | Entity as object | `true` | Unit | ❌ |
| 5 | Equals(object) — non-entity | Entity vs random object | `false` | Unit | ✅ |
| 6 | Equals(object) — null | Entity vs null object | `false` | Unit | ❌ |
| 7 | == both non-null same Id | Same Id | `true` | Unit | ❌ |
| 8 | == both non-null different Id | Different Ids | `false` | Unit | ❌ |
| 9 | == both null | null == null | `true` | Unit | ❌ |
| 10 | == left null, right non-null | null == entity | `false` | Unit | ❌ |
| 11 | == left non-null, right null | entity == null | `false` | Unit | ❌ |
| 12 | != same Id | Same Id | `false` | Unit | ❌ |
| 13 | != different Id | Different Ids | `true` | Unit | ✅ |
| 14 | GetHashCode consistent | Same Id entities | Equal hash codes | Unit | ✅ |
| 15 | GetHashCode distinct | Different Id entities | Different hash codes | Unit | ✅ |

---

## 17. Domain Model — Empty Sentinels (Unit)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | User.Empty fields | `User.Empty` | Id=Empty, Email="", Currency=Unknown, Locale=Unknown | Unit | ✅ |
| 2 | Transaction.Empty fields | `Transaction.Empty` | Id=Empty, Date=MinValue, Name="", Type=Buy, Units=0, Price=0, Amount=0 | Unit | ✅ |
| 3 | AssetItem.Empty fields | `AssetItem.Empty` | Id=Empty, AssetId=Empty, Name="" | Unit | ✅ |
| 4 | Asset.Empty fields | `Asset.Empty` | Id=Empty, AssetClass=Unknown, AssetType=Unknown, Currency=Unknown | Unit | ✅ |

---

## 18. TransactionValidationExtensions (Unit)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | IsValidForAssetType with Unknown AssetType | `AssetType.Unknown`, any TransactionType | Throws `InvalidOperationException` | Unit | ❌ |

---

## 19. RatesExtensions.GetOnOrBeforeValue (Unit)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Exact date match | Dictionary has rate on requested date | Returns that rate | Unit | ✅ |
| 2 | 1-day lookback (weekend) | Rate exists on date-1 only | Returns rate for date-1 | Unit | ✅ |
| 3 | 6-day lookback (long holiday) | Rate exists on date-6 only | Returns rate for date-6 | Unit | ✅ |
| 4 | No rate in 7-day window | No rates within date to date-6 | Throws `InvalidOperationException` | Unit | ✅ |
| 5 | Empty dictionary | Empty rates dictionary | Throws `InvalidOperationException` | Unit | ✅ |

---

## 20. Infrastructure — API Clients (Integration)

### MutualFundApiClient

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | GetBySymbol — API 404 | Stub returns 404 | Empty MutualFund (SchemeCode="") | Integration | ❌ |
| 2 | GetBySymbol — valid response | Stub returns JSON with meta+data | MutualFund with Name, SchemeCode, Currency=INR | Integration | ❌ |
| 3 | GetPrices — API 404 | Stub returns 404 | Empty dictionary | Integration | ❌ |
| 4 | GetPrices — valid response | Stub returns JSON with date/NAV data | FrozenDictionary with parsed dd-MM-yyyy dates | Integration | ❌ |

> **Note:** `MutualFundApiClientTests` exists but only tests `GetOnOrBeforePriceAsync_ThrowsNotSupportedException`.

### StockApiClient

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 5 | GetBySymbol — API 404 | Stub returns 404 | Empty Stock (Symbol="") | Integration | ❌ |
| 6 | GetBySymbol — empty array | Stub returns `[]` | Empty Stock | Integration | ❌ |
| 7 | GetBySymbol — unrecognized currency | Stub returns `[{ Currency: "XYZ" }]` | Empty Stock | Integration | ❌ |
| 8 | GetBySymbol — valid response | Stub returns `[{ Symbol, Name, Currency: "USD" }]` | Stock with parsed fields | Integration | ❌ |
| 9 | GetPrices — API 404 | Stub returns 404 | Empty dictionary | Integration | ❌ |
| 10 | GetPrices — empty array | Stub returns `[]` | Empty dictionary | Integration | ❌ |
| 11 | GetPrices — valid response | Stub returns price array | FrozenDictionary with yyyy-MM-dd dates | Integration | ❌ |

> **Note:** `StockApiClientTests` exists but only tests `GetOnOrBeforePriceAsync_ThrowsNotSupportedException`.

### ExchangeRateApiClient

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 12 | GetExchangeRates — same currency | from=USD, to=USD | Empty dictionary (short-circuit) | Integration | ✅ |
| 13 | GetExchangeRates — valid CSV | Stub returns CSV with timestamp/close columns | FrozenDictionary using close column | Integration | ❌ |

---

## 21. Infrastructure — Caching (Integration)

### CachedExchangeRateApiClient

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | Same currency short-circuits (rates) | from=USD, to=USD | Returns empty dict without API/cache call | Integration | ✅ |
| 2 | Same currency short-circuits (on-or-before) | from=USD, to=USD | Returns 1m without API/cache call | Integration | ✅ |
| 3 | First call fetches and caches | from=INR, to=USD, first call | Fetches from API; subsequent call returns cached | Integration | ❌ |

### CachedAssetApiClient\<T\>

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 4 | GetBySymbol caches result | Same symbol called twice | Second call doesn't hit inner client | Integration | ❌ |
| 5 | GetPrices caches result | Same symbol called twice | Second call from cache | Integration | ❌ |
| 6 | GetOnOrBeforePrice caches | Same symbol+date called twice | Second call from cache | Integration | ❌ |

### CachedUserRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 7 | GetUser caches result | Same userId called twice | Second call from cache | Integration | ❌ |
| 8 | Update invalidates cache | Update then Get | Get returns fresh data | Integration | ❌ |

### CachedAssetItemRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 9 | GetAll caches list | Same userId called twice | Second call from cache | Integration | ❌ |
| 10 | Add invalidates list cache | Add then GetAll | GetAll returns updated list | Integration | ❌ |
| 11 | Delete invalidates list + item cache | Delete then GetAll + GetById | Both return updated data | Integration | ❌ |

### CachedTransactionRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 12 | GetByAssetItemId caches | Same IDs called twice | Second call from cache | Integration | ❌ |
| 13 | Add invalidates txn + valuation tags | Add then Get | Get returns updated list | Integration | ❌ |
| 14 | Update invalidates txn + valuation tags | Update then Get | Get returns updated data | Integration | ❌ |
| 15 | Delete invalidates txn + valuation tags | Delete then Get | Get returns updated list | Integration | ❌ |

---

## 22. Infrastructure — Repositories (Integration)

### UserIdRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | GetUserId — exists | Existing identity provider + ID | Returns UserId | Integration | ✅ |
| 2 | GetUserId — not found | Non-existent entry | Returns UserId.Empty | Integration | ✅ |
| 3 | AddUserId | New identity provider + ID | Inserts row; returns new UserId | Integration | ✅ |

### UserRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 4 | GetUser — exists | Existing userId | Returns User with all fields | Integration | ✅ |
| 5 | GetUser — not found | Non-existent userId | Returns User.Empty | Integration | ❌ |
| 6 | AddUser | New user data | Inserts with defaults (USD, EN_US) | Integration | ✅ |
| 7 | UpdateUserProfile | Existing userId, new currency/locale | Updates fields + UpdatedAt timestamp | Integration | ❌ |

### AssetRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 8 | GetById — exists | Existing assetId | Returns Asset | Integration | ❌ |
| 9 | GetById — not found | Non-existent assetId | Returns Asset.Empty | Integration | ❌ |
| 10 | GetByExternalId — exists | Existing externalId | Returns Asset | Integration | ❌ |
| 11 | GetByExternalId — not found | Non-existent externalId | Returns Asset.Empty | Integration | ❌ |
| 12 | Add | New asset data | Inserts row; returns Asset | Integration | ❌ |

### AssetItemRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 13 | GetAll — has items | UserId with items | Returns all items for user | Integration | ❌ |
| 14 | GetAll — empty | UserId with no items | Returns empty collection | Integration | ❌ |
| 15 | GetById — exists | Existing userId + assetItemId | Returns AssetItem | Integration | ❌ |
| 16 | GetById — not found | Non-existent IDs | Returns AssetItem.Empty | Integration | ❌ |
| 17 | Add | New item data | Inserts row; returns AssetItem | Integration | ❌ |
| 18 | Delete | Existing item | Deletes row | Integration | ❌ |

### TransactionRepository

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 19 | GetByAssetItemId — has transactions | AssetItemId with transactions | Returns all transactions | Integration | ❌ |
| 20 | GetByAssetItemId — empty | AssetItemId with no transactions | Returns empty collection | Integration | ❌ |
| 21 | GetById — exists | Existing IDs | Returns Transaction | Integration | ❌ |
| 22 | GetById — not found | Non-existent IDs | Returns Transaction.Empty | Integration | ❌ |
| 23 | Add | New transaction data | Inserts row; returns Transaction | Integration | ❌ |
| 24 | Update | Existing transaction | Updates all mutable fields + UpdatedAt | Integration | ❌ |
| 25 | Delete | Existing IDs | Deletes row | Integration | ❌ |

---

## 23. Database Schema Constraints (Integration)

| # | Test | Input | Expected | Type | Present |
|---|------|-------|----------|------|---------|
| 1 | User delete cascades to user_ids | Delete user row directly | user_ids rows deleted | Integration | ❌ |
| 2 | User delete cascades to asset_items | Delete user row directly | asset_items rows deleted | Integration | ❌ |
| 3 | User delete cascades to transactions | Delete user row directly | transactions rows deleted | Integration | ❌ |
| 4 | Asset item delete cascades to transactions | Delete asset_item row directly | transactions rows deleted | Integration | ❌ |
| 5 | Asset delete restricted when items exist | Delete asset with linked asset_items | FK violation error | Integration | ❌ |
| 6 | Duplicate email rejected | Insert two users with same email | Unique constraint violation | Integration | ❌ |
| 7 | Duplicate ExternalId rejected | Insert two assets with same ExternalId | Unique constraint violation | Integration | ❌ |

---

## Summary

| Section | Total | Present | Missing |
|---------|-------|---------|---------|
| 1. POST /api/auth/login/google | 10 | 5 | 5 |
| 2. POST /api/auth/refresh-token | 4 | 3 | 1 |
| 3. GET /api/users/me | 7 | 7 | 0 |
| 4. PATCH /api/users/me | 10 | 9 | 1 |
| 5. POST /api/asset-items | 45 | 43 | 2 |
| 6. GET /api/asset-items | 2 | 2 | 0 |
| 7. GET /api/asset-items/{id} | 3 | 3 | 0 |
| 8. DELETE /api/asset-items/{id} | 4 | 4 | 0 |
| 9. GET /api/asset-items/valuations | 37 | 27 | 10 |
| 10. POST .../transactions | 74 | 74 | 0 |
| 11. GET .../transactions | 6 | 5 | 1 |
| 12. GET .../transactions/{id} | 9 | 8 | 1 |
| 13. PATCH .../transactions/{id} | 26 | 22 | 4 |
| 14. DELETE .../transactions/{id} | 3 | 3 | 0 |
| 15. Cross-Cutting | 9 | 5 | 4 |
| 16. Entity\<TId\> (Unit) | 15 | 7 | 8 |
| 17. Empty Sentinels (Unit) | 4 | 4 | 0 |
| 18. TransactionValidationExtensions (Unit) | 1 | 0 | 1 |
| 19. RatesExtensions (Unit) | 5 | 5 | 0 |
| 20. API Clients (Integration) | 13 | 1 | 12 |
| 21. Caching (Integration) | 15 | 2 | 13 |
| 22. Repositories (Integration) | 25 | 5 | 20 |
| 23. DB Schema (Integration) | 7 | 0 | 7 |
| **Total** | **333** | **243** | **90** |
