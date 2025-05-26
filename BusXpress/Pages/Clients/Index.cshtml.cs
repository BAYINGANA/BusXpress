using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using BusManagement.Models;

namespace BusManagement.Pages.Clients
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        public string? Email { get; private set; }
        public string? Role { get; private set; }
        public string? UserId { get; private set; }

        // Holds today's schedules
        public List<ScheduleViewModel> TodaySchedules { get; private set; } = new();

        // Summary counts
        public int TodaySchedulesCount { get; private set; }
        public int TodayTicketsCount { get; private set; }

        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnGet()
        {
            // Populate user info
            Email = User.Identity?.Name;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            UserId = User.FindFirst("UserId")?.Value;

            // Database connection
            string connStr = _config.GetConnectionString("connstring")!;
            using var conn = new SqlConnection(connStr);
            conn.Open();

            // 1) Load today's schedules
            const string schedulesSql = @"
                SELECT s.ScheduleId, s.BusId, b.BusNumber,
                       r.Origin, r.Destination,
                       s.DepartureTime, s.ArrivalTime
                FROM Schedule s
                JOIN Buses b   ON s.BusId   = b.BusId
                JOIN Routes r  ON s.RouteId = r.RouteId
                WHERE CAST(s.DepartureTime AS date) = CAST(GETDATE() AS date)
                ORDER BY s.DepartureTime";

            using (var cmd = new SqlCommand(schedulesSql, conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    TodaySchedules.Add(new ScheduleViewModel
                    {
                        ScheduleId = rdr.GetInt32(0),
                        BusId = rdr.GetInt32(1),
                        BusNumber = rdr.GetString(2),
                        RouteId = 0,               // unused here
                        Origin = rdr.GetString(3),
                        Destination = rdr.GetString(4),
                        Price = 0,               // unused
                        DepartureTime = rdr.GetDateTime(5),
                        ArrivalTime = rdr.GetDateTime(6)
                    });
                }
            }

            TodaySchedulesCount = TodaySchedules.Count;

            // 2) Load today's tickets count for this client
            TodayTicketsCount = 0;
            if (int.TryParse(UserId, out int clientId))
            {
                const string ticketsSql = @"
                    SELECT COUNT(*) 
                        FROM Tickets t
                        JOIN Schedule s 
                            ON t.ScheduleId = s.ScheduleId
                        WHERE t.ClientId = @ClientId
                            AND CAST(s.DepartureTime AS date) = CAST(GETDATE() AS date)";

                using var tcmd = new SqlCommand(ticketsSql, conn);
                tcmd.Parameters.AddWithValue("@ClientId", clientId);
                TodayTicketsCount = (int)tcmd.ExecuteScalar()!;
            }
        }
    }
}
