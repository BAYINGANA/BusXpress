using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using BusManagement.Models;

namespace BusManagement.Pages.Drivers
{
    [Authorize(Roles = "Driver")]
    public class ScheduleModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<ScheduleViewModel> Schedule { get; private set; } = new();

        public ScheduleModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            // Get driver ID from claims
            var driverIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(driverIdClaim, out int driverId))
                return;

            string connStr = _configuration.GetConnectionString("connstring")!;
            using var conn = new SqlConnection(connStr);
            conn.Open();

            // Join Schedule to DriverAssignments to filter only assigned buses
            const string sql = @"
                SELECT 
                    s.ScheduleId, s.BusId, b.BusNumber,
                    s.RouteId, r.Origin, r.Destination,
                    s.DepartureTime, s.ArrivalTime
                FROM Schedule s
                JOIN Buses b    ON s.BusId   = b.BusId
                JOIN Routes r   ON s.RouteId = r.RouteId
                JOIN DriverAssignments da
                  ON da.BusId = s.BusId
                WHERE da.DriverId = @DriverId
                  AND da.Status = 'Active'
                ORDER BY s.DepartureTime";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@DriverId", driverId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Schedule.Add(new ScheduleViewModel
                {
                    ScheduleId = reader.GetInt32(0),
                    BusId = reader.GetInt32(1),
                    BusNumber = reader.GetString(2),
                    RouteId = reader.GetInt32(3),
                    Origin = reader.GetString(4),
                    Destination = reader.GetString(5),
                    Price = 0, // not needed here
                    DepartureTime = reader.GetDateTime(6),
                    ArrivalTime = reader.GetDateTime(7)
                });
            }
        }
    }
}
