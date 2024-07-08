using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using DFI.FaultReporting.Models.Users;

namespace DFI.FaultReporting.Models.FaultReports
{
    public class Repair
    {
        public int ID { get; set; }

        [DisplayName("Fault")]
        [Required(ErrorMessage = "You must enter a fault")]
        public int FaultID { get; set; }

        [DisplayName("Repair target date")]
        [Required(ErrorMessage = "You must enter a repair target date")]
        [DataType(DataType.Date)]
        public DateTime RepairTargetDate { get; set; }

        [DisplayName("Date repaired")]
        [DataType(DataType.Date)]
        public DateTime? ActualRepairDate { get; set; }

        [DisplayName("Notes")]
        [StringLength(1000, ErrorMessage = "Notes must not be more than 1000 characters")]
        public string? RepairNotes { get; set; }

        [DisplayName("Status")]
        [Required(ErrorMessage = "You must provide a status")]
        public int RepairStatusID { get; set; }

        [DisplayName("Assigned contractor")]
        [Required(ErrorMessage = "You must provide a contractor")]
        public int ContractorID { get; set; }

        [DisplayName("Input by")]
        [Required(ErrorMessage = "You must provide an input by")]
        public string? InputBy { get; set; }

        [DisplayName("Input on")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public bool Active { get; set; }
    }
}
