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

        [DisplayName("Fault type")]
        [Required(ErrorMessage = "You must enter a fault type description")]
        [StringLength(50, ErrorMessage = "Fault type description must not be more than 50 characters")]
        public string? FaultTypeDescription { get; set; }

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
