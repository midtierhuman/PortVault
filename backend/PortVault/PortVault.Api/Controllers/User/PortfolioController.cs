using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;
using System.Security.Claims;

namespace PortVault.Api.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class PortfolioController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;

        public PortfolioController(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var result = await _repo.GetAllPortfoliosAsync(userId);
            var portfolios = result.Select(x => new PortfolioResponse 
            { 
                Name = x.Name, 
                Invested = x.Invested, 
                Current = x.Current 
            });
            
            var response = ApiResponse<IEnumerable<PortfolioResponse>>.SuccessResponse(portfolios, "Portfolios retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetOne(string name)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));

            var result = await _repo.GetByNameAsync(name, userId);

            if (result is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Portfolio not found"));

            var portfolio = new PortfolioResponse 
            { 
                Name = result.Name, 
                Invested = result.Invested, 
                Current = result.Current 
            };
            
            var response = ApiResponse<PortfolioResponse>.SuccessResponse(portfolio, "Portfolio retrieved successfully");
            return Ok(response);
        }

        [HttpPost("create-portfolio")]
        public async Task<IActionResult> Create([FromBody] CreatePortfolioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access"));
            
            try
            {
                var created = await _repo.CreateAsync(request.Name, userId);
                var portfolio = new PortfolioResponse 
                { 
                    Name = created.Name, 
                    Invested = created.Invested, 
                    Current = created.Current 
                };
                
                var response = ApiResponse<PortfolioResponse>.SuccessResponse(portfolio, "Portfolio created successfully");
                return CreatedAtAction(nameof(GetOne), new { name = created.Name }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
