using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Services.UserAuthentication
{
    public interface IUserAuthenticationService
    {
        Task<bool> AuthenticateUserAsync(string email, string password);
        Task<bool> IsUserLoggedInAsync();
        Task<string?> GetLoggedInUserEmailAsync();
        Task LogoutAsync();
    }
}
