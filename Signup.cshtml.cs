using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace BusManagement.Pages
{
    public class SignupModel : PageModel
    {
        [BindProperty]
        public string? UserType { get; set; }

        [BindProperty]
        public string? Name { get; set; }

        [BindProperty]
        public string? Licence { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? Phone { get; set; }

        private readonly IConfiguration _configuration;

        public SignupModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Encrypt the password
            string encryptedPassword = EncryptPassword(Password);

            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Insert the client, driver, or admin based on the user type
                if (UserType == "Client")
                {
                    var query = "INSERT INTO Clients (Name, Email, Password, Phone) VALUES (@Name, @Email, @Password, @Phone)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", encryptedPassword);
                        command.Parameters.AddWithValue("@Phone", Phone);
                        command.ExecuteNonQuery();
                    }
                }
                else if (UserType == "Driver")
                {
                    var query = "INSERT INTO Drivers (Name, Licence, Email, Password, Phone) VALUES (@Name, @Licence, @Email, @Password, @Phone)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Licence", "Licence");
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", encryptedPassword);
                        command.Parameters.AddWithValue("@Phone", Phone);
                        command.ExecuteNonQuery();
                    }
                }
                else if (UserType == "Admin")  // Admin registration
                {
                    var query = "INSERT INTO Admins (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", encryptedPassword);  // Store encrypted password
                        command.ExecuteNonQuery();
                    }
                }
            }

            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Encrypts a plain-text password using SHA256 hashing.
        /// </summary>
        /// <param name="password">Plain-text password.</param>
        /// <returns>Hashed password as a hexadecimal string.</returns>
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
