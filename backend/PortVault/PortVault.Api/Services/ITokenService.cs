using PortVault.Api.Models;

namespace PortVault.Api.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiresUtc) CreateToken(AppUser user);
    }
}
