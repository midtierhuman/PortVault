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
                var fund = await FetchAndParseAMFIDataAsync();
                await StoreAMFIDataAsync(fund);
            }
            else
            {
                Console.WriteLine("✅ Mutual Fund data is already available.");
            }
        }

        public async Task<List<MutualFundModel>> GetAllFundsAsync()
        {
            return await _repository.GetAllFundsAsync();
        }

        public async Task<List<MutualFundModel>> SearchFundsAsync(string query)
        {
            return await _repository.SearchMutualFundsAsync(query);
        }

      public async Task EnsureNavIsUpdated()
        {
            var lastNavUpdationTime = await _repository.GetLastNAVTimeAsync();
            var shouldUpdate = ShouldUpdate(Convert.ToDateTime(lastNavUpdationTime));
            if (shouldUpdate)
            {
                await BulkUpdateNAVsAsync();
            }
        } 

        public async Task BulkUpdateNAVsAsync()
        {
            var funds = await FetchAndParseAMFIDataAsync();
            await _repository.BulkUpdateNAVsAsync(funds);
        }
        public async Task BulkInsertNewFundsAsync()
        {
            var funds = await FetchAndParseAMFIDataAsync();
            await _repository.BulkInsertNewFundsAsync(funds);
        }
        public async Task ClearFundsAsync()
        {
            await _repository.ClearFundsAsync();
        }
        public async Task BulkInsertFundsAsync()
        {
            var funds = await FetchAndParseAMFIDataAsync();
            await _repository.InsertFundsAsync(funds);
        }

        //----------------------------------------------//

        private async Task<List<MutualFundModel>> FetchAndParseAMFIDataAsync()
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
                    return null;
                }

                return funds;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"❌ Network error while fetching AMFI data: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching or parsing AMFI data: {ex.Message}");
                return null;
            }
        }

        private async Task StoreAMFIDataAsync(List<MutualFundModel> funds)
        {
            try
            {
                if (funds.Count == 0)
                {
                    Console.WriteLine("⚠ No valid mutual fund records to store.");
                    return;
                }

                await _repository.ClearFundsAsync();
                await _repository.InsertFundsAsync(funds);

                Console.WriteLine($"✅ {funds.Count} Mutual Fund records updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing or storing AMFI data: {ex.Message}");
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
        private bool ShouldUpdate(DateTime lastNavUpdationTime)
        {
            lastNavUpdationTime = lastNavUpdationTime.AddHours(5).AddMinutes(30);
            DateTime currentTimeInIst = DateTime.UtcNow.AddHours(5).AddMinutes(30);

            int currentHourInIst = currentTimeInIst.Hour;
            DateTime currentDateInIst = currentTimeInIst.Date;

            if ((currentTimeInIst - lastNavUpdationTime).TotalHours >= 12)
            {
                return true;
            }

            if (lastNavUpdationTime.Hour < 10 && currentHourInIst >= 10 && lastNavUpdationTime.Date == currentDateInIst)
            {
                return true;
            }

            if (lastNavUpdationTime.Hour < 22 && currentHourInIst >= 22 && lastNavUpdationTime.Date == currentDateInIst)
            {
                return true;
            }

            if (lastNavUpdationTime.Date < currentDateInIst && currentHourInIst >= 10)
            {
                return true;
            }

            if (lastNavUpdationTime.Date < currentDateInIst && currentHourInIst >= 22)
            {
                return true; 
            }

            return false;
        }
    }
}
