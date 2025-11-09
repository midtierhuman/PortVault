using OfficeOpenXml;
using PortVault.Api.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace PortVault.Api.Parsers
{
    public sealed class CamsKfinCasParser : ITradeParser
    {
        public string Provider => "camskfin";

        public IEnumerable<Transaction> Parse(Stream stream, Guid portfolioId, string? pwd)
        {
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream, new ParsingOptions
            {
                Password = pwd
            });

            var lines = new List<string>();

            foreach (var page in pdf.GetPages())
            {
                var rows = page.Letters
                    .GroupBy(l => Math.Round(l.Location.Y, 0)) // double tolerance
                    .OrderByDescending(g => g.Key);

                foreach (var g in rows)
                {
                    var line = string.Concat(g.OrderBy(x => x.Location.X)
                                              .Select(x => x.Value));
                    var clean = line.Trim();
                    if (!string.IsNullOrWhiteSpace(clean))
                        lines.Add(clean);
                }
            }
            var filtered = lines
       .Where(x => x.Length >= 5)   // drop tiny fragments
       .ToList();

            return ParseLines(filtered, portfolioId);
        }

        private IEnumerable<Transaction> ParseLines(IEnumerable<string> lines, Guid pid)
        {
            var tx = new List<Transaction>();

            string isin = "";
            bool inBlock = false;

            var isinRe = new Regex(@"ISIN:\s*([A-Z0-9]{12})", RegexOptions.IgnoreCase);
            var dateRe = new Regex(@"^\d{2}-[A-Za-z]{3}-\d{4}", RegexOptions.IgnoreCase);

            foreach (var raw in lines)
            {
                var line = raw.Replace("\u00A0", " ").Trim();
                if (line.Length < 10) continue;
                if (line.Contains("Stamp Duty", StringComparison.OrdinalIgnoreCase)) continue;

                var mIsin = isinRe.Match(line);
                if (mIsin.Success)
                {
                    isin = mIsin.Groups[1].Value;
                    inBlock = false;
                    continue;
                }

                if (line.StartsWith("Opening Unit Balance", StringComparison.OrdinalIgnoreCase))
                {
                    inBlock = true;
                    continue;
                }

                if (line.StartsWith("Closing Unit Balance", StringComparison.OrdinalIgnoreCase))
                {
                    inBlock = false;
                    continue;
                }

                if (!inBlock || isin.Length == 0) continue;

                if (!dateRe.IsMatch(line)) continue;

                var compact = line.Replace(" ", "");
                if (compact.Length < 20) continue;

                var dateStr = compact[..11];
                var rest = compact[11..];

                var mType = Regex.Match(rest, @"^(Purchase|Redemption)", RegexOptions.IgnoreCase);
                if (!mType.Success) continue;

                var typeStr = mType.Value;
                var tail = rest[typeStr.Length..];

                var dt = DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
                var type = typeStr.StartsWith("P", StringComparison.OrdinalIgnoreCase) ? TradeType.Buy : TradeType.Sell;

                var (amt, units, price, balance) = SplitTail(tail);

                tx.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    PortfolioId = pid,
                    InstrumentId = isin,
                    TradeType = type,
                    Date = dt,
                    Price = price,
                    Qty = units
                });

            }

            return tx;
        }

        private static (decimal amount, decimal units, decimal price, decimal balance) SplitTail(string raw)
        {
            raw = raw.Replace(",", "").Trim();

            decimal pull(string pat, ref string s)
            {
                var m = Regex.Match(s, pat);
                if (!m.Success) return 0m;
                var v = m.Groups["v"].Value;
                s = s[..^v.Length];
                return decimal.Parse(v, CultureInfo.InvariantCulture);
            }

            var balance = pull(@"(?<v>\d+\.\d{3})$", ref raw);
            var price = pull(@"(?<v>\d+\.\d{2})$", ref raw);
            var units = pull(@"(?<v>\d+\.\d{3})$", ref raw);

            decimal amt = 0;
            decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out amt);
            return (amt, units, price, balance);
        }


    }


}

