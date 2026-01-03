using PortVault.Api.Models;

namespace PortVault.Api.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByIdAsync(Guid id);
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<AppUser> CreateAsync(string username, string email, string password);
    }
}
