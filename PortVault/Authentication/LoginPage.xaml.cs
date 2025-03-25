using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PortVault.Services.UserAuthentication;
using System;

namespace PortVault.Authentication
{
    public partial class LoginPage : ContentPage
    {
        private readonly IUserAuthenticationService _authService;

        public LoginPage(IUserAuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text ?? string.Empty;
            string password = passwordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both email and password", "OK");
                return;
            }

            bool isAuthenticated = await _authService.AuthenticateUserAsync(email, password);
            if (isAuthenticated)
            {
                Preferences.Set("UserLoggedIn", true);
                Application.Current.MainPage = new MainPage();
            }
            else
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }
        private async void OnSignupTapped(object sender, EventArgs e)
        {
            
        }
    }
}