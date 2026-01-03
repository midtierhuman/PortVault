using System.Security.Cryptography;
using System.Text;

namespace PortVault.Api.Models
{
    public class Transaction
    {
        public Guid Id { get; init; }
        public string TransactionHash { get; init; } = string.Empty; // Unique identifier based on ISIN + Date + Time + Price + Type + Qty
        public Guid PortfolioId { get; init; }
        
        // Asset identifiers
        public string Symbol { get; init; } = string.Empty;
        public string ISIN { get; init; } = string.Empty;
        
        // Trade details
        public DateTime TradeDate { get; init; }
        public DateTime? OrderExecutionTime { get; init; }
        public string Segment { get; init; } = string.Empty; // EQ, MF, etc.
        public string Series { get; init; } = string.Empty; // A, EQ, etc.
        public TradeType TradeType { get; init; }
        public decimal Quantity { get; init; }
        public decimal Price { get; init; }
        public long TradeID { get; init; } = 0;
        public long OrderID { get; init; } = 0;
        /// <summary>
        /// Generates a unique hash for the transaction based on key fields
        /// </summary>
        public static string GenerateTransactionHash(string isin, DateTime tradeDate, DateTime? executionTime, decimal price, TradeType tradeType, decimal quantity, long tradeId, long orderId)
        {
            var compositeKey = $"{isin}|{tradeDate:yyyyMMdd}|{executionTime?.ToString("HHmmss")}|{price}|{tradeType}|{quantity}|{tradeId}|{orderId}";
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(compositeKey));
            return Convert.ToHexString(hashBytes);
        }
    }
}
