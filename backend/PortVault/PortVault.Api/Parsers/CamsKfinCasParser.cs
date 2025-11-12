using OfficeOpenXml; // (This 'using' seems unused for PDF parsing, but left it as it was in your original)
using PortVault.Api.Models; // (Assuming Transaction and TradeType are in here)
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
                // Use a tolerance to group letters that are "close enough" vertically
                int tolerance = 2;
                var rows = page.Letters
                    .GroupBy(l => Math.Round(l.Location.Y / tolerance, 0))
                    .OrderByDescending(g => g.Key);

                foreach (var g in rows)
                {
                    // Rebuild the line with spaces based on horizontal distance
                    var orderedLetters = g.OrderBy(x => x.Location.X).ToList();
                    var sb = new StringBuilder();

                    for (int i = 0; i < orderedLetters.Count; i++)
                    {
                        var current = orderedLetters[i];
                        sb.Append(current.Value);

                        // If there's a next letter, check the gap
                        if (i + 1 < orderedLetters.Count)
                        {
                            var next = orderedLetters[i + 1];
                            double gap = next.Location.X - current.EndBaseLine.X;

                            // If gap is larger than a small threshold, insert a space
                            if (gap > 1.0)
                            {
                                sb.Append(" ");
                            }
                        }
                    }

                    var line = sb.ToString();
                    var clean = line.Trim();
                    if (!string.IsNullOrWhiteSpace(clean))
                        lines.Add(clean);
                }
            }

            var filtered = lines
                .Where(x => x.Length >= 5) // drop tiny fragments
                .ToList();

            return ParseLines(filtered, portfolioId);
        }

        private IEnumerable<Transaction> ParseLines(IEnumerable<string> lines, Guid pid)
        {
            var tx = new List<Transaction>();

            string isin = "";
            bool inBlock = false;

            var isinRe = new Regex(@"ISIN:\s*([A-Z0-9]{12})", RegexOptions.IgnoreCase);

            // This is the new, robust regex to parse the entire transaction line
            var txnRe = new Regex(
                @"^(?<date>\d{2}-[A-Za-z]{3}-\d{4})\s+(?<type>Purchase|Redemption).*?(?<tail>[\d,\.\(\)-]+\s+[\d,\.\(\)-]+\s+[\d,\.\(\)-]+(?:\s+[\d,\.\(\)-]+)?)\s*$",
                RegexOptions.IgnoreCase
            );

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

                // Use the new regex to match the line
                var m = txnRe.Match(line);
                if (!m.Success) continue;

                // Extract data from named regex groups
                var dateStr = m.Groups["date"].Value;
                var typeStr = m.Groups["type"].Value;
                var tailStr = m.Groups["tail"].Value;

                var dt = DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
                var type = typeStr.StartsWith("P", StringComparison.OrdinalIgnoreCase) ? TradeType.Buy : TradeType.Sell;

                // SplitTail will find the numbers in the "tail" string
                var (amt, units, price, balance) = SplitTail(tailStr);

                // Add only transactions with valid (non-zero) data
                if (amt != 0 || units != 0 || price != 0)
                {
                    tx.Add(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        PortfolioId = pid,
                        InstrumentId = isin,
                        TradeType = type,
                        Date = dt,
                        Price = price,
                        Qty = units
                        // Note: We use Qty (units) and Price. 
                        // The 'amt' (Amount) is often Qty * Price,
                        // so storing Qty and Price is standard.
                    });
                }
            }

            return tx;
        }

        private static (decimal amt, decimal units, decimal price, decimal balance) SplitTail(string s)
        {
            // This regex finds all number-like tokens in the tail string
            var toks = Regex.Matches(s, @"\(?-?[\d,]+(\.\d+)?\)?")
                            .Select(m => m.Value)
                            .ToList();

            if (toks.Count < 3) return (0, 0, 0, 0);

            // [0]=amount
            // [1]=units
            // [2]=price
            // [3]=balance (optional)
            decimal amt = ParseDecimalSafe(toks[0]);
            decimal units = ParseDecimalSafe(toks[1]);
            decimal price = ParseDecimalSafe(toks[2]);
            decimal bal = toks.Count > 3 ? ParseDecimalSafe(toks[3]) : 0;

            return (amt, units, price, bal);
        }

        private static decimal ParseDecimalSafe(string s)
        {
            var neg = s.StartsWith("(") && s.EndsWith(")");
            var clean = s.Replace("(", "").Replace(")", "").Replace(",", "");
            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return neg ? -d : d;
            return 0;
        }
    }
}