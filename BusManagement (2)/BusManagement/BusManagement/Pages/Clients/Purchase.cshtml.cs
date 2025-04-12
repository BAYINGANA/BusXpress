using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Security.Claims;

namespace BusManagement.Pages.Clients
{
    public class PurchaseModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public PurchaseModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<(int BusId, string Name)> Buses { get; set; } = new();
        public List<(int RouteId, string Name, string Dest)> Routes { get; set; } = new();
        public string? Message { get; set; }

        public void OnGet()
        {
            // Fetch Bus and Route data
            string connString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connString))
            {
                connection.Open();

                // Fetch Buses
                using (var command = new SqlCommand("SELECT BusId, BusNumber FROM Buses", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Buses.Add((reader.GetInt32(0), reader.GetString(1)));
                    }
                }

                // Fetch Routes
                using (var command = new SqlCommand("SELECT RouteId, Origin, Destination FROM Routes", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Routes.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }
                }
            }
        }

        public IActionResult OnPost(int BusId, int RouteId, decimal Price)
        {
            // Get logged-in client's ID from cookies
            var clientId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(clientId)) return RedirectToPage("/Login");

            string connString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();

                string query = "INSERT INTO Tickets (ClientId, BusId, RouteId, Price) VALUES (@ClientId, @BusId, @RouteId, @Price)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.Parameters.AddWithValue("@BusId", BusId);
                    command.Parameters.AddWithValue("@RouteId", RouteId);
                    command.Parameters.AddWithValue("@Price", Price);

                    command.ExecuteNonQuery();
                }
            }

            Message = "Ticket purchased successfully!";
            return Page();
        }
    }
}
