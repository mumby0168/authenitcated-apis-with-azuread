using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting .NET  web host");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddLogging();

    builder.Services.AddAuthentication(
            options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
        .AddCookie(
            CookieAuthenticationDefaults.AuthenticationScheme,
            options => { options.AccessDeniedPath = "/Error"; })
        .AddOpenIdConnect(
            "AzureAd",
            "Azure AD Sign In",
            options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.Authority = "https://login.microsoftonline.com/16e04e4f-42c3-445b-9884-605e3bacbeee/v2.0";
                options.ClientId = "971dc533-5326-41bb-9719-8bd8f9cf61f4";

                options.CallbackPath = "/signin-azuread";
            });


    builder.Services.AddAuthorization(
        options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

// Add services to the container.
    builder.Services.AddRazorPages();

    var app = builder.Build();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();

}
catch (Exception e)
{
    Log.Error(e, ".NET Host terminated unexpectedly");
}