# Transaction Upload Template

## Excel File Format

Upload your transactions using an Excel file (.xlsx or .xls) with the following column headers in the first row:

| Column | Header Name | Description | Format | Required | Example |
|--------|-------------|-------------|---------|----------|---------|
| A | Symbol | Stock/Fund symbol or name | Text | Yes | HDFCBANK |
| B | ISIN | International Securities Identification Number | Text (12 chars) | Yes | INE040A01034 |
| C | Trade Date | Date when the trade occurred | dd/MM/yyyy | Yes | 31/07/2023 |
| D | Segment | Market segment | Text | Yes | EQ, MF |
| E | Series | Series type | Text | No | A, EQ |
| F | Trade Type | Type of transaction | Text | Yes | buy, sell |
| G | Quantity | Number of units | Decimal | Yes | 2.00, 128.40 |
| H | Price | Price per unit | Decimal | Yes | 1,641.95, 7.79 |
| I | Order Execution Time | Exact time of order execution | dd/MM/yyyy HH:mm:ss | No | 31/07/2023 09:19:31 |

## Sample Data

```
Symbol                                                      ISIN            Trade Date  Segment Series  Trade Type  Quantity    Price       Order Execution Time
HDFCBANK                                                    INE040A01034    31/07/2023  EQ      A       buy         2.00        1,641.95    31/07/2023 09:19:31
ITBEES                                                      INF204KB15V2    31/07/2023  EQ      EQ      buy         7.00        31.14       31/07/2023 09:19:56
ICICI PRUDENTIAL NASDAQ 100 INDEX FUND - DIRECT PLAN        INF109KC1U50    26/12/2022  MF              Buy         128.40      7.79        
ICICI PRUDENTIAL NASDAQ 100 INDEX FUND - DIRECT PLAN        INF109KC1U50    26/12/2022  MF              Buy         256.81      7.79        
```

## Important Notes

1. **Headers**: First row must contain the exact column headers as specified above
2. **Date Format**: Use DD/MM/YYYY format (e.g., 31/07/2023)
3. **DateTime Format**: Use DD/MM/YYYY HH:mm:ss format (e.g., 31/07/2023 09:19:31)
4. **Trade Type**: Case-insensitive, accepts "buy" or "sell"
5. **Numbers**: Can include commas for thousands separator (e.g., 1,641.95)
6. **Decimals**: Use period (.) as decimal separator
7. **Required Fields**: ISIN, Trade Date, Trade Type, Quantity, and Price are mandatory
8. **Duplicate Prevention**: The system automatically prevents duplicate transactions using a hash of key fields

## Supported Segments

- **EQ**: Equity (Stocks)
- **MF**: Mutual Funds
- Any other custom segment type

## Upload Endpoint

`POST /api/portfolio/{portfolioId}/transactions/upload`

The API will:
- Validate the Excel file
- Parse all transactions
- Skip duplicate transactions automatically
- Return a summary of processed transactions
