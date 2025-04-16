using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusXpress.Pages.Admin.Tickets
{
    public class ViewTicketsModel : PageModel
    {
        [BindProperty]
        public int? ClientId { get; set; }
        [BindProperty]
        public int? BusId { get; set; }
        [BindProperty]
        public int? RouteId { get; set; }

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Buses { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Routes { get; set; } = new List<SelectListItem>();

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ViewTicketsModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("connstring");
        }

        public void OnGet()
        {
            // Fetch available clients, buses, and routes for the dropdowns
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Get clients
                string clientQuery = "SELECT ClientId, Name FROM Clients";
                using (var command = new SqlCommand(clientQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Clients.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }

                // Get buses
                string busQuery = "SELECT BusId, BusNumber FROM Buses";
                using (var command = new SqlCommand(busQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Buses.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }

                // Get routes
                string routeQuery = "SELECT RouteId, Origin, Destination FROM Routes";
                using (var command = new SqlCommand(routeQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Routes.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1) + " to " + reader.GetString(2)
                        });
                    }
                }
            }

            // Fetch tickets with applied filters if any
            string query = "SELECT TicketId, ClientId, BusId, RouteId, DateIssued, Price FROM Tickets WHERE 1=1";

            if (ClientId.HasValue)
                query += " AND ClientId = @ClientId";
            if (BusId.HasValue)
                query += " AND BusId = @BusId";
            if (RouteId.HasValue)
                query += " AND RouteId = @RouteId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    if (ClientId.HasValue)
                        command.Parameters.AddWithValue("@ClientId", ClientId);
                    if (BusId.HasValue)
                        command.Parameters.AddWithValue("@BusId", BusId);
                    if (RouteId.HasValue)
                        command.Parameters.AddWithValue("@RouteId", RouteId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tickets.Add(new Ticket
                            {
                                TicketId = reader.GetInt32(0),
                                ClientId = reader.GetInt32(1),
                                BusId = reader.GetInt32(2),
                                RouteId = reader.GetInt32(3),
                                DateIssued = reader.GetDateTime(4),
                                Price = reader.GetDecimal(5)
                            });
                        }
                    }
                }
            }
        }

        // Handle exporting to CSV
        public IActionResult OnGetExportToCSV()
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("TicketId,ClientId,BusId,RouteId,DateIssued,Price");

            foreach (var ticket in Tickets)
            {
                csv.AppendLine($"{ticket.TicketId},{ticket.ClientId},{ticket.BusId},{ticket.RouteId},{ticket.DateIssued:yyyy-MM-dd},{ticket.Price}");
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(csv.ToString());
            return File(byteArray, "text/csv", "TicketsReport.csv");
        }

        public class Ticket
        {
            public int TicketId { get; set; }
            public int ClientId { get; set; }
            public int BusId { get; set; }
            public int RouteId { get; set; }
            public DateTime DateIssued { get; set; }
            public decimal Price { get; set; }
        }
    }
}
