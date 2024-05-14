using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.FaultReports
{
    public class Fault
    {
        public int ID { get; set; }

        [DisplayName("Lat")]
        [Required(ErrorMessage = "You must enter a latitude")]
        [StringLength(12, ErrorMessage = "Latitude must not be more than 12 characters")]
        public required string Latitude { get; set; }

        [DisplayName("Long")]
        [Required(ErrorMessage = "You must enter a longitude")]
        [StringLength(13, ErrorMessage = "Longitude must not be more than 13 characters")]
        public required string Longitude { get; set; }

        [DisplayName("Road")]
        [Required(ErrorMessage = "You must enter a road")]
        [StringLength(150, ErrorMessage = "Road must not be more than 100 characters")]
        public required string RoadName { get; set; }

        public required virtual FaultType FaultType { get; set; }

        public required virtual FaultStatus FaultStatus { get; set; }

        [DisplayName("Input By")]
        [Required(ErrorMessage = "You must provide an input by")]
        public required string InputBy { get; set; }

        [DisplayName("Input On")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public required DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public required bool Active { get; set; }
    }
}
