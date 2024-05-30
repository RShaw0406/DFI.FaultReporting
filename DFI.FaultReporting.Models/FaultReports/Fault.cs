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
        [StringLength(250, ErrorMessage = "Road must not be more than 250 characters")]
        public required string RoadName { get; set; }

        [DisplayName("Priority")]
        [Required(ErrorMessage = "You must enter a fault priority")]
        public required int FaultPriorityID { get; set; }

        [DisplayName("Type")]
        [Required(ErrorMessage = "You must enter a fault type")]
        public required int FaultTypeID { get; set; }

        [DisplayName("Status")]
        [Required(ErrorMessage = "You must enter a fault status")]
        public required int FaultStatusID { get; set; }

        [DisplayName("Assigned Staff")]
        public int StaffID { get; set; }

        [DisplayName("Input By")]
        [Required(ErrorMessage = "You must provide an input by")]
        public required string InputBy { get; set; }

        [DisplayName("Input On")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public required DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public required bool Active { get; set; }

        //public virtual FaultPriority? FaultPriority { get; set; }

        //public virtual FaultType? FaultType { get; set; }

        //public virtual FaultStatus? FaultStatus { get; set; }

        //public virtual Staff? Staff { get; set; }
    }
}
