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
        public required int FaultID { get; set; }

        [DisplayName("Repair Target Date")]
        [Required(ErrorMessage = "You must enter a repair target date")]
        [DataType(DataType.Date)]
        public DateTime RepairTargetDate { get; set; }

        [DisplayName("Date Repaired")]
        [Required(ErrorMessage = "You must enter a date of repair")]
        [DataType(DataType.Date)]
        public DateTime ActualRepairDate { get; set; }

        [DisplayName("Notes")]
        [StringLength(1000, ErrorMessage = "Notes must not be more than 1000 characters")]
        public string? RepairNotes { get; set; }

        [DisplayName("Assigned Contractor")]
        [Required(ErrorMessage = "You must provide a contractor")]
        public required int ContractorID { get; set; }

        [DisplayName("Input By")]
        [Required(ErrorMessage = "You must provide an input by")]
        public required string InputBy { get; set; }

        [DisplayName("Input On")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public required DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public required bool Active { get; set; }

        public virtual Fault? Fault { get; set; }

        public virtual Contractor? Contractor { get; set; }
    }
}
