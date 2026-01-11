# Add Single Transaction Endpoint

## Overview
You can now add individual transactions manually via API without needing to upload an Excel file.

## Endpoint

### Add Single Transaction
```http
POST /api/portfolio/{portfolioName}/transactions
Authorization: Bearer {user-token}
Content-Type: application/json
```

### Request Body
```json
{
  "symbol": "TATASTEEL",
  "isin": "INE081A01020",
  "tradeDate": "2024-01-15T00:00:00Z",
  "orderExecutionTime": "2024-01-15T10:30:00Z",
  "segment": "EQ",
  "series": "EQ",
  "tradeType": "Buy",
  "quantity": 100,
  "price": 150.50,
  "tradeID": "12345",
  "orderID": "67890"
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `symbol` | string | Yes | Stock symbol (1-100 chars) |
| `isin` | string | Yes | ISIN identifier (1-50 chars) |
| `tradeDate` | DateTime | Yes | Date of the trade |
| `orderExecutionTime` | DateTime | No | Exact time of execution |
| `segment` | string | Yes | Market segment (e.g., "EQ", "MF") |
| `series` | string | No | Series (e.g., "EQ", "BE") |
| `tradeType` | string | Yes | "Buy" or "Sell" |
| `quantity` | decimal | Yes | Number of shares/units |
| `price` | decimal | Yes | Price per share/unit |
| `tradeID` | string | No | Trade ID (max 100 chars) |
| `orderID` | string | No | Order ID (max 100 chars) |

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Transaction added successfully",
  "data": {
    "addedCount": 1
  }
}
```

### Error Responses

#### 400 Bad Request - Invalid Data
```json
{
  "success": false,
  "message": "Invalid transaction data",
  "data": null
}
```

#### 400 Bad Request - Invalid Trade Type
```json
{
  "success": false,
  "message": "Invalid trade type. Valid types are: Buy, Sell",
  "data": null
}
```

#### 404 Not Found - Portfolio Not Found
```json
{
  "success": false,
  "message": "Portfolio 'MyPortfolio' not found",
  "data": null
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "Unauthorized access",
  "data": null
}
```

## Behavior

### Automatic Actions After Adding Transaction
1. **Instrument Lookup/Creation**: 
   - System checks if an instrument with the given ISIN exists
   - If not found, creates a new instrument with the symbol as the name
   
2. **Holdings Recalculation**:
   - Automatically recalculates portfolio holdings
   - Applies corporate action adjustments if any exist
   - Updates portfolio invested and current values

3. **Validation**:
   - Validates all required fields
   - Checks that quantity and price are positive
   - Ensures trade type is either "Buy" or "Sell"

## Example Usage

### cURL Example
```bash
curl -X POST "https://api.portvault.com/api/portfolio/MyPortfolio/transactions" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "TATASTEEL",
    "isin": "INE081A01020",
    "tradeDate": "2024-01-15T00:00:00Z",
    "segment": "EQ",
    "series": "EQ",
    "tradeType": "Buy",
    "quantity": 100,
    "price": 150.50
  }'
```

### JavaScript/TypeScript Example
```typescript
const response = await fetch('/api/portfolio/MyPortfolio/transactions', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    symbol: "TATASTEEL",
    isin: "INE081A01020",
    tradeDate: "2024-01-15T00:00:00Z",
    segment: "EQ",
    series: "EQ",
    tradeType: "Buy",
    quantity: 100,
    price: 150.50
  })
});

const result = await response.json();
console.log(result);
```

### C# Example
```csharp
var request = new CreateTransactionRequest
{
    Symbol = "TATASTEEL",
    ISIN = "INE081A01020",
    TradeDate = DateTime.Parse("2024-01-15"),
    Segment = "EQ",
    Series = "EQ",
    TradeType = "Buy",
    Quantity = 100,
    Price = 150.50m
};

var response = await httpClient.PostAsJsonAsync(
    "/api/portfolio/MyPortfolio/transactions", 
    request
);
```

## Use Cases

? **Manual Transaction Entry**: Add trades that aren't in broker statements  
? **Quick Corrections**: Fix individual transactions without re-uploading files  
? **Mobile App Integration**: Allow users to add transactions from mobile devices  
? **API Integrations**: Enable third-party apps to add transactions  
? **Testing**: Quickly add test data during development  
? **Historical Data Entry**: Backfill old transactions one at a time  

## Related Endpoints

- `GET /api/portfolio/{name}/transactions` - List all transactions
- `DELETE /api/portfolio/{name}/transactions/{id}` - Delete a transaction
- `DELETE /api/portfolio/{name}/transactions/all?confirm=true` - Clear all transactions
- `POST /api/portfolio/{name}/transactions/upload` - Bulk upload via Excel
- `PUT /api/portfolio/{name}/holdings/recalculate` - Manually recalculate holdings

## Notes

- The endpoint automatically recalculates holdings after adding the transaction
- Corporate action adjustments are applied during recalculation
- The ISIN must be valid; the system will create a new instrument if needed
- You cannot add duplicate transactions (same trade details)
- Trade date can be in the past for historical data entry
