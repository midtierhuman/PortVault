using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;
using System.Security.Claims;

namespace PortVault.Api.Controllers.User
{
    [Route("api/portfolio/{name}/analytics")]
    [ApiController]
    [Authorize]
    public class PortfolioAnalyticsController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;

        public PortfolioAnalyticsController(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics(
            string name, 
            [FromQuery] string duration = "ALL", 
            [FromQuery] string frequency = "Daily")
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

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
            
            var response = ApiResponse<AnalyticsResponse>.SuccessResponse(analytics, "Analytics retrieved successfully");
            return Ok(response);
        }
    }
}
