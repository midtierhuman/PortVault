using OfficeOpenXml;
using PortVault.Api.Models;

namespace PortVault.Api.Parsers
{
    public class CamsKfinCasParser
    {
        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId)
        {
            new EPPlusLicense().SetNonCommercialPersonal("Subhadip"); // or whatever string u want

            using var pkg = new ExcelPackage(stream);
            var ws = pkg.Workbook.Worksheets[0];

            const int headerRow = 15;
            var list = new List<Transaction>();

            for (var r = headerRow + 1; r <= ws.Dimension.End.Row; r++)
            {
                var isin = ws.Cells[r, 3].Text;
                var tradeDate = ws.Cells[r, 4].GetValue<DateTime>();
                var tradeType = ws.Cells[r, 8].Text?.ToLower();
                var qty = ws.Cells[r, 10].GetValue<decimal>();
                var price = ws.Cells[r, 11].GetValue<decimal>();
                var tradeId = ws.Cells[r, 12].GetValue<int>();

                if (string.IsNullOrWhiteSpace(isin) || string.IsNullOrWhiteSpace(tradeType)) break;

                list.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    PortfolioId = portfolioId,
                    InstrumentId = isin,
                    Date = tradeDate,
                    TradeType = tradeType == "buy" ? TradeType.Buy : TradeType.Sell,
                    Qty = qty,
                    Price = price,
                    TradeId = tradeId
                });
            }

            return list;
        }
    }
}
