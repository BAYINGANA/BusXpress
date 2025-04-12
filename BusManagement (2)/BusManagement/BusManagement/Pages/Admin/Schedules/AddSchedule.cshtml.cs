using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace BusManagement.Pages.Admin.Schedules
{
    public class AddScheduleModel : PageModel
    {
        [BindProperty]
        public int BusId { get; set; }

        [BindProperty]
        public int RouteId { get; set; }

        [BindProperty]
        public DateTime DepartureTime { get; set; }

        [BindProperty]
        public DateTime ArrivalTime { get; set; }

        public List<Bus> Buses { get; set; } = new();
        public List<Route> Routes { get; set; } = new();

        private readonly IConfiguration _configuration;

        public AddScheduleModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("connstring");

            // Fetch available buses
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT BusId, BusNumber FROM Buses";  // Assumes Bus table exists
                using (var command = new SqlCommand(query, connection))
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

            // Fetch available routes
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT RouteId, Origin, Destination FROM Routes";  // Assumes Routes table exists
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Routes.Add(new Route
                        {
                            RouteId = reader.GetInt32(0),
                            Origin = reader.GetString(1),
                            Destination = reader.GetString(2)
                        });
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("connstring");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Schedule (BusId, RouteId, DepartureTime, ArrivalTime) " +
                               "VALUES (@BusId, @RouteId, @DepartureTime, @ArrivalTime)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusId", BusId);
                    command.Parameters.AddWithValue("@RouteId", RouteId);
                    command.Parameters.AddWithValue("@DepartureTime", DepartureTime);
                    command.Parameters.AddWithValue("@ArrivalTime", ArrivalTime);

                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Schedules/ManageSchedules");
        }

        // Models for Bus and Route
        public class Bus
        {
            public int BusId { get; set; }
            public string BusNumber { get; set; }
        }

        public class Route
        {
            public int RouteId { get; set; }
            public string Origin { get; set; }
            public string Destination { get; set; }
        }
    }
}
