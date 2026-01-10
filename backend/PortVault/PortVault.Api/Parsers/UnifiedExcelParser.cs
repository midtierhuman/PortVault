using OfficeOpenXml;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using System.Globalization;

namespace PortVault.Api.Parsers
{
    public class UnifiedExcelParser : ITradeParser
    {
        public string Provider => "unified";

        public IEnumerable<TransactionImportDto> Parse(Stream stream, Guid portfolioId, Guid userId, string? password = null)
        {
            ExcelPackage.License.SetNonCommercialOrganization("PortVault");

            var transactions = new List<TransactionImportDto>();

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

            var columnMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var colCount = worksheet.Dimension?.Columns ?? 0;

            for (int col = 1; col <= colCount; col++)
            {
                var header = GetCellValue(worksheet.Cells[1, col]);
                if (!string.IsNullOrWhiteSpace(header) && !columnMapping.ContainsKey(header))
                {
                    columnMapping[header] = col;
                }
            }

            var missingHeaders = expectedHeaders.Where(h => !columnMapping.ContainsKey(h)).ToList();
            if (missingHeaders.Any())
            {
                throw new InvalidOperationException($"Missing required headers: {string.Join(", ", missingHeaders)}");
            }
            
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            
            // Start from row 2
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var symbol = GetCellValue(worksheet.Cells[row, columnMapping["Symbol"]]);
                    var isin = GetCellValue(worksheet.Cells[row, columnMapping["ISIN"]]);
                    var tradeDateValue = GetCellValue(worksheet.Cells[row, columnMapping["Trade Date"]]);
                    var segment = GetCellValue(worksheet.Cells[row, columnMapping["Segment"]]);
                    var series = GetCellValue(worksheet.Cells[row, columnMapping["Series"]]);
                    var tradeTypeStr = GetCellValue(worksheet.Cells[row, columnMapping["Trade Type"]]);
                    var quantityValue = GetCellValue(worksheet.Cells[row, columnMapping["Quantity"]]);
                    var priceValue = GetCellValue(worksheet.Cells[row, columnMapping["Price"]]);
                    var executionTimeValue = GetCellValue(worksheet.Cells[row, columnMapping["Order Execution Time"]]);
                    var tradeIdValue = GetIdCellValue(worksheet.Cells[row, columnMapping["Trade ID"]]);
                    var orderIdValue = GetIdCellValue(worksheet.Cells[row, columnMapping["Order ID"]]);

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
                    // NOTE: Changed to strings to preserve full fidelity and avoid Excel float precision loss for long IDs
                    string? tradeId = string.IsNullOrWhiteSpace(tradeIdValue) ? null : tradeIdValue;
                    string? orderId = string.IsNullOrWhiteSpace(orderIdValue) ? null : orderIdValue;

                    // Use execution time if available, otherwise default to trade date at midnight
                    var effectiveTime = executionTime ?? tradeDate.Value.Date;

                    var transaction = new TransactionImportDto
                    {
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

        private static string GetCellValue(OfficeOpenXml.ExcelRange cell)
        {
            return cell.Value?.ToString()?.Trim() ?? string.Empty;
        }

        private static string GetIdCellValue(OfficeOpenXml.ExcelRange cell)
        {
            if (cell.Value == null) return string.Empty;

            if (cell.Value is double d)
            {
                // Format double to string without scientific notation
                // This preserves integers up to 15-17 digits which is typical for double precision
                // Using "0.#########################" ensures we get all available digits without E notation
                return d.ToString("0.#########################", CultureInfo.InvariantCulture);
            }

            return cell.Value.ToString()?.Trim() ?? string.Empty;
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

