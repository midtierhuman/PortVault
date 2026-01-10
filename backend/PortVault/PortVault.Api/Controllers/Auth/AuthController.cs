using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models;
using PortVault.Api.Models.Dtos;
using PortVault.Api.Repositories;
using PortVault.Api.Services;

namespace PortVault.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;

        public AuthController(IUserRepository users, ITokenService tokens)
        {
            _users = users;
            _tokens = tokens;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            if (await _users.UsernameExistsAsync(request.Username.Trim()))
                return Conflict(ApiResponse<object>.ErrorResponse("Username already exists"));

            if (await _users.EmailExistsAsync(request.Email.Trim()))
                return Conflict(ApiResponse<object>.ErrorResponse("Email already exists"));

            var role = await _users.HasAnyUsersAsync() ? AppRole.User : AppRole.Admin;

            var user = await _users.CreateAsync(request.Username, request.Email, request.Password, role);
            var (token, exp) = _tokens.CreateToken(user);

            var authResponse = new AuthResponse
            {
                AccessToken = token,
                ExpiresUtc = exp,
                Username = user.Username,
                Email = user.Email
            };
            
            var response = ApiResponse<AuthResponse>.SuccessResponse(authResponse, "User registered successfully");
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data"));

            var hasAnyIdentifier = !string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(request.Username);
            if (!hasAnyIdentifier)
                return BadRequest(ApiResponse<object>.ErrorResponse("Provide either email or username"));

            var user = !string.IsNullOrWhiteSpace(request.Email)
                ? await _users.GetByEmailAsync(request.Email.Trim())
                : await _users.GetByUsernameAsync(request.Username!.Trim());

            if (user is null)
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid credentials"));

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid credentials"));

            var (token, exp) = _tokens.CreateToken(user);

            var authResponse = new AuthResponse
            {
                AccessToken = token,
                ExpiresUtc = exp,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
            
            var response = ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Login successful");
            return Ok(response);
        }
    }
}
