# Transaction Schema Update Summary

## Overview
Updated the PortVault API to handle unified Excel file uploads containing both equity (EQ) and mutual fund (MF) transactions. The system now uses ISIN as the primary identifier and generates unique transaction hashes to prevent duplicates.

## Changes Made

### 1. **Transaction Model Updates** (`Models/Transaction.cs`)
- **Removed Fields:**
  - `TradeId` (int) - Replaced with `TransactionHash`

- **Added Fields:**
  - `TransactionHash` (string) - SHA256 hash generated from: ISIN + TradeDate + ExecutionTime + Price + TradeType + Quantity
  - `Symbol` (string) - Stock/MF symbol
  - `ISIN` (string) - Primary identifier for the asset
  - `TradeDate` (DateTime) - Date of the trade
  - `OrderExecutionTime` (DateTime?) - Optional execution timestamp
  - `Segment` (string) - Market segment (EQ, MF, etc.)
  - `Series` (string) - Series type (A, EQ, etc.)

- **Backward Compatibility Properties:**
  - `InstrumentId` ? Returns `ISIN`
  - `Date` ? Returns `TradeDate`
  - `Qty` ? Returns `Quantity`

- **New Method:**
  - `GenerateTransactionHash()` - Static method to create unique transaction identifiers

### 2. **New Unified Excel Parser** (`Parsers/UnifiedExcelParser.cs`)
- **Provider Name:** `"unified"`
- **Expected Excel Format:**
  - Column 1: Symbol
  - Column 2: ISIN
  - Column 3: Trade Date (dd/MM/yyyy)
  - Column 4: Segment
  - Column 5: Series
  - Column 6: Trade Type (buy/sell)
  - Column 7: Quantity
  - Column 8: Price
  - Column 9: Order Execution Time (dd/MM/yyyy HH:mm:ss)

- **Features:**
  - Handles commas in numeric values
  - Supports both date formats (string and Excel serial)
  - Generates unique transaction hash for duplicate detection
  - Skips invalid/empty rows gracefully

### 3. **Updated Existing Parsers**

#### ZerodhaParser
- Updated to use new Transaction model fields
- Generates transaction hash
- Sets Segment to "EQ"

#### CamsKfinCasParser
- Updated to use new Transaction model fields
- Generates transaction hash
- Sets Segment to "MF"

### 4. **Repository Updates** (`Repositories/PortfolioRepository.cs`)
- Changed duplicate detection from `TradeId` to `TransactionHash`
- Uses `ToHashSet()` for efficient duplicate checking

### 5. **Controller Updates** (`Controllers/PortfolioController.cs`)
- **New Route:** `POST /api/portfolio/{portfolioId:guid}/transactions/upload`
- **Features:**
  - Requires portfolio ID in URL
  - Validates file extension (.xlsx, .xls)
  - Verifies portfolio exists before processing
  - Returns detailed response with:
    - Total transactions processed
    - New transactions added
    - Duplicates skipped
  - Better error handling with detailed error messages

### 6. **Program.cs Updates**
- Registered `UnifiedExcelParser` in DI container
- Existing automatic migration logic remains active

### 7. **Database Migration**
- **Migration Name:** `UpdateTransactionSchema`
- **Location:** `Migrations/20260103060452_UpdateTransactionSchema.cs`

#### Schema Changes:
```sql
-- Removed
DROP COLUMN TradeId (int)

-- Renamed
Qty ? Quantity
InstrumentId ? TransactionHash
Date ? TradeDate

-- Added
ISIN (nvarchar(max), NOT NULL)
Symbol (nvarchar(max), NOT NULL)
Segment (nvarchar(max), NOT NULL)
Series (nvarchar(max), NOT NULL)
OrderExecutionTime (datetime2, NULL)
```

## How to Use

### 1. **Start the Application**
The database will automatically be updated with the new schema on startup (via `context.Database.Migrate()`).

### 2. **Upload Transactions**
**Endpoint:** `POST /api/portfolio/{portfolioId}/transactions/upload`

**Example using Swagger:**
1. Navigate to `/swagger`
2. Find the upload endpoint
3. Enter your portfolio GUID
4. Upload your Excel file

**Example using cURL:**
```bash
curl -X POST "https://localhost:7061/api/portfolio/{YOUR-PORTFOLIO-ID}/transactions/upload" \
  -H "accept: application/json" \
  -F "file=@your-transactions.xlsx"
```

**Response Example:**
```json
{
  "message": "Successfully processed 10 new transactions.",
  "totalProcessed": 15,
  "newTransactions": 10,
  "duplicatesSkipped": 5
}
```

### 3. **Excel File Format**
Create an Excel file with these columns (first row should be headers):

| Symbol | ISIN | Trade Date | Segment | Series | Trade Type | Quantity | Price | Order Execution Time |
|--------|------|------------|---------|--------|------------|----------|-------|---------------------|
| HDFCAMC | INE127D01025 | 28/07/2023 | EQ | A | sell | 3.00 | 2,529.55 | 28/07/2023 12:27:06 |
| ITBEES | INF204KB15V2 | 28/07/2023 | EQ | EQ | buy | 457.00 | 31.02 | 28/07/2023 13:14:57 |

## Key Features

### Duplicate Prevention
- Transactions are uniquely identified by a SHA256 hash of:
  - ISIN
  - Trade Date
  - Order Execution Time
  - Price
  - Trade Type
  - Quantity
- Re-uploading the same file won't create duplicates

### ISIN-Based Asset Tracking
- ISIN is now the primary identifier
- Symbol is stored but not used for uniqueness
- Works across different data sources that might use different names for the same asset

### Backward Compatibility
- Old parsers (Zerodha, CamsKfin) still work
- Existing code using `InstrumentId`, `Date`, `Qty` properties will continue to work

## Testing Checklist

- [x] Build succeeds
- [x] Migration created successfully
- [ ] Test Excel upload with sample data
- [ ] Verify duplicate detection works
- [ ] Check Holdings recalculation works with new schema
- [ ] Test with both EQ and MF transactions

## Next Steps (Optional Improvements)

1. **Add Index on TransactionHash** for faster duplicate detection
2. **Add Index on ISIN** for faster holdings calculation
3. **Add Validation** for ISIN format (should be 12 characters)
4. **Add Asset Auto-Creation** - Create Asset records from transactions if they don't exist
5. **Add Transaction History Endpoint** to view transactions by portfolio
6. **Add Excel Template Download** endpoint for users

## Files Modified

1. `PortVault.Api\Models\Transaction.cs`
2. `PortVault.Api\Parsers\UnifiedExcelParser.cs` (NEW)
3. `PortVault.Api\Parsers\ZerodhaParser.cs`
4. `PortVault.Api\Parsers\CamsKfinCasParser.cs`
5. `PortVault.Api\Repositories\PortfolioRepository.cs`
6. `PortVault.Api\Controllers\PortfolioController.cs`
7. `PortVault.Api\Program.cs`
8. `PortVault.Api\Migrations\20260103060452_UpdateTransactionSchema.cs` (NEW)

## Breaking Changes

?? **Warning:** This is a breaking change for existing databases with transaction data.

- The `TradeId` field has been removed
- The `InstrumentId` column is now `TransactionHash`
- Existing transactions will need their data migrated if you have production data

If you have existing production data, you may need to create a data migration script to:
1. Populate the new fields (ISIN, Symbol, etc.) from existing InstrumentId
2. Generate TransactionHash for existing records
3. Update any external references

---

**Migration Applied:** ? (Will apply automatically on next app start)
**Build Status:** ? Successful
**Ready for Testing:** ? Yes
