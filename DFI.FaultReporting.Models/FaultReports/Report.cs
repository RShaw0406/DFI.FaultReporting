using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFI.FaultReporting.Models.Users;

namespace DFI.FaultReporting.Models.FaultReports
{
    public class Report
    {
        public int ID { get; set; }

        [DisplayName("Fault")]
        [Required(ErrorMessage = "You must enter a fault")]
        public int FaultID { get; set; }

        [DisplayName("Additional information")]
        [Required(ErrorMessage = "You must enter additional info")]
        [StringLength(1000, ErrorMessage = "Additional info must not be more than 1000 characters")]
        public string? AdditionalInfo { get; set; }

        [DisplayName("Reporting user")]
        [Required(ErrorMessage = "You must enter a user")]
        public int UserID { get; set; }

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
