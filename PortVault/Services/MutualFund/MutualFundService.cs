using PortVault.Models;
using PortVault.Repositories.MutualFund;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Services.MutualFund
{
    public class MutualFundService : IMutualFundService
    {
        private readonly IMutualFundRepository _repository;

        public MutualFundService(IMutualFundRepository fundRepository)
        {
            _repository = fundRepository;
        }

        public async Task EnsureFundsExistAsync()
        {
            int fundCount = await _repository.GetFundCountAsync();

            if (fundCount < 15)
            {
                Console.WriteLine("⚠️ Not enough data! Fetching new mutual fund data...");
                await FetchAndStoreAMFIData();
            }
            else
            {
                Console.WriteLine("✅ Mutual Fund data is already available.");
            }
        }

        public async Task<List<MutualFundModel>> GetFundsAsync()
        {
            return await _repository.GetFundsAsync();
        }

        public async Task<List<MutualFundModel>> SearchFundsAsync(string query)
        {
            return await _repository.SearchMutualFundsAsync(query);
        }
        //----------------------------------------------//
        private async Task FetchAndStoreAMFIData()
        {
            try
            {
                using HttpClient client = new();
                string url = "https://www.amfiindia.com/spages/NAVAll.TXT";
                string response = await client.GetStringAsync(url);

                var funds = ParseAMFIResponse(response);

                if (funds.Count == 0)
                {
                    Console.WriteLine("⚠ No valid mutual fund records found in AMFI data.");
                    return;
                }

                await _repository.ClearFundsAsync();
                await _repository.InsertFundsAsync(funds);

                Console.WriteLine($"✅ {funds.Count} Mutual Fund records updated.");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"❌ Network error while fetching AMFI data: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching or processing AMFI data: {ex.Message}");
            }
        }

        private List<MutualFundModel> ParseAMFIResponse(string data)
        {
            List<MutualFundModel> funds = new();
            string[] lines = data.Split('\n');

            foreach (string line in lines)
            {
                var parts = line.Split(';');

                if (parts.Length < 6 ||
                    !int.TryParse(parts[0], out int schemeCode) ||
                    !decimal.TryParse(parts[4], out decimal netAssetValue))
                {
                    continue;
                }

                funds.Add(new MutualFundModel
                {
                    SchemeCode = schemeCode,
                    ISINDivPayoutOrGrowth = (parts[1]?.Trim() == "-" ? null : parts[1]?.Trim()),
                    ISINDivReinvestment = (parts[2]?.Trim() == "-" ? null : parts[2]?.Trim()),
                    SchemeName = parts[3]?.Trim(),
                    NetAssetValue = netAssetValue,
                    NAVDate = DateTime.TryParseExact(parts[5]?.Trim(), "dd-MMM-yyyy",
                                      System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.None,
                                      out DateTime navDate)
                 ? navDate
                 : DateTime.MinValue // Fallback to an explicit invalid date
                });

            }

            return funds;
        }
    }
}
