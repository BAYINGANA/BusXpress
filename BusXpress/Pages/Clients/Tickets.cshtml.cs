using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace BusManagement.Pages.Clients
{
    public class TicketsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public TicketsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<(int TicketId, string BusName, string RouteName, string RouteDestination, decimal Price, DateTime DepartureTime, DateTime ArrivalTime, DateTime DateIssued)> Tickets { get; set; } = new();

        public void OnGet()
        {
            var clientId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(clientId)) return;

            string connString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                string query = @"
                    SELECT t.TicketId, b.BusNumber AS BusName, r.Origin AS RouteName, r.Destination AS RouteDestination, r.Price AS Price, s.DepartureTime AS DepartureTime, s.ArrivalTime AS ArrivalTime, t.DateIssued 
                    FROM Tickets t 
                    JOIN Schedule s ON t.ScheduleId = s.ScheduleId
		            JOIN Buses b ON s.BusId = b.BusId
                    JOIN Routes r ON s.RouteId = r.RouteId
                    WHERE t.ClientId = @ClientId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tickets.Add((
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetDecimal(4),
                                reader.GetDateTime(5),
                                reader.GetDateTime(6),
                                reader.GetDateTime(7)
                            ));
                        }
                    }
                }
            }
        }
    }
}
