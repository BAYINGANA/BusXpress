using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace BusManagement.Pages.Admin.Buses
{
    public class AddBusModel : PageModel
    {
        [BindProperty]
        public string BusNumber { get; set; }

        [BindProperty]
        public int Capacity { get; set; }

        [BindProperty]
        public string Model { get; set; }

        [BindProperty]
        public string Status { get; set; }

        private readonly IConfiguration _configuration;

        public AddBusModel(IConfiguration configuration)
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
                string query = "INSERT INTO Buses (BusNumber, Capacity, Model, Status) " +
                               "VALUES (@BusNumber, @Capacity, @Model, @Status)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusNumber", BusNumber);
                    command.Parameters.AddWithValue("@Capacity", Capacity);
                    command.Parameters.AddWithValue("@Model", Model);
                    command.Parameters.AddWithValue("@Status", Status);

                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Buses/ManageBuses");
        }
    }
}
