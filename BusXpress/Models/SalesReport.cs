using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusXpress.Models
{
    public class SalesReport
    {
        [Key]
        public int ReportId { get; set; }

        public DateTime DateGenerated { get; set; } = DateTime.Now;

        [Required]
        public int TotalTicketsSold { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalRevenue { get; set; }

        [Required]
        public int TotalClients { get; set; }

        [Required]
        public int TotalBuses { get; set; }
    }
}