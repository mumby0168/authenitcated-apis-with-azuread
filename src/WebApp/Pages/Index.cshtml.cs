using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly IAuthenticationService _authenticationService;

    public IndexModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    
    public async Task<IActionResult> OnGet()
    {
        var result = await _authenticationService.AuthenticateAsync(
            HttpContext,
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (result.Succeeded)
        {
            return RedirectToPage("/Details");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var result = await _authenticationService.AuthenticateAsync(
            HttpContext,
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (result.Succeeded)
        {
            return RedirectToPage("/Details");
        }
        
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = "/Details"
            },
            "AzureAd");
    }
}