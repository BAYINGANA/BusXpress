using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace BusManagement.Pages.Admin.Schedules
{
    public class EditScheduleModel : PageModel
    {
        [BindProperty]
        public int ScheduleId { get; set; }

        [BindProperty]
        public int BusId { get; set; }

        [BindProperty]
        public int RouteId { get; set; }

        [BindProperty]
        public DateTime DepartureTime { get; set; }

        [BindProperty]
        public DateTime ArrivalTime { get; set; }

        private readonly IConfiguration _configuration;

        public EditScheduleModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(int scheduleId)
        {
            string connectionString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Schedule WHERE ScheduleId = @ScheduleId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        ScheduleId = reader.GetInt32(0);
                        BusId = reader.GetInt32(1);
                        RouteId = reader.GetInt32(2);
                        DepartureTime = reader.GetDateTime(3);
                        ArrivalTime = reader.GetDateTime(4);
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
                string query = "UPDATE Schedule SET BusId = @BusId, RouteId = @RouteId, DepartureTime = @DepartureTime, ArrivalTime = @ArrivalTime WHERE ScheduleId = @ScheduleId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScheduleId", ScheduleId);
                    command.Parameters.AddWithValue("@BusId", BusId);
                    command.Parameters.AddWithValue("@RouteId", RouteId);
                    command.Parameters.AddWithValue("@DepartureTime", DepartureTime);
                    command.Parameters.AddWithValue("@ArrivalTime", ArrivalTime);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Schedules/ManageSchedules");
        }
    }
}
