using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using MimeKit;
using MailKit.Net.Smtp;

namespace BusManagement.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] public string? UserType { get; set; }
        [BindProperty] public string? Email { get; set; }
        [BindProperty] public string? Password { get; set; }
        public string? ErrorMessage { get; set; }

        private readonly IConfiguration _configuration;
        public LoginModel(IConfiguration configuration) => _configuration = configuration;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // 1) Validate inputs
            if (string.IsNullOrEmpty(UserType) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please fill all fields.";
                return Page();
            }

            // 2) Check credentials
            var encrypted = EncryptPassword(Password);
            if (!TryAuthenticate(UserType!, Email!, encrypted, out int userId, out string role))
            {
                ErrorMessage = "Invalid credentials.";
                return Page();
            }

            // 3) Generate OTP and store in Session
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("MFA_OTP", otp);
            HttpContext.Session.SetString("MFA_UserId", userId.ToString());
            HttpContext.Session.SetString("MFA_UserRole", role);
            HttpContext.Session.SetString("MFA_Email", Email!);

            // 4) Email the OTP
            SendOtpEmail(Email!, otp);

            // 5) Redirect to OTP verification
            return RedirectToPage("/VerifyOtp");
        }

        private bool TryAuthenticate(string userType, string email, string hashedPwd, out int userId, out string role)
        {
            userId = 0; role = "";
            var connStr = _configuration.GetConnectionString("connstring");
            using var conn = new SqlConnection(connStr);
            conn.Open();

            string sql = userType switch
            {
                "Client" => "SELECT ClientId FROM Clients WHERE Email=@e AND Password=@p",
                "Driver" => "SELECT DriverId FROM Drivers WHERE Email=@e AND Password=@p",
                "Admin" => "SELECT AdminId  FROM Admins  WHERE Email=@e AND Password=@p",
                _ => ""
            };
            if (string.IsNullOrEmpty(sql)) return false;
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@p", hashedPwd);
            var res = cmd.ExecuteScalar();
            if (res != null)
            {
                userId = Convert.ToInt32(res);
                role = userType;
                return true;
            }
            return false;
        }

        private string EncryptPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private void SendOtpEmail(string to, string otp)
        {
            // 1) Trim & validate the recipient address
            var trimmed = to?.Trim();
            if (string.IsNullOrEmpty(trimmed) || !MailboxAddress.TryParse(trimmed, out var recipient))
            {
                // You could throw, log, or skip email send if invalid
                throw new ArgumentException($"Invalid email address: '{to}'");
            }

            // 2) Build the message
            var msg = new MimeMessage();
            // Give your from?address a display name + valid mailbox syntax
            msg.From.Add(new MailboxAddress("BusManagement", "h1rhodin@gmail.com"));
            msg.To.Add(recipient);
            msg.Subject = "Your Login Code";
            msg.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {otp}"
            };

            // 3) Send via SMTP
            using var client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate("h1rhodin@gmail.com", "mebl bwjo kuvy rpbq");
            client.Send(msg);
            client.Disconnect(true);
        }

    }
}
