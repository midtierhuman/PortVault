using Dapper;
using PortVault.Models;
using System;
using System.Collections.Generic;
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
        public async Task<List<MutualFundModel>> GetFundsAsync()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            const string query = "SELECT * FROM MutualFunds;";
            var funds = await connection.QueryAsync<MutualFundModel>(query);
            return funds.AsList(); // Converts IEnumerable to List
        }
        public async Task InsertFundsAsync(List<MutualFundModel> funds)
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
        }

        public async Task ClearFundsAsync()
        {
            using var connection = _dbHelper.GetConnection();
            connection.Open();

            const string query = "DELETE FROM MutualFunds;";
            await connection.ExecuteAsync(query);
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
            using var connection = _dbHelper.GetConnection();
            using var transaction = connection.BeginTransaction();

            var sql = "UPDATE MutualFunds SET NetAssetValue = @NAV, NAVDate = @Date WHERE SchemeCode = @SchemeCode";
            await connection.ExecuteAsync(sql, funds, transaction: transaction);

            transaction.Commit();
        }
        public async Task BulkInsertNewFundsAsync(List<MutualFundModel> newFunds)
        {
            using var connection = _dbHelper.GetConnection();
            using var transaction = connection.BeginTransaction();

            var sql = @"
        INSERT INTO MutualFunds (SchemeCode, ISINDivPayoutOrGrowth, ISINDivReinvestment, SchemeName, NetAssetValue, NAVDate)
        VALUES (@SchemeCode, @ISINDivPayoutOrGrowth, @ISINDivReinvestment, @SchemeName, @NetAssetValue, @NAVDate)
        ON CONFLICT(SchemeCode) DO NOTHING;"; // If SchemeCode exists, it won't insert

            await connection.ExecuteAsync(sql, newFunds, transaction: transaction);
            transaction.Commit();
        }
    }
}

