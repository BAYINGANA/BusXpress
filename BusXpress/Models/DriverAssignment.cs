using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusXpress.Models
{
    public class DriverAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [ForeignKey("Driver")]
        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        [ForeignKey("Bus")]
        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }

        public DateTime AssignmentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Assigned";
    }
}