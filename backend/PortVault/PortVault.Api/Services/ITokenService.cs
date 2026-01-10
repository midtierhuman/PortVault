using PortVault.Api.Models.Entities;

namespace PortVault.Api.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiresUtc) CreateToken(AppUser user);
    }
}
