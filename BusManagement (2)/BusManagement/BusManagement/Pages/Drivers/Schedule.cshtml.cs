using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;

namespace BusManagement.Pages.Drivers
{
    public class ScheduleModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // Property to hold the list of assignments (replacing schedules)
        public List<DriverAssignment> DriverAssignments { get; set; }

        // Constructor to inject IConfiguration
        public ScheduleModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            // Get DriverId from Claims
            var driverId = Convert.ToInt32(User.FindFirstValue("UserId"));
            string connectionString = _configuration.GetConnectionString("connstring");

            DriverAssignments = new List<DriverAssignment>(); // Initialize the list

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Updated query to select from DriverAssignments table
                string query = @"
                    SELECT da.AssignmentId, b.BusNumber, d.Name, da.AssignmentDate, da.Status
                    FROM DriverAssignments da
                    JOIN Buses b ON da.BusId = b.BusId
                    JOIN Drivers d ON da.DriverId = d.DriverId
                    WHERE da.DriverId = @DriverId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DriverId", driverId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DriverAssignments.Add(new DriverAssignment
                            {
                                AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
                                BusName = reader["BusNumber"].ToString(),
                                DriverName = reader["Name"].ToString(),
                                AssignmentDate = Convert.ToDateTime(reader["AssignmentDate"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
        }
    }

    // DriverAssignment class for mapping the data
    public class DriverAssignment
    {
        public int AssignmentId { get; set; }
        public string BusName { get; set; }
        public string DriverName { get; set; } // Assuming DriverName is in the Drivers table
        public DateTime AssignmentDate { get; set; }
        public string Status { get; set; }
    }
}
