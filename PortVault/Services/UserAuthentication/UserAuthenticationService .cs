using PortVault.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Services.UserAuthentication
{
    internal class UserAuthenticationService : IUserAuthenticationService
    {
        private const string AuthKey = "IsUserLoggedIn";
        private const string UserEmailKey = "LoggedInUserEmail";

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            // TODO: Replace this with actual authentication logic (e.g., check DB)
            if (email == "admin@portvault.com" && password == "portvault")
            {
                Preferences.Set(AuthKey, true);
                Preferences.Set(UserEmailKey, email);
                return true;
            }
            return false;
        }

        public Task<bool> IsUserLoggedInAsync()
        {
            bool isLoggedIn = Preferences.Get(AuthKey, false);
            return Task.FromResult(isLoggedIn);
        }

        public Task<string?> GetLoggedInUserEmailAsync()
        {
            string? email = Preferences.Get(UserEmailKey, null);
            return Task.FromResult(email);
        }

        public async Task LogoutAsync()
        {
            Preferences.Remove(AuthKey);
            Preferences.Remove(UserEmailKey);
            Preferences.Set("UserLoggedIn", false);
            Application.Current.MainPage = new LoginPage(this); // ✅ Redirect to login page
            await Task.CompletedTask;
        }
    }
}
