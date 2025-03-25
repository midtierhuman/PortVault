using Dapper;
using PortVault.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Repositories.MutualFund
{
    internal class MutualFundRepository : IMutualFundRepository
    {

        private readonly DBHelper _dbHelper;

        public MutualFundRepository(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }
        public async Task<int> GetFundCountAsync()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            const string query = "SELECT COUNT(*) FROM MutualFunds;";
            return await connection.ExecuteScalarAsync<int>(query);
        }
        public async Task<List<MutualFundModel>> GetAllFundsAsync()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            const string query = "SELECT * FROM MutualFunds;";
            var funds = await connection.QueryAsync<MutualFundModel>(query);
            return funds.AsList(); // Converts IEnumerable to List
        }
        public async Task InsertFundsAsync(List<MutualFundModel> funds)
        {
            try
            {
                if (funds == null || funds.Count == 0)
                    return;

                using var connection = _dbHelper.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction(); // Start transaction

                const string query = @"
            INSERT INTO MutualFunds (SchemeCode, ISINDivPayoutOrGrowth, ISINDivReinvestment, SchemeName, NetAssetValue, NAVDate) 
            VALUES (@SchemeCode, @ISINDivPayoutOrGrowth, @ISINDivReinvestment, @SchemeName, @NetAssetValue, @NAVDate)
            ON CONFLICT(SchemeCode) DO UPDATE SET 
                ISINDivPayoutOrGrowth = excluded.ISINDivPayoutOrGrowth,
                ISINDivReinvestment = excluded.ISINDivReinvestment,
                SchemeName = excluded.SchemeName,
                NetAssetValue = excluded.NetAssetValue,
                NAVDate = excluded.NAVDate;";

                await connection.ExecuteAsync(query, funds, transaction); // Run all inserts in one transaction

                transaction.Commit(); // Commit transaction

                await UpdateNAVTimeAsync(); // Update last updated NAV timestamp
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting MutualFunds: {ex.Message}");
            }
        }

        public async Task ClearFundsAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                connection.Open();

                const string query = "DELETE FROM MutualFunds;";
                await connection.ExecuteAsync(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing MutualFunds: {ex.Message}");
            }
        }

        public async Task<List<MutualFundModel>> SearchMutualFundsAsync(string query)
        {
            using var connection = _dbHelper.GetConnection();

            var words = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var conditions = string.Join(" AND ", words.Select((w, i) => $"SchemeName LIKE @SearchText{i}"));

            var parameters = new DynamicParameters();
            for (int i = 0; i < words.Length; i++)
            {
                parameters.Add($"SearchText{i}", $"%{words[i]}%");
            }

            var sql = $"SELECT * FROM MutualFunds WHERE {conditions} LIMIT 10";
            return (await connection.QueryAsync<MutualFundModel>(sql, parameters)).AsList();
        }

        public async Task BulkUpdateNAVsAsync(List<MutualFundModel> funds)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();
                var sql = "UPDATE MutualFunds SET NetAssetValue = @NAV, NAVDate = @Date WHERE SchemeCode = @SchemeCode";
                var parameters = funds.Select(f => new DynamicParameters(new
                {
                    NAV = f.NetAssetValue,
                    Date = f.NAVDate.ToString("yyyy-MM-dd"), // Format DateTime as string if needed
                    SchemeCode = f.SchemeCode
                })).ToList();

                await connection.ExecuteAsync(sql, parameters, transaction: transaction);

                transaction.Commit();
                await UpdateNAVTimeAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task BulkInsertNewFundsAsync(List<MutualFundModel> newFunds)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();

                var sql = @"
                    INSERT INTO MutualFunds (SchemeCode, ISINDivPayoutOrGrowth, ISINDivReinvestment, SchemeName, NetAssetValue, NAVDate)
                    VALUES (@SchemeCode, @ISINDivPayoutOrGrowth, @ISINDivReinvestment, @SchemeName, @NetAssetValue, @NAVDate)
                    ON CONFLICT(SchemeCode) DO NOTHING;"; // If SchemeCode exists, it won't insert

                await connection.ExecuteAsync(sql, newFunds, transaction: transaction);
                transaction.Commit();
            }
            catch(Exception ex){
                Console.WriteLine(ex.Message);
            }
        }
        public async Task<DateTime?> GetLastNAVTimeAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                const string query = "SELECT LastUpdatedUtc FROM LastUpdateLog WHERE EntityType = 'MutualFunds' ORDER BY LastUpdatedUtc DESC LIMIT 1;";
                var lastUpdatedNav = await connection.ExecuteScalarAsync<DateTime?>(query);
                return lastUpdatedNav;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching last updated NAV: {ex.Message}");
                return null;
            }
        }
        public async Task UpdateNAVTimeAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                var currentUtcTime = DateTime.UtcNow;

                // Use INSERT OR REPLACE to simplify the logic
                const string query = @"
            INSERT INTO LastUpdateLog (EntityType, LastUpdatedUtc)
            VALUES ('MutualFunds', @LastUpdatedUtc)
            ON CONFLICT(EntityType) DO UPDATE SET LastUpdatedUtc = @LastUpdatedUtc;";

                var parameters = new { LastUpdatedUtc = currentUtcTime };
                await connection.ExecuteAsync(query, parameters);

                Console.WriteLine($"Last updated NAV timestamp for MutualFunds updated/inserted to {currentUtcTime}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last updated NAV: {ex.Message}");
            }
        }
    }
}

