using PortVault.Authentication;
using PortVault.Services.UserAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault.Services.Navigation
{
  
    public class NavigationService : INavigationService
    {
        private readonly IUserAuthenticationService _authService;

        public NavigationService(IUserAuthenticationService authService)
        {
            _authService = authService;
        }
        public async Task NavigateToLoginPageAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new LoginPage(_authService));
        }
    }
    
}
