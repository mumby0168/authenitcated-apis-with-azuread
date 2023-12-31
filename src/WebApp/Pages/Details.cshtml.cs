using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using WebApp.Services;

namespace WebApp.Pages;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IDownstreamApiService _downstreamApiService;

    public DetailsModel(
        IConfiguration configuration,
        IDownstreamApiService downstreamApiService)
    {
        _configuration = configuration;
        _downstreamApiService = downstreamApiService;
    }

    public string? MessageForAuthenticatedUser { get; set; }

    public string? MessageForAuthenticatedUserWithReaderRole { get; set; }
    
    public string? MessageForAuthenticatedUserWithContributorRole { get; set; }

    public string? MessageForAuthenticatedUserWithOwnerRole { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToPage("/");
        }
        
        if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
        {
            MessageForAuthenticatedUser = await _downstreamApiService.CallWebApiForUserAsync();
        }
        else
        {
            MessageForAuthenticatedUser = $"Hello {result.Principal.Identity?.Name}";    
        }

        if (result.Principal.IsInRole("Reader"))
        {
            if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
            {
                MessageForAuthenticatedUser = await _downstreamApiService.CallWebApiForReaderAsync();
            }

            MessageForAuthenticatedUserWithReaderRole = "You are in the User role";
        }

        if (result.Principal.IsInRole("Contributor"))
        {
            if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
            {
                MessageForAuthenticatedUser = await _downstreamApiService.CallWebApiForContributorAsync();
            }

            MessageForAuthenticatedUserWithContributorRole = "You are in the Admin role";
        }
        
        if (result.Principal.IsInRole("Owner"))
        {
            if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
            {
                MessageForAuthenticatedUser = await _downstreamApiService.CallWebApiForOwnerAsync();
            }

            MessageForAuthenticatedUserWithOwnerRole = "You are in the Admin role";
        }

        return Page();
    }
}