using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BusXpress.Pages.Admin.Routes
{
    public class AddRouteModel : PageModel
    {
        [BindProperty]
        public string Origin { get; set; }

        [BindProperty]
        public string Destination { get; set; }

        [BindProperty]
        public decimal Distance { get; set; }

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
                string query = "INSERT INTO Routes (Origin, Destination, Distance) " +
                               "VALUES (@Origin, @Destination, @Distance)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Origin", Origin);
                    command.Parameters.AddWithValue("@Destination", Destination);
                    command.Parameters.AddWithValue("@Distance", Distance);

                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Routes/ManageRoutes");
        }
    }
}
