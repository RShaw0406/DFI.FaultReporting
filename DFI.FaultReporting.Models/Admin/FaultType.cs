using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Admin
{
    public class FaultType
    {
        public int ID { get; set; }

        [DisplayName("Fault Type")]
        [Required(ErrorMessage = "You must enter a fault type description")]
        [StringLength(50, ErrorMessage = "Fault type description must not be more than 50 characters")]
        public required string FaultTypeDescription { get; set; }

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
