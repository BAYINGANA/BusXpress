using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace BusManagement.Pages.Admin.Routes
{
    public class EditRouteModel : PageModel
    {
        [BindProperty]
        public int RouteId { get; set; }

        [BindProperty]
        public string Origin { get; set; }

        [BindProperty]
        public string Destination { get; set; }

        [BindProperty]
        public decimal Distance { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        private readonly IConfiguration _configuration;

        public EditRouteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(int routeId)
        {
            string connectionString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Routes WHERE RouteId = @RouteId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RouteId", routeId);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        RouteId = reader.GetInt32(0);
                        Origin = reader.GetString(1);
                        Destination = reader.GetString(2);
                        Distance = reader.GetDecimal(3);
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
                string query = "UPDATE Routes SET Origin = @Origin, Destination = @Destination, Distance = @Distance, Price = @Price WHERE RouteId = @RouteId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RouteId", RouteId);
                    command.Parameters.AddWithValue("@Origin", Origin);
                    command.Parameters.AddWithValue("@Destination", Destination);
                    command.Parameters.AddWithValue("@Distance", Distance);
                    command.Parameters.AddWithValue("@Price", Price);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Routes/ManageRoutes");
        }
    }
}
