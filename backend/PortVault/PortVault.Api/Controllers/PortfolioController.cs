using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortVault.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PortVault.Api.Parsers;
    using PortVault.Api.Repositories;
    using PortVault.Api.Services;
    using System.ComponentModel;

    [Route("api/[controller]")]
    [ApiController]
    public sealed class PortfolioController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;
        private readonly TradeParserFactory _factory;

        public PortfolioController(IPortfolioRepository repo, TradeParserFactory tradeParserFactory)
        {
            _repo = repo;
            _factory = tradeParserFactory;
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
        public async Task<IActionResult> Create([FromBody] Portfolio portfolio)
        {
            var created = await _repo.CreateAsync(portfolio);
            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }

        [HttpGet("{id:guid}/getholdings")]
        public async Task<IActionResult> GetHoldings(Guid id)
        {
            var result = await _repo.GetHoldingsByPortfolioIdAsync(id);
            return Ok(result);
        }

        //[HttpPost("{id:guid}/transactions/upload/{provider}")]
        //public async Task<IActionResult> Upload(Guid id,string provider, IFormFile file, [FromQuery] string? pwd)
        [HttpPost("transactions/upload")]
        public async Task<IActionResult> Upload( IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("no file");

            try
            {
                Guid id = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
                await using var stream = file.OpenReadStream();
                var parser = _factory.Get("camskfin");
                var txns = parser.Parse(stream, id, "hello1234");

                await _repo.AddTransactionsAsync(txns);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
