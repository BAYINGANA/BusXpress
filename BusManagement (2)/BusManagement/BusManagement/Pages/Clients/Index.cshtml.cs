using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BusManagement.Pages.Clients
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? UserId { get; set; }

        public void OnGet()
        {
            // Access User Claims
            Email = User.Identity?.Name;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            UserId = User.FindFirst("UserId")?.Value;
        }
    }
}
