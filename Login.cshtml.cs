using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BusManagement.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? UserType { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(UserType) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please fill all fields.";
                return Page();
            }

            string encryptedPassword = EncryptPassword(Password);
            string connectionString = _configuration.GetConnectionString("connstring");
            bool isAuthenticated = false;
            string? userRole = null;
            int userId = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = string.Empty;

                if (UserType == "Client")
                {
                    query = "SELECT ClientId FROM Clients WHERE Email = @Email AND Password = @Password";
                    userRole = "Client";
                }
                else if (UserType == "Driver")
                {
                    query = "SELECT DriverId FROM Drivers WHERE Email = @Email AND Password = @Password";
                    userRole = "Driver";
                }
                else if (UserType == "Admin")
                {
                    query = "SELECT AdminId FROM Admins WHERE Email = @Email AND Password = @Password";
                    userRole = "Admin";
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@Password", encryptedPassword);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                        isAuthenticated = true;
                    }
                }
            }

            if (isAuthenticated && userRole != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Email!),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim("UserId", userId.ToString()) // Store the ClientId/DriverId/AdminId
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                HttpContext.SignInAsync("CookieAuth", claimsPrincipal).Wait();

                // Redirect based on user role
                if (userRole == "Client") return RedirectToPage("/Clients/Index");
                if (userRole == "Driver") return RedirectToPage("/Drivers/Index");
                if (userRole == "Admin") return RedirectToPage("/Admin/Index");
            }
            else
            {
                ErrorMessage = "Invalid login credentials.";
            }

            return Page();
        }

        private string EncryptPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert byte to hexadecimal
                }
                return builder.ToString();
            }
        }
    }
}
