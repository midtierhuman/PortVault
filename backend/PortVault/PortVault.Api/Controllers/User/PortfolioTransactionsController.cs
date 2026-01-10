using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PortVault.Api.Models;
using PortVault.Api.Models.Entities;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Parsers;
using PortVault.Api.Repositories;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PortVault.Api.Controllers.User
{
    [Route("api/portfolio/{name}/transactions")]
    [ApiController]
    [Authorize]
    public class PortfolioTransactionsController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;
        private readonly ITradeParser _parser;

        public PortfolioTransactionsController(IPortfolioRepository repo, ITradeParser parser)
        {
            _repo = repo;
            _parser = parser;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            string name, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? search = null)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            var (items, totalCount) = await _repo.GetTransactionsAsync(portfolio.Id, page, pageSize, from, to, search);
            
            var paginatedResponse = new PaginatedTransactionsResponse
            {
                Data = items.Select(x => new TransactionResponse
                {
                    Id = x.Id,
                    Symbol = x.Instrument?.Name ?? string.Empty,
                    ISIN = x.Instrument?.Identifiers.FirstOrDefault(i => i.Type == IdentifierType.ISIN)?.Value ?? string.Empty,
                    TradeDate = x.TradeDate,
                    OrderExecutionTime = x.OrderExecutionTime,
                    Segment = x.Segment,
                    Series = x.Series,
                    TradeType = x.TradeType.ToString(),
                    Quantity = x.Quantity,
                    Price = x.Price,
                    TradeID = x.TradeID ?? string.Empty,
                    OrderID = x.OrderID ?? string.Empty
                }),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
            
            var response = ApiResponse<PaginatedTransactionsResponse>.SuccessResponse(paginatedResponse, "Transactions retrieved successfully");
            return Ok(response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(string name, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("No file uploaded"));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(ApiResponse<object>.ErrorResponse("Only Excel files (.xlsx, .xls) are supported"));

            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            try
            {
                var portfolio = await _repo.GetByNameAsync(name, userId);
                if (portfolio is null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Portfolio '{name}' not found"));

                string fileHash;
                using (var sha256 = SHA256.Create())
                {
                    await using var hashStream = file.OpenReadStream();
                    var hashBytes = await sha256.ComputeHashAsync(hashStream);
                    fileHash = Convert.ToBase64String(hashBytes);
                }

                if (await _repo.IsFileUploadedAsync(portfolio.Id, fileHash))
                {
                    return Conflict(ApiResponse<object>.ErrorResponse("This file has already been uploaded to this portfolio"));
                }

                await using var stream = file.OpenReadStream();
                var txns = _parser.Parse(stream, portfolio.Id, userId, null);

                var result = await _repo.AddTransactionsAsync(txns, portfolio.Id, userId);

                if (result.Errors.Any())
                {
                    var errorResponse = new TransactionUploadResponse
                    {
                        Message = "Some transactions failed to import",
                        AddedCount = result.AddedCount,
                        Errors = result.Errors,
                        TotalProcessed = txns.Count()
                    };
                    
                    var apiErrorResponse = new ApiResponse<TransactionUploadResponse>
                    {
                        Success = false,
                        Message = "Some transactions failed to import",
                        Data = errorResponse
                    };
                    
                    return BadRequest(apiErrorResponse);
                }

                if (result.AddedCount > 0)
                {
                    await _repo.RecordFileUploadAsync(new FileUpload
                    {
                        PortfolioId = portfolio.Id,
                        UserId = userId,
                        FileName = file.FileName,
                        FileHash = fileHash,
                        UploadedAt = DateTime.UtcNow,
                        TransactionCount = result.AddedCount
                    });

                    await _repo.RecalculateHolding(portfolio.Id);
                }

                var successResponse = new TransactionUploadResponse
                {
                    Message = $"Successfully processed {result.AddedCount} new transactions",
                    TotalProcessed = txns.Count(),
                    NewTransactions = result.AddedCount,
                    AddedCount = result.AddedCount,
                    Errors = result.Errors
                };
                
                var response = ApiResponse<TransactionUploadResponse>.SuccessResponse(successResponse, "File uploaded successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error processing file: {ex.Message}"));
            }
        }

        [HttpDelete("{transactionId:long}")]
        public async Task<IActionResult> DeleteTransaction(string name, long transactionId)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            await _repo.DeleteTransactionAsync(transactionId, portfolio.Id);
            
            var response = ApiResponse<object>.SuccessResponse(null, "Transaction deleted successfully");
            return Ok(response);
        }

        [HttpDelete("all")]
        public async Task<IActionResult> ClearAllTransactions(string name, [FromQuery] bool confirm = false)
        {
            if (!confirm)
                return BadRequest(ApiResponse<object>.ErrorResponse("Safety check: To delete all transactions, you must explicitly provide the query parameter '?confirm=true'"));

            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            await _repo.DeleteTransactionsByPortfolioIdAsync(portfolio.Id);
            
            var response = ApiResponse<object>.SuccessResponse(null, "All transactions and holdings cleared for this portfolio");
            return Ok(response);
        }

        [HttpGet("~/api/portfolio/transactions/template")]
        public IActionResult DownloadTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");

            var headers = new[] 
            { 
                "Symbol", "ISIN", "Trade Date", "Segment", "Series", 
                "Trade Type", "Quantity", "Price", "Order Execution Time",
                "Trade ID", "Order ID"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            worksheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PortVault_Transaction_Template.xlsx");
        }
    }
}
