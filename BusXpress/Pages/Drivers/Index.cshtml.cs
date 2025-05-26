using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace BusManagement.Pages.Drivers
{
    [Authorize(Roles = "Driver")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string? Email { get; private set; }
        public string? Role { get; private set; }
        public string? UserId { get; private set; }

        // Summary counts
        public int AssignedBusesCount { get; private set; }
        public int TodaySchedulesCount { get; private set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            // 1) Populate user info
            Email = User.Identity?.Name;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            UserId = User.FindFirst("UserId")?.Value;

            if (!int.TryParse(UserId, out int driverId))
                return;

            string connString = _configuration.GetConnectionString("connstring")!;
            using var conn = new SqlConnection(connString);
            conn.Open();

            // 2) Count assigned buses (distinct, active)
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(DISTINCT da.BusId)
                FROM DriverAssignments da
                WHERE da.DriverId = @DriverId
                  AND da.Status = 'Active'
            ", conn))
            {
                cmd.Parameters.AddWithValue("@DriverId", driverId);
                AssignedBusesCount = (int)cmd.ExecuteScalar()!;
            }

            // 3) Count today's schedules on those buses
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Schedule s
                WHERE CAST(s.DepartureTime AS date) = CAST(GETDATE() AS date)
                  AND s.BusId IN (
                      SELECT BusId
                      FROM DriverAssignments
                      WHERE DriverId = @DriverId
                        AND Status = 'Active'
                  )
            ", conn))
            {
                cmd.Parameters.AddWithValue("@DriverId", driverId);
                TodaySchedulesCount = (int)cmd.ExecuteScalar()!;
            }
        }
    }
}
