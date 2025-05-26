using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace BusManagement.Pages
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LicenceNumber { get; set; }
        public string LicencePhoto { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    [Authorize(Policy = "LicenseAccess")]
    public class LicenseDownloadModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string TokenValidationMessage { get; set; }

        public Driver DriverDetails { get; set; }
        public bool ShowLicenseDetails { get; set; }
        public bool IsLinkActive { get; set; }
        public string SecureDownloadUrl { get; set; }
        public int ExpiryMinutes { get; set; } = 30; // Default expiry time in minutes

        public LicenseDownloadModel(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult OnGet(string token = null)
        {
            ShowLicenseDetails = false;

            // Check if we have a download token
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenData = ValidateAndDecodeToken(token);
                    if (tokenData != null)
                    {
                        int driverId = Convert.ToInt32(tokenData["driverId"]);
                        LoadDriverDetails(driverId);

                        // Check if token is still valid
                        DateTime expiryTime = DateTime.Parse(tokenData["expiry"]);
                        if (DateTime.UtcNow <= expiryTime)
                        {
                            // If there's a download parameter, handle the direct download
                            if (Request.Query.ContainsKey("download"))
                            {
                                return HandleFileDownload(driverId);
                            }

                            ShowLicenseDetails = true;
                            IsLinkActive = true;
                            SecureDownloadUrl = $"/LicenseDownload?token={token}&download=true";
                        }
                        else
                        {
                            TokenValidationMessage = "This download link has expired. Please generate a new link.";
                            ShowLicenseDetails = true;
                            IsLinkActive = false;
                        }
                    }
                    else
                    {
                        TokenValidationMessage = "Invalid download link. Please request a new one.";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error processing download link: {ex.Message}";
                }
            }

            return Page();
        }

        public IActionResult OnPostAsync(int driverId)
        {
            try
            {
                LoadDriverDetails(driverId);

                if (DriverDetails != null)
                {
                    ShowLicenseDetails = true;

                    // Generate a secure download token
                    var expiryTime = DateTime.UtcNow.AddMinutes(ExpiryMinutes);
                    var token = GenerateDownloadToken(driverId, expiryTime);

                    IsLinkActive = true;
                    SecureDownloadUrl = $"/LicenseDownload?token={token}";
                }
                else
                {
                    ErrorMessage = "Driver not found.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating download link: {ex.Message}";
            }

            return Page();
        }

        public IActionResult OnPostSearchAsync(string driverEmail, string licenseNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(driverEmail) && string.IsNullOrEmpty(licenseNumber))
                {
                    ErrorMessage = "Please provide either driver email or license number.";
                    return Page();
                }

                int? driverId = FindDriverId(driverEmail, licenseNumber);

                if (driverId.HasValue)
                {
                    LoadDriverDetails(driverId.Value);
                    ShowLicenseDetails = true;
                }
                else
                {
                    ErrorMessage = "No driver found with the provided information.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error searching for driver: {ex.Message}";
            }

            return Page();
        }

        private int? FindDriverId(string email, string licenseNumber)
        {
            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT DriverId FROM Drivers WHERE ";
                SqlCommand command;

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(licenseNumber))
                {
                    query += "Email = @Email AND LicenceNumber = @LicenceNumber";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@LicenceNumber", licenseNumber);
                }
                else if (!string.IsNullOrEmpty(email))
                {
                    query += "Email = @Email";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Email", email);
                }
                else
                {
                    query += "LicenceNumber = @LicenceNumber";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@LicenceNumber", licenseNumber);
                }

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            return null;
        }

        private void LoadDriverDetails(int driverId)
        {
            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT DriverId, Name, LicenceNumber, LicencePhoto, Email, Phone FROM Drivers WHERE DriverId = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", driverId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DriverDetails = new Driver
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DriverId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                LicenceNumber = reader.GetString(reader.GetOrdinal("LicenceNumber")),
                                LicencePhoto = reader.GetString(reader.GetOrdinal("LicencePhoto")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            };

                            ShowLicenseDetails = true;
                        }
                    }
                }
            }
        }

        private IActionResult HandleFileDownload(int driverId)
        {
            try
            {
                LoadDriverDetails(driverId);
                if (DriverDetails == null || string.IsNullOrEmpty(DriverDetails.LicencePhoto))
                {
                    return NotFound("License photo not found.");
                }

                // Get the file path from the database path
                string filePath = Path.Combine(_environment.WebRootPath, DriverDetails.LicencePhoto.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("License photo file not found.");
                }

                // Get original file extension
                string fileExtension = Path.GetExtension(filePath);

                // Create a temporary file for the decrypted content
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"dec_{Guid.NewGuid()}{fileExtension}");

                try
                {
                    // Decrypt the file
                    DecryptFile(filePath, tempFilePath);

                    // Read the decrypted file
                    byte[] fileBytes = System.IO.File.ReadAllBytes(tempFilePath);

                    // Delete the temporary file
                    System.IO.File.Delete(tempFilePath);

                    // Determine content type
                    string contentType = "image/jpeg"; // Default
                    if (fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        contentType = "image/png";
                    }
                    else if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        contentType = "application/pdf";
                    }

                    // Return the file
                    string fileName = $"License_{DriverDetails.LicenceNumber}{fileExtension}";
                    return File(fileBytes, contentType, fileName);
                }
                catch (Exception ex)
                {
                    // Clean up if an error occurs
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    throw new Exception($"Error decrypting file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error downloading file: {ex.Message}";
                return RedirectToPage();
            }
        }

        private string GenerateDownloadToken(int driverId, DateTime expiryTime)
        {
            // Create a dictionary with the claims to include in the token
            var tokenData = new Dictionary<string, string>
            {
                { "driverId", driverId.ToString() },
                { "expiry", expiryTime.ToString("o") } // ISO 8601 format
            };

            // Serialize the dictionary to JSON
            string jsonData = System.Text.Json.JsonSerializer.Serialize(tokenData);

            // Encrypt the token data
            byte[] encryptedData = EncryptStringToBytes(jsonData, GetEncryptionKey(), GetEncryptionIV());

            // Base64Url encode the encrypted data to make it URL-safe
            string token = WebEncoders.Base64UrlEncode(encryptedData);

            return token;
        }

        private Dictionary<string, string> ValidateAndDecodeToken(string token)
        {
            try
            {
                // Base64Url decode the token
                byte[] encryptedData = WebEncoders.Base64UrlDecode(token);

                // Decrypt the token
                string jsonData = DecryptStringFromBytes(encryptedData, GetEncryptionKey(), GetEncryptionIV());

                // Deserialize the JSON back to a dictionary
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
            }
            catch
            {
                return null; // Invalid token
            }
        }

        #region Encryption/Decryption Methods

        private byte[] GetEncryptionKey()
        {
            // Make sure this matches EXACTLY what was used in SignupModel.cs
            string keyStr = "0123456789abcdef"; // Must be exactly 16 bytes for AES-128
            return Encoding.UTF8.GetBytes(keyStr);
        }

        private byte[] GetEncryptionIV()
        {
            // Make sure this matches EXACTLY what was used in SignupModel.cs
            string ivStr = "abcdef9876543210"; // Must be exactly 16 bytes
            return Encoding.UTF8.GetBytes(ivStr);
        }

        private void DecryptFile(string inputFile, string outputFile)
{
    // Create the same key and IV used for encryption
    var key = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16-byte key for AES-128
    var iv = Encoding.UTF8.GetBytes("abcdef9876543210");  // 16-byte IV

    using (var aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using (var inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
        using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        using (var cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read))
        using (var outputFileStream = new FileStream(outputFile, FileMode.Create))
        {
            try
            {
                cryptoStream.CopyTo(outputFileStream);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException($"Decryption failed. This could indicate the file was not properly encrypted or the key/IV don't match. Details: {ex.Message}", ex);
            }
        }
    }
}


        private byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }

        private string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipherText))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        #endregion
    }
}