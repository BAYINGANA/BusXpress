using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using BusManagement.Hubs;

namespace BusManagement.Services
{
    public class AdminDashboardPublisher : BackgroundService
    {
        private readonly IHubContext<DashboardHub> _hub;
        private readonly string _conn;
        private Timer? _timer;

        public AdminDashboardPublisher(IHubContext<DashboardHub> hub, IConfiguration cfg)
        {
            _hub = hub;
            _conn = cfg.GetConnectionString("connstring")!;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // fire immediately (0 ms), then every 10 s
            _timer = new Timer(async _ => await Broadcast(),
                               null,
                               TimeSpan.Zero,                   // no initial delay
                               TimeSpan.FromSeconds(10));       // every 10 s instead of 30 s
            return Task.CompletedTask;
        }

        private async Task Broadcast()
        {
            Console.WriteLine($"[Publisher] Broadcasting at {DateTime.Now:HH:mm:ss}");
            await using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            var totalBuses = (int)await new SqlCommand(
              "SELECT COUNT(*) FROM Buses WHERE Status='Available'", conn)
              .ExecuteScalarAsync();
            var totalDrivers = (int)await new SqlCommand(
              "SELECT COUNT(*) FROM Drivers", conn)
              .ExecuteScalarAsync();
            var schedulesToday = (int)await new SqlCommand(
              "SELECT COUNT(*) FROM Schedule WHERE CAST(DepartureTime AS date)=CAST(GETDATE() AS date)", conn)
              .ExecuteScalarAsync();
            var ticketsSold = (int)await new SqlCommand(
              "SELECT COUNT(*) FROM Tickets WHERE CAST(DateIssued AS date)=CAST(GETDATE() AS date)", conn)
              .ExecuteScalarAsync();
            var revenue = Convert.ToDecimal(await new SqlCommand(
              @"SELECT ISNULL(SUM(r.Price),0) 
                FROM Tickets t
                JOIN Schedule s ON t.ScheduleId=s.ScheduleId
                JOIN Routes r ON s.RouteId=r.RouteId
                WHERE CAST(t.DateIssued AS date)=CAST(GETDATE() AS date)", conn)
              .ExecuteScalarAsync());

            var payload = new
            {
                timestamp = DateTime.Now.ToString("HH:mm:ss"),
                totalBuses,
                totalDrivers,
                schedulesToday,
                ticketsSold,
                revenue
            };

            await _hub.Clients.Group("Admins")
                     .SendAsync("ReceiveAdminUpdate", payload);
        }

        public override Task StopAsync(CancellationToken ct)
        {
            _timer?.Dispose();
            return base.StopAsync(ct);
        }
    }
}
