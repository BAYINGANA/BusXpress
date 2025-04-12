using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

namespace BusManagement.Pages.Admin.AssignDrivers
{
    public class ManageAssignDriversModel : PageModel
    {
        public List<DriverAssignment> Assignments { get; set; } = new List<DriverAssignment>();
        public List<Driver> Drivers { get; set; } = new List<Driver>();
        public List<Bus> Buses { get; set; } = new List<Bus>();

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ManageAssignDriversModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public void OnGet()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Fetch assignments
                string assignmentsQuery = @"
                    SELECT da.AssignmentId, da.DriverId, d.Name AS DriverName, 
                           da.BusId, b.BusNumber, da.AssignmentDate, da.Status 
                    FROM DriverAssignments da
                    JOIN Drivers d ON da.DriverId = d.DriverId
                    JOIN Buses b ON da.BusId = b.BusId";

                using (var command = new SqlCommand(assignmentsQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assignments.Add(new DriverAssignment
                        {
                            AssignmentId = reader.GetInt32(0),
                            DriverId = reader.GetInt32(1),
                            DriverName = reader.GetString(2),
                            BusId = reader.GetInt32(3),
                            BusNumber = reader.GetString(4),
                            AssignmentDate = reader.GetDateTime(5),
                            Status = reader.GetString(6)
                        });
                    }
                }

                // Fetch drivers
                string driversQuery = "SELECT DriverId, Name FROM Drivers";
                using (var command = new SqlCommand(driversQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Drivers.Add(new Driver
                        {
                            DriverId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }

                // Fetch buses
                string busesQuery = @"
                    SELECT BusId, BusNumber 
                    FROM Buses 
                    WHERE BusId NOT IN (SELECT BusId FROM DriverAssignments WHERE Status = 'Active')";
                using (var command = new SqlCommand(busesQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Buses.Add(new Bus
                        {
                            BusId = reader.GetInt32(0),
                            BusNumber = reader.GetString(1)
                        });
                    }
                }
            }
        }

        public IActionResult OnPostAssignDriver(int driverId, int busId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Ensure the bus doesn't already have a driver
                string checkBusQuery = "SELECT COUNT(*) FROM DriverAssignments WHERE BusId = @BusId AND Status = 'Active'";
                using (var checkBusCommand = new SqlCommand(checkBusQuery, connection))
                {
                    checkBusCommand.Parameters.AddWithValue("@BusId", busId);
                    int busCount = (int)checkBusCommand.ExecuteScalar();
                    if (busCount > 0)
                    {
                        ModelState.AddModelError(string.Empty, "This bus already has an active driver.");
                        return RedirectToPage();
                    }
                }

                // Ensure the driver is not already assigned to another bus
                string checkDriverQuery = "SELECT COUNT(*) FROM DriverAssignments WHERE DriverId = @DriverId AND Status = 'Active'";
                using (var checkDriverCommand = new SqlCommand(checkDriverQuery, connection))
                {
                    checkDriverCommand.Parameters.AddWithValue("@DriverId", driverId);
                    int driverCount = (int)checkDriverCommand.ExecuteScalar();
                    if (driverCount > 0)
                    {
                        ModelState.AddModelError(string.Empty, "This driver is already assigned to another active bus.");
                        return RedirectToPage();
                    }
                }

                // Assign the driver to the bus
                string insertQuery = @"
            INSERT INTO DriverAssignments (DriverId, BusId, AssignmentDate, Status)
            VALUES (@DriverId, @BusId, @AssignmentDate, @Status)";

                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@DriverId", driverId);
                    command.Parameters.AddWithValue("@BusId", busId);
                    command.Parameters.AddWithValue("@AssignmentDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Status", "Active");
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage();
        }


        public IActionResult OnPostRemoveAssignment(int assignmentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Remove the driver assignment
                string query = "DELETE FROM DriverAssignments WHERE AssignmentId = @AssignmentId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AssignmentId", assignmentId);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage();
        }

        public class DriverAssignment
        {
            public int AssignmentId { get; set; }
            public int DriverId { get; set; }
            public string DriverName { get; set; }
            public int BusId { get; set; }
            public string BusNumber { get; set; }
            public DateTime AssignmentDate { get; set; }
            public string Status { get; set; }
        }

        public class Driver
        {
            public int DriverId { get; set; }
            public string Name { get; set; }
        }

        public class Bus
        {
            public int BusId { get; set; }
            public string BusNumber { get; set; }
        }
    }
}
