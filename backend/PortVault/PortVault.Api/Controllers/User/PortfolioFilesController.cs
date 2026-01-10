using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;
using System.Security.Claims;

namespace PortVault.Api.Controllers.User
{
    [Route("api/portfolio/{name}/files")]
    [ApiController]
    [Authorize]
    public class PortfolioFilesController : ControllerBase
    {
        private readonly IPortfolioRepository _repo;

        public PortfolioFilesController(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetFileUploads(string name)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            var files = await _repo.GetFileUploadsByPortfolioIdAsync(portfolio.Id);
            
            var response = ApiResponse<IEnumerable<FileUploadResponse>>.SuccessResponse(
                files.Select(f => new FileUploadResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    UploadedAt = f.UploadedAt,
                    TransactionCount = f.TransactionCount
                }),
                "Files retrieved successfully"
            );
            
            return Ok(response);
        }

        [HttpDelete("{fileId:long}")]
        public async Task<IActionResult> DeleteFileUpload(string name, long fileId)
        {
            var userIdClaim = base.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var portfolio = await _repo.GetByNameAsync(name, userId);
            if (portfolio is null) return NotFound();

            await _repo.DeleteFileUploadAsync(fileId, portfolio.Id);
            
            var response = ApiResponse<object>.SuccessResponse(null, "File upload deleted.");
            return Ok(response);
        }
    }
}
