using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BusManagement.Pages.Clients
{
    public class ExportToPdfModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ExportToPdfModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet(int ticketId)
        {
            var clientId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(clientId)) return RedirectToPage("/Login");

            // Fetch ticket details
            var ticket = GetTicket(ticketId, clientId);
            if (ticket == null) return NotFound();

            // Generate PDF
            var document = new Document();
            var stream = new MemoryStream();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            if (ticket.HasValue)
            {
                document.Add(new Paragraph("Bus Ticket Details"));
                document.Add(new Paragraph($"Ticket ID: {ticket.Value.TicketId}"));
                document.Add(new Paragraph($"Bus: {ticket.Value.BusName}"));
                document.Add(new Paragraph($"Route: {ticket.Value.RouteName}"));
                document.Add(new Paragraph($"Date Issued: {ticket.Value.DateIssued}"));
                document.Add(new Paragraph($"Price: ${ticket.Value.Price}"));
            }
            else
            {
                document.Add(new Paragraph("No ticket found."));
            }

            document.Close();
            var bytes = stream.ToArray();

            return File(bytes, "application/pdf", $"Ticket_{ticketId}.pdf");
        }

        private (int TicketId, string BusName, string RouteName, string RouteDestination, DateTime DateIssued, decimal Price)? GetTicket(int ticketId, string clientId)
        {
            string connString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                string query = @"
                    SELECT t.TicketId, b.BusNumber AS BusName, r.Origin AS RouteName, r.Destination As RouteDestination, t.DateIssued, t.Price
                    FROM Tickets t
                    JOIN Buses b ON t.BusId = b.BusId
                    JOIN Routes r ON t.RouteId = r.RouteId
                    WHERE t.TicketId = @TicketId AND t.ClientId = @ClientId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TicketId", ticketId);
                    command.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetDateTime(4),
                                reader.GetDecimal(5)
                            );
                        }
                    }
                }
            }
            return null;
        }
    }
}
