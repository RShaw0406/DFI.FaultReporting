using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Admin
{
    public class ClaimStatus
    {
        public int ID { get; set; }

        [DisplayName("Claim Status")]
        [Required(ErrorMessage = "You must enter a claim status description")]
        [StringLength(50, ErrorMessage = "Claim status description must not be more than 50 characters")]
        public required string ClaimStatusDescription { get; set; }

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
