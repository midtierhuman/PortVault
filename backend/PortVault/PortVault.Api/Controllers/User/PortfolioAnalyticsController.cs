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

        // Valid values for query parameters
        private static readonly string[] ValidViewTypes = { "cumulative", "period" };
        private static readonly string[] ValidFrequencies = { "daily", "weekly", "monthly", "halfyearly", "yearly", "transaction" };
        private static readonly string[] ValidDurations = { "1M", "3M", "6M", "YTD", "1Y", "3Y", "5Y", "ALL" };

        public PortfolioAnalyticsController(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics(
            string name, 
            [FromQuery] string duration = "ALL", 
            [FromQuery] string frequency = "Monthly",
            [FromQuery] string view = "cumulative")
        {
            // Validate all parameters first before any database operations
            var viewType = view.ToLowerInvariant();
            if (!ValidViewTypes.Contains(viewType))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Invalid view type. Valid types are: {string.Join(", ", ValidViewTypes)}"
                ));

            var freq = frequency.ToLowerInvariant();
            if (!ValidFrequencies.Contains(freq))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Invalid frequency. Valid types are: {string.Join(", ", ValidFrequencies)}"
                ));

            var dur = duration.ToUpperInvariant();
            if (!ValidDurations.Contains(dur))
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Invalid duration. Valid types are: {string.Join(", ", ValidDurations)}"
                ));

            // Now validate user and portfolio
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) 
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            // Calculate date range based on duration
            DateTime? from = null;
            var now = DateTime.UtcNow.Date;

            from = dur switch
            {
                "1M" => now.AddMonths(-1),
                "3M" => now.AddMonths(-3),
                "6M" => now.AddMonths(-6),
                "YTD" => new DateTime(now.Year, 1, 1),
                "1Y" => now.AddYears(-1),
                "3Y" => now.AddYears(-3),
                "5Y" => now.AddYears(-5),
                "ALL" => null,
                _ => null
            };

            var analytics = await _repo.GetPortfolioAnalyticsAsync(portfolio.Id, from, freq, viewType);
            
            var response = ApiResponse<AnalyticsResponse>.SuccessResponse(analytics, "Analytics retrieved successfully");
            return Ok(response);
        }
    }
}
