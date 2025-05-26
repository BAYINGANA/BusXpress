using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace BusManagement.Pages.Admin
{
    public class ManageBusesModel : PageModel
    {
        private readonly string _connectionString;

        public ManageBusesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public List<Bus> Buses { get; set; } = new List<Bus>();

        public void OnGet()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT * FROM Buses";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Buses.Add(new Bus
                            {
                                BusId = (int)reader["BusId"],
                                BusNumber = reader["BusNumber"].ToString(),
                                Model = reader["Model"].ToString(),
                                Capacity = (int)reader["Capacity"],
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
        }
        public IActionResult OnPostDelete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "DELETE FROM Buses WHERE BusId = @BusId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusId", id);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/ManageBuses");
        }

    }

    public class Bus
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; }
        public string Model { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
    }
}
