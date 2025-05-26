using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace BusManagement.Pages.Admin.Routes
{
    public class AddRouteModel : PageModel
    {
        [BindProperty]
        public string Origin { get; set; }

        [BindProperty]
        public string Destination { get; set; }

        [BindProperty]
        public decimal Distance { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        private readonly IConfiguration _configuration;

        public AddRouteModel(IConfiguration configuration)
        {
            _configuration = configuration;
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
                string query = "INSERT INTO Routes (Origin, Destination, Distance, Price) " +
                               "VALUES (@Origin, @Destination, @Distance, @Price)";

                using (var command = new SqlCommand(query, connection))
                {
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
