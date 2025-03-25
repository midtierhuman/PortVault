using Microsoft.AspNetCore.Components;
using PortVault.Services.UserAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortVault
{
    public class BaseComponent : ComponentBase
    {
        [Inject] protected IUserAuthenticationService AuthService { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }

        protected bool IsUserAuthenticated { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            IsUserAuthenticated = await AuthService.IsUserLoggedInAsync();
            if (!IsUserAuthenticated)
            {
                Navigation.NavigateTo("/login", forceLoad: true);
            }
        }
    }
}
