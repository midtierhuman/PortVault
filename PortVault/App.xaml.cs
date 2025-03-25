using Microsoft.Extensions.DependencyInjection;
using PortVault.Authentication;

namespace PortVault
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            bool isUserLoggedIn = Preferences.Get("UserLoggedIn", false);

            if (isUserLoggedIn)
            {
                MainPage = new MainPage(); // ✅ Load Blazor if logged in
            }
            else
            {
                MainPage = serviceProvider.GetService<LoginPage>(); // ✅ Show Login Page first
            }

            //MainPage = new MainPage();
        }
    }
}
