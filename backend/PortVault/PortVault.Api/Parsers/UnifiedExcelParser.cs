using OfficeOpenXml;
using PortVault.Api.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PortVault.Api.Parsers
{
    public class UnifiedExcelParser : ITradeParser
    {
        public string Provider => "unified";

        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId, Guid userId, string? password = null)
        {
            ExcelPackage.License.SetNonCommercialOrganization("PortVault");

            var transactions = new List<Transaction>();

            using var package = new ExcelPackage(stream);

            // Validate Sheet Name
            var worksheet = package.Workbook.Worksheets["Transactions"];
            if (worksheet == null)
            {
                throw new InvalidOperationException("The Excel file must contain a worksheet named 'Transactions'.");
            }

            // Validate Headers
            var expectedHeaders = new[] 
            { 
                "Symbol", "ISIN", "Trade Date", "Segment", "Series", 
                "Trade Type", "Quantity", "Price", "Order Execution Time",
                "Trade ID", "Order ID"
            };

            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                var header = GetCellValue(worksheet.Cells[1, i + 1]);
                if (!string.Equals(header, expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Invalid header in column {i + 1}. Expected '{expectedHeaders[i]}', found '{header}'.");
                }
            }
            
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            
            // Start from row 2
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
                    var tradeIdValue = GetCellValue(worksheet.Cells[row, 10]);
                    var orderIdValue = GetCellValue(worksheet.Cells[row, 11]);

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

                    // Parse TradeID and OrderID as nullable
                    long? tradeId = null;
                    long? orderId = null;
                    
                    if (!string.IsNullOrEmpty(tradeIdValue) && long.TryParse(tradeIdValue, out var tid))
                        tradeId = tid;
                    
                    if (!string.IsNullOrEmpty(orderIdValue) && long.TryParse(orderIdValue, out var oid))
                        orderId = oid;

                    // Use execution time if available, otherwise default to trade date at midnight
                    var effectiveTime = executionTime ?? tradeDate.Value.Date;

                    // Generate deterministic ID based on trade data
                    // We use a pipe separator to ensure structure is preserved even when optional fields are null
                    var rawIdData = $"{portfolioId}|{isin}|{tradeDate.Value:yyyyMMdd}|{tradeType}|{quantity}|{price}|{tradeId}|{orderId}|{effectiveTime:yyyyMMddHHmmss}";
                    var id = GenerateDeterministicGuid(rawIdData);

                    var transaction = new Transaction
                    {
                        Id = id,
                        PortfolioId = portfolioId,
                        Symbol = symbol,
                        ISIN = isin,
                        TradeDate = tradeDate.Value,
                        OrderExecutionTime = executionTime,
                        Segment = segment,
                        Series = series,
                        TradeType = tradeType,
                        Quantity = quantity,
                        Price = price,
                        TradeID = tradeId,
                        OrderID = orderId
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

        private static Guid GenerateDeterministicGuid(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes);
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

