using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortVault.Api.Models.Auth;
using PortVault.Api.Repositories;
using PortVault.Api.Services;

namespace PortVault.Api.Controllers
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
                return BadRequest(ModelState);

            if (await _users.UsernameExistsAsync(request.Username.Trim()))
                return Conflict(new { error = "Username already exists." });

            if (await _users.EmailExistsAsync(request.Email.Trim()))
                return Conflict(new { error = "Email already exists." });

            var user = await _users.CreateAsync(request.Username, request.Email, request.Password);
            var (token, exp) = _tokens.CreateToken(user);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                ExpiresUtc = exp,
                Username = user.Username,
                Email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hasAnyIdentifier = !string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(request.Username);
            if (!hasAnyIdentifier)
                return BadRequest(new { error = "Provide either email or username." });

            var user = !string.IsNullOrWhiteSpace(request.Email)
                ? await _users.GetByEmailAsync(request.Email.Trim())
                : await _users.GetByUsernameAsync(request.Username!.Trim());

            if (user is null)
                return Unauthorized(new { error = "Invalid credentials." });

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized(new { error = "Invalid credentials." });

            var (token, exp) = _tokens.CreateToken(user);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                ExpiresUtc = exp,
                Username = user.Username,
                Email = user.Email
            });
        }
    }
}
