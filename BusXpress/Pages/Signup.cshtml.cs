using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace BusManagement.Pages
{
    public class SignupModel : PageModel
    {
        [BindProperty]
        public string? UserType { get; set; }

        [BindProperty]
        public string? Name { get; set; }

        [BindProperty]
        public string? LicenceNumber { get; set; }

        [BindProperty]
        public IFormFile? LicencePhoto { get; set; }

        // This will store the relative path of the uploaded file in the database.
        [BindProperty]
        public string? Licencefile { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? Phone { get; set; }

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SignupModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Return early validation for file size/type if needed
            if (UserType == "Driver" && (LicencePhoto == null || LicencePhoto.Length == 0))
            {
                ModelState.AddModelError(string.Empty, "Licence photo is required for drivers.");
                return Page();
            }

            if (UserType == "Driver" && LicencePhoto != null)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
                if (!allowedTypes.Contains(LicencePhoto.ContentType.ToLower()))
                {
                    ModelState.AddModelError(string.Empty, "Only JPEG and PNG images are allowed.");
                    return Page();
                }

                // Validate file size (5MB max)
                const int maxSize = 5 * 1024 * 1024;
                if (LicencePhoto.Length > maxSize)
                {
                    ModelState.AddModelError(string.Empty, "File size exceeds the 5MB limit.");
                    return Page();
                }
            }

            // Encrypt the password using SHA256
            string encryptedPassword = EncryptPassword(Password);

            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
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
                        // Process the LicencePhoto file upload first
                        if (LicencePhoto != null && LicencePhoto.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "licences");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }
                            // Generate a unique (but shorter) file name using 4 characters of GUID
                            var fileName = $"{Path.GetFileNameWithoutExtension(LicencePhoto.FileName)}_{Guid.NewGuid().ToString().Substring(0, 4)}{Path.GetExtension(LicencePhoto.FileName)}";
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            await EncryptAndSaveFileAsync(LicencePhoto, filePath);

                            // Store the relative path for DB
                            Licencefile = $"/uploads/licences/{fileName}";
                        }

                        // Trim the licence number and licencefile before checking
                        var trimmedLicenceNumber = LicenceNumber?.Trim();
                        var trimmedLicencefile = Licencefile?.Trim();

                        // Now validate that both LicenceNumber and Licencefile are present for drivers.
                        if (string.IsNullOrWhiteSpace(trimmedLicenceNumber) || string.IsNullOrWhiteSpace(trimmedLicencefile))
                        {
                            ModelState.AddModelError(string.Empty, "Licence number and photo are required for drivers.");
                            return Page();
                        }

                        var query = "INSERT INTO Drivers (Name, LicenceNumber, LicencePhoto, Email, Password, Phone) VALUES (@Name, @LicenceNumber, @LicencePhoto, @Email, @Password, @Phone)";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", Name);
                            command.Parameters.AddWithValue("@LicenceNumber", trimmedLicenceNumber);
                            command.Parameters.AddWithValue("@LicencePhoto", trimmedLicencefile);
                            command.Parameters.AddWithValue("@Email", Email);
                            command.Parameters.AddWithValue("@Password", encryptedPassword);
                            command.Parameters.AddWithValue("@Phone", Phone);
                            command.ExecuteNonQuery();
                        }
                    }

                    else if (UserType == "Admin")
                    {
                        var query = "INSERT INTO Admins (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Name", Name);
                            command.Parameters.AddWithValue("@Email", Email);
                            command.Parameters.AddWithValue("@Password", encryptedPassword);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error during signup: {ex.Message}");
                    return Page();
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
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task EncryptAndSaveFileAsync(IFormFile file, string outputPath)
{
    // Create a secure key and IV
    var key = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16-byte key for AES-128
    var iv = Encoding.UTF8.GetBytes("abcdef9876543210");  // 16-byte IV

    using (var aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        using (var outputFileStream = new FileStream(outputPath, FileMode.Create))
        using (var cryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write))
        {
            using (var inputStream = file.OpenReadStream())
            {
                await inputStream.CopyToAsync(cryptoStream);
            }
            // CRITICAL: FlushFinalBlock must be called BEFORE disposing the CryptoStream
            cryptoStream.FlushFinalBlock();
        }
    }
}


    }
}