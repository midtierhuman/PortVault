using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortVault.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private static readonly List<Portfolio> _portfolios = new()
    {
        new Portfolio { Id="p1", Name="LargeCap", Invested=100000m, Current=132500m },
        new Portfolio { Id="p2", Name="MidCap Mix", Invested=80000m, Current=92000m },
    };

        private static readonly Dictionary<string, PortfolioDetails> _details =
            new()
            {
                ["p1"] = new PortfolioDetails
                {
                    Id = "p1",
                    Name = "LargeCap",
                    Holdings = new List<Asset>
                    {
                    new Asset
                    {
                        InstrumentId="INF109K01VT7",
                        Type=AssetType.Mf,
                        Name = "Parag Parikh Flexi Cap Fund Direct Growth",
                        Nav=187.44m,
                        MarketPrice=187.44m,
                        LastUpdated=DateTime.UtcNow
                    },
                    new Asset
                    {
                        InstrumentId="TCS",
                        Type=AssetType.Stock,
                        Name = "Tata Consultancy Services Ltd",
                        MarketPrice=4015.35m,
                        LastUpdated=DateTime.UtcNow
                    }
                    }
                },
                ["p2"] = new PortfolioDetails
                {
                    Id = "p2",
                    Name = "MidCap Mix",
                    Holdings = new List<Asset>()
                }
            };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_portfolios);
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(string id)
        {
            return _details.TryGetValue(id, out var data) ? Ok(data) : NotFound();
        }
        [HttpPost]
        public IActionResult Create([FromBody] Portfolio portfolio)
        {
            _portfolios.Add(portfolio);
            return CreatedAtAction(nameof(GetOne), new { id = portfolio.Id }, portfolio);
        }
    }
}
