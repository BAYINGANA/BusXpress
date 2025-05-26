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

namespace BusManagement.Pages.Admin
{
    [Authorize(Policy = "Admin")]
    public class ManageDriverLicensesModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public List<Driver> Drivers { get; set; } = new List<Driver>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public bool IsSuccess { get; set; }

        public bool ShowDownloadLink { get; set; }
        public string DownloadLink { get; set; }
        public string SelectedDriverName { get; set; }
        public int ExpiryMinutes { get; set; } = 30;

        public ManageDriverLicensesModel(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnGet(int page = 1, string searchTerm = null)
        {
            CurrentPage = page < 1 ? 1 : page;
            SearchTerm = searchTerm;

            LoadDrivers();
        }

        public IActionResult OnPost(int driverId)
        {
            try
            {
                // Get driver details
                var driver = GetDriverById(driverId);
                if (driver == null)
                {
                    IsSuccess = false;
                    StatusMessage = "Driver not found.";
                    return RedirectToPage();
                }

                // Generate a secure download token
                var expiryTime = DateTime.UtcNow.AddMinutes(ExpiryMinutes);
                var token = GenerateDownloadToken(driverId, expiryTime);

                // Create a full URL for the download link
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                DownloadLink = $"{baseUrl}/LicenseDownload?token={token}";

                ShowDownloadLink = true;
                SelectedDriverName = driver.Name;
                IsSuccess = true;
                StatusMessage = "Download link generated successfully.";

                return Page();
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                StatusMessage = $"Error generating download link: {ex.Message}";
                return RedirectToPage();
            }
        }

        private void LoadDrivers()
        {
            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get total count for pagination
                string countQuery = "SELECT COUNT(*) FROM Drivers";
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    countQuery += " WHERE Name LIKE @Search OR Email LIKE @Search OR LicenceNumber LIKE @Search";
                }

                using (var countCommand = new SqlCommand(countQuery, connection))
                {
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        countCommand.Parameters.AddWithValue("@Search", $"%{SearchTerm}%");
                    }

                    int totalRecords = (int)countCommand.ExecuteScalar();
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
                }

                // Get paginated data
                string query = @"
                    SELECT DriverId, Name, LicenceNumber, LicencePhoto, Email, Phone
                    FROM Drivers
                    {0}
                    ORDER BY Name
                    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

                string whereClause = string.Empty;
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    whereClause = " WHERE Name LIKE @Search OR Email LIKE @Search OR LicenceNumber LIKE @Search";
                }

                query = string.Format(query, whereClause);

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Skip", (CurrentPage - 1) * PageSize);
                    command.Parameters.AddWithValue("@Take", PageSize);

                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{SearchTerm}%");
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Drivers.Add(new Driver
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DriverId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                LicenceNumber = reader.GetString(reader.GetOrdinal("LicenceNumber")),
                                LicencePhoto = reader.IsDBNull(reader.GetOrdinal("LicencePhoto")) ? null : reader.GetString(reader.GetOrdinal("LicencePhoto")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            });
                        }
                    }
                }
            }
        }

        private Driver GetDriverById(int driverId)
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
                            return new Driver
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DriverId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                LicenceNumber = reader.GetString(reader.GetOrdinal("LicenceNumber")),
                                LicencePhoto = reader.IsDBNull(reader.GetOrdinal("LicencePhoto")) ? null : reader.GetString(reader.GetOrdinal("LicencePhoto")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            };
                        }
                    }
                }
            }

            return null;
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

        #region Encryption Methods

        private byte[] GetEncryptionKey()
        {
            string keyStr = _configuration["FileEncryption:Key"] ?? "0123456789ABCDEF0123456789ABCDEF"; // 32 chars for AES-256
            return Encoding.UTF8.GetBytes(keyStr);
        }

        private byte[] GetEncryptionIV()
        {
            string ivStr = _configuration["FileEncryption:IV"] ?? "FEDCBA9876543210"; // 16 chars
            return Encoding.UTF8.GetBytes(ivStr);
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

        #endregion
    }

    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LicenceNumber { get; set; }
        public string LicencePhoto { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}