using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using BusManagement.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MimeKit;

namespace BusManagement.Pages.Clients
{
    public class PurchaseModel : PageModel
    {
        private readonly IConfiguration _config;
        public PurchaseModel(IConfiguration config) => _config = config;

        // All future schedules
        public List<ScheduleViewModel> AllSchedules { get; set; } = new();

        // Distinct "Origin - Destination"
        public List<string> Routes { get; set; } = new();

        // Filtered lists
        public List<ScheduleViewModel> FilteredByRoute { get; set; } = new();
        public List<ScheduleViewModel> FilteredByBus { get; set; } = new();

        // Bind GET
        [BindProperty(SupportsGet = true)]
        public string? SelectedRoute { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? SelectedBusId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? SelectedScheduleId { get; set; }

        // Purchase form bound
        [BindProperty]
        public string? EmailOverride { get; set; }

        public string? Message { get; set; }
        public string? DownloadLink { get; set; }

        public ScheduleViewModel? SelectedSchedule { get; set; }
        public ScheduleViewModel? Current { get; set; }

        public void OnGet()
        {
            var conn = new SqlConnection(_config.GetConnectionString("connstring"));
            conn.Open();
            // load all future schedules, with capacity + tickets sold
            var sql = @"
                SELECT 
                  s.ScheduleId, s.BusId, b.BusNumber, b.Capacity,
                  s.RouteId, r.Origin, r.Destination, r.Price,
                  s.DepartureTime, s.ArrivalTime,
                  (SELECT COUNT(*) FROM Tickets t WHERE t.ScheduleId = s.ScheduleId) AS TicketsSold
                FROM Schedule s
                JOIN Buses b   ON s.BusId = b.BusId
                JOIN Routes r  ON s.RouteId = r.RouteId
                WHERE s.DepartureTime > GETDATE()
                ORDER BY r.Origin, r.Destination, b.BusNumber, s.DepartureTime";
            using var cmd = new SqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                AllSchedules.Add(new ScheduleViewModel
                {
                    ScheduleId = rdr.GetInt32(0),
                    BusId = rdr.GetInt32(1),
                    BusNumber = rdr.GetString(2),
                    Capacity = rdr.GetInt32(3),
                    RouteId = rdr.GetInt32(4),
                    Origin = rdr.GetString(5),
                    Destination = rdr.GetString(6),
                    Price = rdr.GetDecimal(7),
                    DepartureTime = rdr.GetDateTime(8),
                    ArrivalTime = rdr.GetDateTime(9),
                    TicketsSold = rdr.GetInt32(10)
                });
            }
            conn.Close();

            // distinct routes
            Routes = AllSchedules
                .Select(s => $"{s.Origin} - {s.Destination}")
                .Distinct()
                .ToList();

            // filter by route
            if (!string.IsNullOrEmpty(SelectedRoute))
            {
                FilteredByRoute = AllSchedules
                    .Where(s => $"{s.Origin} - {s.Destination}" == SelectedRoute)
                    .ToList();
            }

            // filter by bus
            if (SelectedBusId.HasValue)
            {
                FilteredByBus = FilteredByRoute
                    .Where(s => s.BusId == SelectedBusId.Value)
                    .ToList();
            }

            // pick the single schedule if selected
            if (SelectedScheduleId.HasValue)
            {
                SelectedSchedule = FilteredByBus
                    .FirstOrDefault(s => s.ScheduleId == SelectedScheduleId.Value);
            }

            // After filtering schedules:
            Current = AllSchedules.FirstOrDefault(s => s.ScheduleId == SelectedScheduleId);

        }

        public IActionResult OnPost()
        {
            if (!SelectedScheduleId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Invalid schedule.");
                return Page();
            }

            int scheduleId = SelectedScheduleId.Value;

            var clientId = User.FindFirst("UserId")?.Value;
            if (clientId == null) return RedirectToPage("/Login");

            // reload GET state so we can redisplay on error
            OnGet();

            SelectedSchedule = AllSchedules
                .FirstOrDefault(s => s.ScheduleId == scheduleId);

            if (SelectedSchedule == null)
            { Message = "Invalid schedule."; return Page(); }

            if (SelectedSchedule.AvailableSeats <= 0)
            { Message = "Sorry, this bus is full."; return Page(); }

            // INSERT ticket + get new id
            int ticketId;
            var conn = new SqlConnection(_config.GetConnectionString("connstring"));
            conn.Open();
            using (var cmd = new SqlCommand(
                "INSERT INTO Tickets (ClientId, ScheduleId, DateIssued) OUTPUT INSERTED.TicketId VALUES(@c,@s,GETDATE())",
                conn))
            {
                cmd.Parameters.AddWithValue("@c", clientId);
                cmd.Parameters.AddWithValue("@s", scheduleId);
                ticketId = (int)cmd.ExecuteScalar()!;
            }

            // generate PDF
            var ticket = GetTicket(ticketId, clientId);
            var pdf = GenerateTicketPdf(ticket);

            // decide email
            var to = String.IsNullOrWhiteSpace(EmailOverride)
                     ? GetClientEmail(clientId)
                     : EmailOverride!;

            if (!String.IsNullOrWhiteSpace(to))
                SendEmailWithAttachment(to,
                  "Your Bus Ticket",
                  "Please find attached your ticket.",
                  pdf,
                  $"Ticket_{ticketId}.pdf");

            DownloadLink = Url.Page("/Clients/ExportToPdf", new { ticketId });

            Message = "Ticket purchased" +
                      (String.IsNullOrWhiteSpace(to)
                        ? ". Download below."
                        : " and sent to your email!");

            conn.Close();
            return Page();
        }

        // fetch ticket tuple for PDF
        private (int, string, string, string, decimal, DateTime, DateTime, DateTime)
          GetTicket(int id, string clientId)
        {
            using var c = new SqlConnection(_config.GetConnectionString("connstring"));
            c.Open();
            var q = @"
                SELECT t.TicketId,
                       b.BusNumber, 
                       r.Origin, 
                       r.Destination,
                       r.Price,
                       s.DepartureTime,
                       s.ArrivalTime,
                       t.DateIssued
                FROM Tickets t
                JOIN Schedule s ON t.ScheduleId=s.ScheduleId
                JOIN Buses b    ON s.BusId=b.BusId
                JOIN Routes r   ON s.RouteId=r.RouteId
                WHERE t.TicketId=@id AND t.ClientId=@c";
            using var cmd = new SqlCommand(q, c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@c", clientId);
            using var r = cmd.ExecuteReader();
            r.Read();
            return (
               r.GetInt32(0),
               r.GetString(1),
               r.GetString(2),
               r.GetString(3),
               r.GetDecimal(4),
               r.GetDateTime(5),
               r.GetDateTime(6),
               r.GetDateTime(7)
            );
        }

        private byte[] GenerateTicketPdf(
            (int TicketId, string BusName, string RouteName, string RouteDestination,
             decimal Price, DateTime Dep, DateTime Arr, DateTime Issued) t)
        {
            using var ms = new MemoryStream();
            var doc = new Document();
            PdfWriter.GetInstance(doc, ms);
            doc.Open();
            var tf = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            doc.Add(new Paragraph("Bus Ticket", tf) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 12f });

            var table = new PdfPTable(2) { WidthPercentage = 100 };
            void cell(string text, Font f)
              => table.AddCell(new PdfPCell(new Phrase(text, f)) { Border = 0, Padding = 4 });
            var lf = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var nf = FontFactory.GetFont(FontFactory.HELVETICA, 12);

            cell("Ticket ID:", lf); cell(t.TicketId.ToString(), nf);
            cell("Bus:", lf); cell(t.BusName, nf);
            cell("Route:", lf); cell($"{t.RouteName}—{t.RouteDestination}", nf);
            cell("Price:", lf); cell($"{t.Price:F2}", nf);
            cell("Departure Time:", lf); cell($"{t.Dep:yyyy-MM-dd  HH:mm}", nf);
            cell("Arrival Time:", lf); cell($"{t.Arr:yyyy-MM-dd  HH:mm}", nf);
            cell("Issued:", lf); cell(t.Issued.ToString("yyyy-MM-dd HH:mm"), nf);

            doc.Add(table);
            doc.Close();
            return ms.ToArray();
        }

        private string GetClientEmail(string clientId)
        {
            using var c = new SqlConnection(_config.GetConnectionString("connstring"));
            c.Open();
            using var cmd = new SqlCommand("SELECT Email FROM Clients WHERE ClientId=@c", c);
            cmd.Parameters.AddWithValue("@c", clientId);
            return cmd.ExecuteScalar()?.ToString() ?? "";
        }

        private void SendEmailWithAttachment(
            string to, string subj, string body, byte[] attachment, string filename)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("BusManagement", "h1rhodin@gmail.com"));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subj;
            var b = new BodyBuilder { TextBody = body };
            b.Attachments.Add(filename, attachment,
              new ContentType("application", "pdf"));
            msg.Body = b.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect("smtp.gmail.com", 587,
               MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate("h1rhodin@gmail.com", "mebl bwjo kuvy rpbq");
            client.Send(msg);
            client.Disconnect(true);
        }
    }
}
