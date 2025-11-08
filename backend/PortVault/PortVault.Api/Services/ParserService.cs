using OfficeOpenXml;
using PortVault.Api.Models;
using System.ComponentModel;

namespace PortVault.Api.Services
{
    public class ParserService : IParserService
    {
        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId)
        {
            // Use the correct EPPlusLicense method for non-commercial use.
            // You may use either SetNonCommercialPersonal or SetNonCommercialOrganization.
            // Example with personal use:
            new EPPlusLicense().SetNonCommercialPersonal("Your Name");

            using var pkg = new ExcelPackage(stream);
            var ws = pkg.Workbook.Worksheets[0];

            const int headerRow = 9;
            var list = new List<Transaction>();

            for (var r = headerRow + 1; r <= ws.Dimension.End.Row; r++)
            {
                var isin = ws.Cells[r, 2].GetValue<string>();
                if (string.IsNullOrWhiteSpace(isin)) continue;

                list.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    PortfolioId = portfolioId,
                    InstrumentId = isin,
                    Type = ws.Cells[r, 7].GetValue<string>()?.ToLower() ?? "",
                    Date = ws.Cells[r, 3].GetValue<DateTime>(),
                    Qty = ws.Cells[r, 9].GetValue<decimal>(),
                    Price = ws.Cells[r, 10].GetValue<decimal>()
                });
            }

            return list;
        }

    }
}
