using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace BusManagement.Pages.Admin.Buses
{
    public class EditBusModel : PageModel
    {
        private readonly string _connectionString;

        public EditBusModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connstring");
        }

        [BindProperty]
        public BusViewModel Bus { get; set; }

        // Fetch the bus data when the page loads
        public void OnGet(int busId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT BusId, BusNumber, Capacity, Model, Status FROM Buses WHERE BusId = @BusId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusId", busId);

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        Bus = new BusViewModel
                        {
                            BusId = reader.GetInt32(0),
                            BusNumber = reader.GetString(1),
                            Capacity = reader.GetInt32(2),
                            Model = reader.GetString(3),
                            Status = reader.GetString(4)
                        };
                    }
                }
            }
        }

        // Handle form submission to update the bus details
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update the bus information in the database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE Buses SET BusNumber = @BusNumber, Capacity = @Capacity, Model = @Model, Status = @Status WHERE BusId = @BusId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusId", Bus.BusId);
                    command.Parameters.AddWithValue("@BusNumber", Bus.BusNumber);
                    command.Parameters.AddWithValue("@Capacity", Bus.Capacity);
                    command.Parameters.AddWithValue("@Model", Bus.Model);
                    command.Parameters.AddWithValue("@Status", Bus.Status);

                    command.ExecuteNonQuery();
                }
            }

            // Redirect to the manage buses page after successful update
            return RedirectToPage("/Admin/Buses/ManageBuses");
        }

        // Bus ViewModel for binding data
        public class BusViewModel
        {
            public int BusId { get; set; }
            public string BusNumber { get; set; }
            public int Capacity { get; set; }
            public string Model { get; set; }
            public string Status { get; set; }
        }
    }
}
