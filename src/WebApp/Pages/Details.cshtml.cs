using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;

namespace WebApp.Pages;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IConfiguration _configuration;

    public DetailsModel(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? MessageForAuthenticatedUser { get; set; }

    public string? MessageForAuthenticatedUserWithUserRole { get; set; }

    public string? MessageForAuthenticatedUserWithAdminRole { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToPage("/");
        }

        MessageForAuthenticatedUser = $"Hello {result.Principal.Identity?.Name}";

        if (result.Principal.IsInRole("User"))
        {
            if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
            {
                throw new NotImplementedException();
            }

            MessageForAuthenticatedUserWithUserRole = "You are in the User role";
        }

        if (result.Principal.IsInRole("Admin"))
        {
            if (_configuration.GetValue<bool>("DownstreamApi:IsEnabled"))
            {
                throw new NotImplementedException();
            }

            MessageForAuthenticatedUserWithAdminRole = "You are in the Admin role";
        }

        return Page();
    }
}