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
            var result = await _repo.GetAllPortfoliosAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOne(Guid id)
        {
            var result = await _repo.GetPortfolioByIdAsync(id);

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("create-portfolio")]
        public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var created = await _repo.CreateAsync(request.Name, userId);
            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}/getholdings")]
        public async Task<IActionResult> GetHoldings(Guid id)
        {
            var result = await _repo.GetHoldingsByPortfolioIdAsync(id);
            return Ok(result);
        }

        [HttpPost("{portfolioId:guid}/transactions/upload")]
        public async Task<IActionResult> Upload(Guid portfolioId, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest("Only Excel files (.xlsx, .xls) are supported.");

            try
            {
                // Verify portfolio exists
                var portfolio = await _repo.GetPortfolioByIdAsync(portfolioId);
                if (portfolio is null)
                    return NotFound($"Portfolio with ID {portfolioId} not found.");

                await using var stream = file.OpenReadStream();
                var txns = _parser.Parse(stream, portfolioId, null);

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
        [HttpPut("{id}/holdings/recalculate")]
        public async Task<IActionResult> RecalculateHoldings(Guid id)
        {
            await _repo.RecalculateHolding(id);
            return Ok();
        }

    }

}
