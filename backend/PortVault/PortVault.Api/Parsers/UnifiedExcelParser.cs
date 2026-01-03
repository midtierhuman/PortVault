using OfficeOpenXml;
using PortVault.Api.Models;
using System.Globalization;

namespace PortVault.Api.Parsers
{
    public class UnifiedExcelParser : ITradeParser
    {
        public string Provider => "unified";

        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId, string? password = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var transactions = new List<Transaction>();

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            
            // Start from row 2 (row 1 contains headers: Symbol, ISIN, Trade Date, Segment, Series, Trade Type, Quantity, Price, Order Execution Time)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var symbol = GetCellValue(worksheet.Cells[row, 1]);
                    var isin = GetCellValue(worksheet.Cells[row, 2]);
                    var tradeDateValue = GetCellValue(worksheet.Cells[row, 3]);
                    var segment = GetCellValue(worksheet.Cells[row, 4]);
                    var series = GetCellValue(worksheet.Cells[row, 5]);
                    var tradeTypeStr = GetCellValue(worksheet.Cells[row, 6]);
                    var quantityValue = GetCellValue(worksheet.Cells[row, 7]);
                    var priceValue = GetCellValue(worksheet.Cells[row, 8]);
                    var executionTimeValue = GetCellValue(worksheet.Cells[row, 9]);

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(isin) || string.IsNullOrWhiteSpace(tradeDateValue))
                        continue;

                    // Parse trade date
                    var tradeDate = ParseDate(tradeDateValue);
                    if (!tradeDate.HasValue)
                        continue;

                    // Parse execution time (optional)
                    DateTime? executionTime = ParseDateTime(executionTimeValue);

                    // Parse trade type
                    var tradeType = tradeTypeStr.ToLower() switch
                    {
                        "buy" => TradeType.Buy,
                        "sell" => TradeType.Sell,
                        _ => TradeType.Buy
                    };

                    // Parse quantity and price
                    if (!decimal.TryParse(quantityValue.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal quantity))
                        continue;

                    if (!decimal.TryParse(priceValue.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        continue;

                    var transactionHash = Transaction.GenerateTransactionHash(
                        isin, 
                        tradeDate.Value, 
                        executionTime, 
                        price, 
                        tradeType, 
                        quantity
                    );

                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionHash = transactionHash,
                        PortfolioId = portfolioId,
                        Symbol = symbol,
                        ISIN = isin,
                        TradeDate = tradeDate.Value,
                        OrderExecutionTime = executionTime,
                        Segment = segment,
                        Series = series,
                        TradeType = tradeType,
                        Quantity = quantity,
                        Price = price
                    };

                    transactions.Add(transaction);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return transactions;
        }

        private static string GetCellValue(OfficeOpenXml.ExcelRange cell)
        {
            return cell.Value?.ToString()?.Trim() ?? string.Empty;
        }

        private static DateTime? ParseDate(string dateValue)
        {
            if (string.IsNullOrWhiteSpace(dateValue))
                return null;

            // Try parsing as formatted date
            if (DateTime.TryParseExact(dateValue, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" }, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            // Try parsing as Excel date serial number
            if (double.TryParse(dateValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double excelDate))
            {
                return DateTime.FromOADate(excelDate);
            }

            return null;
        }

        private static DateTime? ParseDateTime(string dateTimeValue)
        {
            if (string.IsNullOrWhiteSpace(dateTimeValue))
                return null;

            // Try parsing as formatted datetime
            if (DateTime.TryParseExact(dateTimeValue, 
                new[] { "dd/MM/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss", "dd/MM/yyyy H:mm:ss", "d/M/yyyy HH:mm:ss",
                        "dd-MM-yyyy HH:mm:ss", "d-M-yyyy H:mm:ss" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                return dateTime;
            }

            // Try parsing as Excel date serial number
            if (double.TryParse(dateTimeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double excelDate))
            {
                return DateTime.FromOADate(excelDate);
            }

            return null;
        }
    }
}
