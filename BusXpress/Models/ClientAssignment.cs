using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusXpress.Models
{
    public class ClientAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        [ForeignKey("Bus")]
        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }

        [ForeignKey("Route")]
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Reserved";
    }
}