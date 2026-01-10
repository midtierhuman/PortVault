using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;
using System.Security.Claims;

namespace PortVault.Api.Controllers.User
{
    [Route("api/portfolio/{name}")]
    [ApiController]
    [Authorize]
    public class PortfolioHoldingsController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;

        public PortfolioHoldingsController(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("getholdings")]
        public async Task<IActionResult> GetHoldings(string name)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            var result = await _repo.GetHoldingsByPortfolioIdAsync(portfolio.Id);
            var holdings = result.Select(x => new HoldingResponse
            {
                ISIN = x.ISIN,
                Symbol = x.Symbol,
                Qty = x.Qty,
                AvgPrice = x.AvgPrice
            });
            
            var response = ApiResponse<IEnumerable<HoldingResponse>>.SuccessResponse(holdings, "Holdings retrieved successfully");
            return Ok(response);
        }

        [HttpPut("holdings/recalculate")]
        public async Task<IActionResult> RecalculateHoldings(string name)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            await _repo.RecalculateHolding(portfolio.Id);
            
            var response = ApiResponse<object>.SuccessResponse(null, "Holdings recalculated successfully");
            return Ok(response);
        }
    }
}
