using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BusManagement.Pages
{
    public class VerifyOtpModel : PageModel
    {
        [BindProperty] public string? Code { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var sessionOtp = HttpContext.Session.GetString("MFA_OTP");
            var userId = HttpContext.Session.GetString("MFA_UserId");
            var userRole = HttpContext.Session.GetString("MFA_UserRole");
            var email = HttpContext.Session.GetString("MFA_Email");

            if (Code == sessionOtp && int.TryParse(userId, out int uid))
            {
                // Build claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email!),
                    new Claim(ClaimTypes.Role, userRole!),
                    new Claim("UserId", uid.ToString())
                };
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                await HttpContext.SignInAsync("CookieAuth", principal);

                // Clean up session
                HttpContext.Session.Clear();

                // Redirect based on role
                return userRole == "Client" ? RedirectToPage("/Clients/Index")
                     : userRole == "Driver" ? RedirectToPage("/Drivers/Index")
                     : RedirectToPage("/Admin/Index");
            }

            ErrorMessage = "Invalid or expired code.";
            return Page();
        }
    }
}
