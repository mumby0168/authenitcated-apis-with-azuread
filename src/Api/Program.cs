using Api.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
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

    builder.Services.AddApplicationInsightsTelemetry();
    builder.Host.UseSerilog((_, serviceProvider, loggerConfiguration) =>
    {
        // write to application insights as trace logs
        // recommended approach to re-use the same instance of TelemetryConfiguration as the AI SDK.
        loggerConfiguration
            .WriteTo
            .ApplicationInsights(
                serviceProvider.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces);

        loggerConfiguration
            .WriteTo
            .Console();

        // read serilog config block from IConfiguration
        // reads things such as log levels and filter rules
        loggerConfiguration
            .ReadFrom
            .Configuration(builder.Configuration);
    });

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