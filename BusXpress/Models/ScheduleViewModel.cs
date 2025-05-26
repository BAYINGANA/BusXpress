namespace BusManagement.Models
{
    public class ScheduleViewModel
    {
        public int ScheduleId { get; set; }
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public int RouteId { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public int Capacity { get; set; }
        public int TicketsSold { get; set; }
        public int AvailableSeats => Capacity - TicketsSold;
    }
}
