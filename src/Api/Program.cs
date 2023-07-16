using Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting .NET web host");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddAuthorization(
        options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

    var app = builder.Build();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet(
        "/",
        (
            IConfiguration configuration) => $"Azure AD Auth Web API - {configuration.GetDockerImage()}");

    app.MapGet(
            "api/v1/default",
            () => $"This is the default message {DateTime.UtcNow}")
        .RequireAuthorization();

    app.Run();
}
catch (Exception e)
{
    Log.Error(
        e,
        ".NET Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}