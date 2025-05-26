using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

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

            // Define fonts that will be used irrespective of condition
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
            var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

            if (ticket.HasValue)
            {
                // Add Title
                var titleParagraph = new Paragraph("BUS TICKET", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(titleParagraph);

                // Create a table with 2 columns for ticket details
                PdfPTable table = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };

                // Helper function to add a cell
                void AddCell(PdfPTable tbl, string text, Font font)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(text, font))
                    {
                        Border = Rectangle.NO_BORDER,
                        Padding = 5
                    };
                    tbl.AddCell(cell);
                }

                // Add rows with ticket details
                AddCell(table, "Ticket ID:", labelFont);
                AddCell(table, ticket.Value.TicketId.ToString(), normalFont);

                AddCell(table, "Bus:", labelFont);
                AddCell(table, ticket.Value.BusName, normalFont);

                AddCell(table, "Route:", labelFont);
                AddCell(table, $"{ticket.Value.RouteName} - {ticket.Value.RouteDestination}", normalFont);

                AddCell(table, "Price:", labelFont);
                AddCell(table, $"{ticket.Value.Price:F2} Frw", normalFont);

                AddCell(table, "Duration:", labelFont);
                string duration = $"{ticket.Value.DepartureTime:yyyy-MM-dd HH:mm} - {ticket.Value.ArrivalTime:HH:mm}";
                AddCell(table, duration, normalFont);

                AddCell(table, "Date Issued:", labelFont);
                AddCell(table, ticket.Value.DateIssued.ToString("yyyy-MM-dd HH:mm"), normalFont);

                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("No ticket found.", normalFont));
            }

            document.Close();
            var bytes = stream.ToArray();

            return File(bytes, "application/pdf", $"Ticket_{ticketId}.pdf");
        }

        private (int TicketId, string BusName, string RouteName, string RouteDestination, decimal Price, DateTime DepartureTime, DateTime ArrivalTime, DateTime DateIssued)? GetTicket(int ticketId, string clientId)
        {
            string connString = _configuration.GetConnectionString("connstring");
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                string query = @"
                    SELECT t.TicketId, b.BusNumber AS BusName, r.Origin AS RouteName, r.Destination AS RouteDestination, r.Price AS Price, 
                           s.DepartureTime AS DepartureTime, s.ArrivalTime AS ArrivalTime, t.DateIssued 
                    FROM Tickets t 
                    JOIN Schedule s ON t.ScheduleId = s.ScheduleId
                    JOIN Buses b ON s.BusId = b.BusId
                    JOIN Routes r ON s.RouteId = r.RouteId
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
                                reader.GetDecimal(4),
                                reader.GetDateTime(5),
                                reader.GetDateTime(6),
                                reader.GetDateTime(7)
                            );
                        }
                    }
                }
            }
            return null;
        }
    }
}
