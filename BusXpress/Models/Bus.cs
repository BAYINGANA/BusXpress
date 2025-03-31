using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusXpress.Models
{
    public class Bus
    {
        [Key]
        public int BusId { get; set; }

        [Required]
        [StringLength(50)]
        public string BusNumber { get; set; }

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Available";
    }
}