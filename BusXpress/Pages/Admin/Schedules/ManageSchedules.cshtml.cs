using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BusManagement.Pages.Admin.Schedules
{
    public class ManageSchedulesModel : PageModel
    {
        public List<Schedule> Schedules { get; set; } = new();

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ManageSchedulesModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public void OnGet()
        {
            // Fetch schedules
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT s.ScheduleId, b.BusNumber, r.Origin, r.Destination, s.DepartureTime, s.ArrivalTime " +
                    "FROM Schedule s " +
                    "JOIN Buses b ON s.BusId = b.BusId " +
                    "JOIN Routes r ON s.RouteId = r.RouteId";  

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Schedules.Add(new Schedule
                        {
                            ScheduleId = reader.GetInt32(0),
                            BusNumber = reader.GetString(1),
                            Origin = reader.GetString(2),
                            Destination = reader.GetString(3),
                            DepartureTime = reader.GetDateTime(4),
                            ArrivalTime = reader.GetDateTime(5)
                        });
                    }
                }
            }
        }

        // Handle the deletion of a schedule
        public IActionResult OnPostDelete(int scheduleId)
        {
            // Deleting the schedule from the database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Schedule WHERE ScheduleId = @ScheduleId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                    command.ExecuteNonQuery();
                }
            }

            // Redirect to the same page to refresh the schedule list
            return RedirectToPage();
        }
    }

    public class Schedule
    {
        public int ScheduleId { get; set; }
        public string BusNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
