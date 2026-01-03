using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
                InstrumentId = x.InstrumentId,
                Qty = x.Qty,
                AvgPrice = x.AvgPrice
            }));
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

                await using var stream = file.OpenReadStream();
                var txns = _parser.Parse(stream, portfolio.Id, null);

                var addedCount = await _repo.AddTransactionsAsync(txns);

                return Ok(new { 
                    message = $"Successfully processed {addedCount} new transactions.", 
                    totalProcessed = txns.Count(),
                    newTransactions = addedCount,
                    duplicatesSkipped = txns.Count() - addedCount
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

    }

}
