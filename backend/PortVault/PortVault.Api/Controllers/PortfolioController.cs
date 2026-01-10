using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PortVault.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PortVault.Api.Parsers;
    using PortVault.Api.Repositories;
    using System.ComponentModel;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class PortfolioController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;
        private readonly ITradeParser _parser;

        public PortfolioController(IPortfolioRepository repo, ITradeParser parser)
        {
            _repo = repo;
            _parser = parser;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var result = await _repo.GetAllPortfoliosAsync(userId);
            return Ok(result.Select(x => new PortfolioResponse 
            { 
                Name = x.Name, 
                Invested = x.Invested, 
                Current = x.Current 
            }));
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetOne(string name)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var result = await _repo.GetByNameAsync(name, userId);

            if (result is null)
                return NotFound();

            return Ok(new PortfolioResponse 
            { 
                Name = result.Name, 
                Invested = result.Invested, 
                Current = result.Current 
            });
        }

        [HttpPost("create-portfolio")]
        public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            try
            {
                var created = await _repo.CreateAsync(request.Name, userId);
                var response = new PortfolioResponse 
                { 
                    Name = created.Name, 
                    Invested = created.Invested, 
                    Current = created.Current 
                };
                return CreatedAtAction(nameof(GetOne), new { name = created.Name }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("{name}/getholdings")]
        public async Task<IActionResult> GetHoldings(string name)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            var result = await _repo.GetHoldingsByPortfolioIdAsync(portfolio.Id);
            return Ok(result.Select(x => new HoldingResponse
            {
                ISIN = x.ISIN,
                Symbol = x.Symbol,
                Qty = x.Qty,
                AvgPrice = x.AvgPrice
            }));
        }

        [HttpGet("{name}/transactions")]
        public async Task<IActionResult> GetTransactions(
            string name, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? search = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            var (items, totalCount) = await _repo.GetTransactionsAsync(portfolio.Id, page, pageSize, from, to, search);
            
            return Ok(new 
            {
                Data = items.Select(x => new TransactionResponse
                {
                    Id = x.Id,
                    Symbol = x.Instrument?.Name ?? string.Empty, // Map from Instrument
                    ISIN = x.Instrument?.Identifiers.FirstOrDefault(i => i.Type == IdentifierType.ISIN)?.Value ?? string.Empty,
                    TradeDate = x.TradeDate,
                    OrderExecutionTime = x.OrderExecutionTime,
                    Segment = x.Segment,
                    Series = x.Series,
                    TradeType = x.TradeType.ToString(),
                    Quantity = x.Quantity,
                    Price = x.Price,
                    TradeID = x.TradeID ?? 0,
                    OrderID = x.OrderID ?? 0
                }),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpGet("{name}/analytics")]
        public async Task<IActionResult> GetAnalytics(
            string name, 
            [FromQuery] string duration = "ALL", 
            [FromQuery] string frequency = "Daily")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            DateTime? from = null;
            var now = DateTime.UtcNow.Date;

            switch (duration.ToUpperInvariant())
            {
                case "1M": from = now.AddMonths(-1); break;
                case "3M": from = now.AddMonths(-3); break;
                case "6M": from = now.AddMonths(-6); break;
                case "YTD": from = new DateTime(now.Year, 1, 1); break;
                case "1Y": from = now.AddYears(-1); break;
                case "3Y": from = now.AddYears(-3); break;
                case "5Y": from = now.AddYears(-5); break;
                case "ALL": default: from = null; break;
            }

            var analytics = await _repo.GetPortfolioAnalyticsAsync(portfolio.Id, from, frequency);
            return Ok(analytics);
        }

        [HttpGet("transactions/template")]
        public IActionResult DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("P    ortVault");
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");

            // Add Headers
            var headers = new[] 
            { 
                "Symbol", "ISIN", "Trade Date", "Segment", "Series", 
                "Trade Type", "Quantity", "Price", "Order Execution Time",
                "Trade ID", "Order ID"
            };

            // Unlock all cells so users can enter data
            worksheet.Cells.Style.Locked = false;

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Locked = true;
            }

            // AutoFit columns
            worksheet.Cells.AutoFitColumns();

            // Protect the worksheet
            worksheet.Protection.IsProtected = true;

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PortVault_Transaction_Template.xlsx");
        }

        [HttpPost("{name}/transactions/upload")]
        public async Task<IActionResult> Upload(string name, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest("Only Excel files (.xlsx, .xls) are supported.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            try
            {
                // Verify portfolio exists
                var portfolio = await _repo.GetByNameAsync(name, userId);
                if (portfolio is null)
                    return NotFound($"Portfolio '{name}' not found.");

                // Compute File Hash
                string fileHash;
                using (var sha256 = SHA256.Create())
                {
                    await using var hashStream = file.OpenReadStream();
                    var hashBytes = await sha256.ComputeHashAsync(hashStream);
                    fileHash = Convert.ToBase64String(hashBytes);
                }

                // Check for duplicate upload
                if (await _repo.IsFileUploadedAsync(portfolio.Id, fileHash))
                {
                    return Conflict(new { message = "This file has already been uploaded to this portfolio." });
                }

                await using var stream = file.OpenReadStream();
                var txns = _parser.Parse(stream, portfolio.Id, userId, null);

                var result = await _repo.AddTransactionsAsync(txns, portfolio.Id, userId);

                if (result.Errors.Any())
                {
                    return BadRequest(new { 
                        message = "Some transactions failed to import",
                        addedCount = result.AddedCount,
                        errors = result.Errors,
                        totalProcessed = txns.Count()
                    });
                }

                // Record successful upload
                if (result.AddedCount > 0)
                {
                    await _repo.RecordFileUploadAsync(new FileUpload
                    {
                        // Id not set (auto-increment)
                        PortfolioId = portfolio.Id,
                        UserId = userId,
                        FileName = file.FileName,
                        FileHash = fileHash,
                        UploadedAt = DateTime.UtcNow,
                        TransactionCount = result.AddedCount
                    });

                    // Auto-recalculate holdings and update portfolio stats
                    await _repo.RecalculateHolding(portfolio.Id);
                }

                return Ok(new { 
                    message = $"Successfully processed {result.AddedCount} new transactions.", 
                    totalProcessed = txns.Count(),
                    newTransactions = result.AddedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPut("{name}/holdings/recalculate")]
        public async Task<IActionResult> RecalculateHoldings(string name)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            await _repo.RecalculateHolding(portfolio.Id);
            return Ok();
        }

        [HttpDelete("{name}/transactions/{transactionId:long}")]
        public async Task<IActionResult> DeleteTransaction(string name, long transactionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            await _repo.DeleteTransactionAsync(transactionId, portfolio.Id);
            
            return Ok(new { message = "Transaction deleted." });
        }

        // It is better to have two separate endpoints for safety and clarity.
        // We use a specific path '/all' for bulk deletion to prevent accidents where a missing ID in the single-delete route
        // results in calling the collection delete route.
        [HttpDelete("{name}/transactions/all")]
        public async Task<IActionResult> ClearAllTransactions(string name, [FromQuery] bool confirm = false)
        {
            if (!confirm)
                return BadRequest("Safety check: To delete all transactions, you must explicitly provide the query parameter '?confirm=true'.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            // This is a destructive operation - use with caution
            await _repo.DeleteTransactionsByPortfolioIdAsync(portfolio.Id);
            
            return Ok(new { message = "All transactions and holdings cleared for this portfolio." });
        }

    }

}

