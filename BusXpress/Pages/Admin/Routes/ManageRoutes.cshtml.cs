using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BusManagement.Pages.Admin.Routes
{
    public class ManageRoutesModel : PageModel
    {
        public List<Route> Routes { get; set; } = new List<Route>();

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ManageRoutesModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public void OnGet()
        {
            // Fetch routes from the database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT RouteId, Origin, Destination, Distance, Price FROM Routes";  // Query for Routes table

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Routes.Add(new Route
                        {
                            RouteId = reader.GetInt32(0),
                            Origin = reader.GetString(1),
                            Destination = reader.GetString(2),
                            Distance = reader.GetDecimal(3),
                            Price = reader.GetDecimal(4)
                        });
                    }
                }
            }
        }

        // Handle the deletion of a route
        public IActionResult OnPostDelete(int routeId)
        {
            // Deleting the route from the database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Routes WHERE RouteId = @RouteId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RouteId", routeId);
                    command.ExecuteNonQuery();
                }
            }

            // Redirect to the same page to refresh the routes list
            return RedirectToPage();
        }
    }

    public class Route
    {
        public int RouteId { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public decimal Distance { get; set; }  
        public decimal Price { get; set; }  
    }
}
