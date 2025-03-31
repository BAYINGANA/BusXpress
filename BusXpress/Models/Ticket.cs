using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusXpress.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        [ForeignKey("Bus")]
        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }

        [ForeignKey("Route")]
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public DateTime DateIssued { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }
}