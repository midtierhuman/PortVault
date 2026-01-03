using Microsoft.EntityFrameworkCore;
using PortVault.Api.Data;
using PortVault.Api.Models;
using PortVault.Api.Services;

namespace PortVault.Api.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDb _db;
        public UserRepository(AppDb db) => _db = db;

        public Task<AppUser?> GetByIdAsync(Guid id) =>
            _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        public Task<AppUser?> GetByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(x => x.Email == email);

        public Task<AppUser?> GetByUsernameAsync(string username) =>
            _db.Users.FirstOrDefaultAsync(x => x.Username == username);

        public Task<bool> UsernameExistsAsync(string username) =>
            _db.Users.AnyAsync(x => x.Username == username);

        public Task<bool> EmailExistsAsync(string email) =>
            _db.Users.AnyAsync(x => x.Email == email);

        public async Task<AppUser> CreateAsync(string username, string email, string password)
        {
            var (hash, salt) = PasswordHasher.Hash(password);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = username.Trim(),
                Email = email.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedUtc = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
