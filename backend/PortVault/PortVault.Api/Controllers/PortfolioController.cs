using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortVault.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PortVault.Api.Repositories;
    using PortVault.Api.Services;

    [Route("api/[controller]")]
    [ApiController]
    public sealed class PortfolioController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;
        private readonly IParserService _parser;

        public PortfolioController(IPortfolioRepository repo, IParserService parserService)
        {
            _repo = repo;
            _parser = parserService;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Portfolio portfolio)
        {
            var created = await _repo.CreateAsync(portfolio);
            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }
        [HttpGet("{id:guid}/getholdings")]
        public async Task<IActionResult> GetHoldings([FromBody] Guid id)
        {
            var result = await _repo.GetHoldingsByPortfolioIdAsync(id);
            return Ok(result);
        }

        [HttpPost("{id:guid}/transactions/upload")]
        public async Task<IActionResult> Upload(Guid id, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("no file");

            using var stream = file.OpenReadStream();

            var txns = _parser.Parse(stream, id); // your EPPlus parser

            //await _repo.(txns);

            return Ok();
        }
    }

}
