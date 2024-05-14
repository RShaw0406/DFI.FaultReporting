using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Admin
{
    public class FaultStatus
    {
        public int ID { get; set; }

        [DisplayName("Fault Status")]
        [Required(ErrorMessage = "You must enter a fault status description")]
        [StringLength(50, ErrorMessage = "Fault status description must not be more than 50 characters")]
        public required string FaultStatusDescription { get; set; }

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
