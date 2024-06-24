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
        public string? Latitude { get; set; }

        [DisplayName("Long")]
        [Required(ErrorMessage = "You must enter a longitude")]
        public string? Longitude { get; set; }

        [DisplayName("Road")]
        [Required(ErrorMessage = "You must enter a road")]
        [StringLength(250, ErrorMessage = "Road must not be more than 250 characters")]
        public string? RoadName { get; set; }

        [DisplayName("Priority")]
        [Required(ErrorMessage = "You must enter a fault priority")]
        public int FaultPriorityID { get; set; }

        [DisplayName("Type")]
        [Required(ErrorMessage = "You must enter a fault type")]
        public int FaultTypeID { get; set; }

        [DisplayName("Status")]
        [Required(ErrorMessage = "You must enter a fault status")]
        public int FaultStatusID { get; set; }

        [DisplayName("Assigned staff")]
        public int StaffID { get; set; }

        [DisplayName("Input by")]
        [Required(ErrorMessage = "You must provide an input by")]
        public string? InputBy { get; set; }

        [DisplayName("Input on")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public bool Active { get; set; }

        [DisplayName("Road number")]
        [StringLength(10, ErrorMessage = "Road number must not be more than 10 characters")]
        public string? RoadNumber { get; set; }

        [DisplayName("Road town")]
        [StringLength(100, ErrorMessage = "Road town must not be more than 100 characters")]
        public string? RoadTown { get; set; }

        [DisplayName("Road county")]
        [StringLength(50, ErrorMessage = "Road county must not be more than 50 characters")]
        public string? RoadCounty { get; set; }
    }
}
