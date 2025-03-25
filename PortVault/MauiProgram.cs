using Microsoft.Extensions.Logging;
using PortVault.Authentication;
using PortVault.Repositories.MutualFund;
using PortVault.Services.MutualFund;
using PortVault.Services.Navigation;
using PortVault.Services.UserAuthentication;
using SQLitePCL;

namespace PortVault
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Batteries.Init();
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>();
            builder.Services.AddTransient<LoginPage>();

            // MutualFundService should be Singleton (if it manages state or caching)
            builder.Services.AddSingleton<IMutualFundService, MutualFundService>();

            // MutualFundRepository should be Scoped to ensure fresh DB connections per request
            builder.Services.AddScoped<IMutualFundRepository, MutualFundRepository>();

            builder.Services.AddSingleton<IUserAuthenticationService, UserAuthenticationService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<DBHelper>();

            var app = builder.Build();

            var dbHelper = app.Services.GetRequiredService<DBHelper>();
            Task.Run(async () =>
            {
                await dbHelper.InitializeDatabase();
            }).ConfigureAwait(false);

            return app;
        }
    }
}
