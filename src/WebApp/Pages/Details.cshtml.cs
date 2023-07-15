using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

[Authorize(AuthenticationSchemes = "Cookies")]
public class Details : PageModel
{
    public void OnGet()
    {
        
    }
}