using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BusManagement.Pages.Admin.Buses
{
    public class ManageBusesModel : PageModel
    {
        public List<Bus> Buses { get; set; } = new List<Bus>();

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ManageBusesModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public void OnGet()
        {
            // Fetch buses
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT BusId, BusNumber, Capacity, Model, Status FROM Buses";  // Query for Buses table

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Buses.Add(new Bus
                        {
                            BusId = reader.GetInt32(0),
                            BusNumber = reader.GetString(1),
                            Capacity = reader.GetInt32(2),
                            Model = reader.GetString(3),
                            Status = reader.GetString(4)
                        });
                    }
                }
            }
        }

        // Handle the deletion of a bus
        public IActionResult OnPostDelete(int busId)
        {
            // Deleting the bus from the database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Buses WHERE BusId = @BusId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusId", busId);
                    command.ExecuteNonQuery();
                }
            }

            // Redirect to the same page to refresh the bus list
            return RedirectToPage();
        }
    }

    public class Bus
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; }
        public int Capacity { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
    }
}
