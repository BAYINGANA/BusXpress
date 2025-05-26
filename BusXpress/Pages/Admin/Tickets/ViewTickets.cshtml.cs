using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.PdfDocument;

namespace BusManagement.Pages.Admin.Tickets
{
    public class ViewTicketsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ViewTicketsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public int? ClientId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? ScheduleId { get; set; }
        public List<TicketViewModel> Tickets { get; set; } = new();
        public List<SelectListItem> Clients { get; set; } = new();
        public List<SelectListItem> Schedules { get; set; } = new();

        public void OnGet()
        {
            LoadDropdowns();
            LoadTickets();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int ticketId)
        {
            string connStr = _configuration.GetConnectionString("connstring");
            string clientEmail = null;
            int clientId;
            string origin;
            string destination;

            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                // retrieve client email before deleting
                using (var cmd = new SqlCommand(
                    "SELECT c.Email, t.ClientId, s.ScheduleId, r.Origin, r.Destination FROM Tickets t JOIN Clients c ON t.ClientId=c.ClientId JOIN Schedule s ON t.ScheduleId = s.ScheduleId JOIN Routes r ON s.RouteId = r.RouteId WHERE t.TicketId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", ticketId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                            return NotFound();
                        clientEmail = reader.GetString(0);
                        clientId = reader.GetInt32(1);
                        ScheduleId = reader.GetInt32(2);
                        origin = reader.GetString(3);
                        destination = reader.GetString(4);

                    }
                }

                // delete the ticket
                using (var cmd = new SqlCommand(
                    "DELETE FROM Tickets WHERE TicketId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", ticketId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // send cancellation email (fire-and-forget)
            if (!string.IsNullOrEmpty(clientEmail))
                _ = SendCancellationEmailAsync(clientEmail, ticketId, origin, destination);

            // stay on the filtered page
            return RedirectToPage(new { ClientId, ScheduleId });
        }

        private void LoadDropdowns()
        {
            string connStr = _configuration.GetConnectionString("connstring");
            using var conn = new SqlConnection(connStr);
            conn.Open();

            // Clients
            using (var cmd = new SqlCommand("SELECT ClientId, Name FROM Clients", conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    Clients.Add(new SelectListItem
                    {
                        Value = r.GetInt32(0).ToString(),
                        Text = r.GetString(1)
                    });

            // Schedules
            using (var cmd = new SqlCommand(@"
                SELECT s.ScheduleId, b.BusNumber, r.Origin, r.Destination, s.DepartureTime 
                FROM Schedule s
                  JOIN Buses b ON s.BusId=b.BusId
                  JOIN Routes r ON s.RouteId=r.RouteId", conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    Schedules.Add(new SelectListItem
                    {
                        Value = r.GetInt32(0).ToString(),
                        Text = $"{r.GetString(1)} | {r.GetString(2)}–{r.GetString(3)} @ {r.GetDateTime(4):HH:mm}"
                    });
        }

        private void LoadTickets()
        {
            string connStr = _configuration.GetConnectionString("connstring");
            using var conn = new SqlConnection(connStr);
            conn.Open();

            var sb = new StringBuilder(@"
                SELECT t.TicketId, c.Name, b.BusNumber, r.Origin, r.Destination, r.Price,
                       s.DepartureTime, s.ArrivalTime, t.DateIssued
                FROM Tickets t
                  JOIN Clients c ON t.ClientId=c.ClientId
                  JOIN Schedule s ON t.ScheduleId=s.ScheduleId
                  JOIN Buses b ON s.BusId=b.BusId
                  JOIN Routes r ON s.RouteId=r.RouteId
                WHERE 1=1");

            if (ClientId.HasValue) sb.Append(" AND t.ClientId=@cid");
            if (ScheduleId.HasValue) sb.Append(" AND t.ScheduleId=@sid");

            using var cmd = new SqlCommand(sb.ToString(), conn);
            if (ClientId.HasValue) cmd.Parameters.AddWithValue("@cid", ClientId.Value);
            if (ScheduleId.HasValue) cmd.Parameters.AddWithValue("@sid", ScheduleId.Value);

            using var r2 = cmd.ExecuteReader();
            while (r2.Read())
            {
                Tickets.Add(new TicketViewModel
                {
                    TicketId = r2.GetInt32(0),
                    ClientName = r2.GetString(1),
                    BusNumber = r2.GetString(2),
                    Origin = r2.GetString(3),
                    Destination = r2.GetString(4),
                    Price = r2.GetDecimal(5),
                    DepartureTime = r2.GetDateTime(6),
                    ArrivalTime = r2.GetDateTime(7),
                    DateIssued = r2.GetDateTime(8)
                });
            }
        }

        private async Task SendCancellationEmailAsync(string toEmail, int ticketId, string origin, string destination)
        {
            var msg = new MimeMessage();
            msg.From.Add( new MailboxAddress("BusManagement", "h1rhodin@gmail.com"));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = "Your Bus Ticket Has Been Canceled";
            msg.Body = new TextPart("plain")
            {
                Text = $"We regret to inform you that your ticket number {ticketId} from {origin} to {destination} has been canceled by the administrator."
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            // TODO: use your SMTP server credentials
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("h1rhodin@gmail.com", "mebl bwjo kuvy rpbq");
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }

        public class TicketViewModel
        {
            public int TicketId { get; set; }
            public string ClientName { get; set; } = "";
            public string BusNumber { get; set; } = "";
            public string Origin { get; set; } = "";
            public string Destination { get; set; } = "";
            public decimal Price { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public DateTime DateIssued { get; set; }
        }
    }
}
