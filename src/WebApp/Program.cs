using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using WebApp.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting .NET web host");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddLogging();

    builder.Services.AddSingleton(new DefaultAzureCredential());

    builder.Services.AddHttpClient<IDownstreamApiService, DownstreamApiService>(
        client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration.GetValue<string>("DownstreamApi:BaseUrl") ??
                throw new InvalidOperationException("Please provider a downstream api base url"));
        });

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


    // To allow a docker image hosted on http to pass the original https URI to azure ad
    builder.Services.Configure<ForwardedHeadersOptions>(
        options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto;

            // Only loopback proxies are allowed by default.
            // Clear that restriction because forwarders are enabled by explicit
            // configuration.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
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

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseForwardedHeaders();

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch (Exception e)
{
    Log.Error(
        e,
        ".NET Host terminated unexpectedly");
}