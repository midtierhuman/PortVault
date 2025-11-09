using OfficeOpenXml;
using PortVault.Api.Models;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace PortVault.Api.Parsers
{
    public class CamsKfinCasParser : ITradeParser
    {
        public string Provider => "camskfin";

        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId, string? pwd)
        {
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream, new ParsingOptions
            {
                Password = pwd
            });

            var raw = new StringBuilder();
            foreach (var page in pdf.GetPages())
                raw.AppendLine(page.Text);

            var txt = raw.ToString();
            return ParseText(txt, portfolioId);
        }

        private IEnumerable<Transaction> ParseText(string txt, Guid portfolioId)
        {
            // TODO: see below: regex loop
            throw new NotImplementedException();
        }
    }
}

