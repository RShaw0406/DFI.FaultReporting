using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Admin
{
    public class Contractor
    {
        public int ID { get; set; }

        [DisplayName("Email address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public string? Email { get; set; }

        [DisplayName("Contractor name")]
        [Required]
        [StringLength(250, ErrorMessage = "Contractor name must not be more than 250 characters")]
        public string? ContractorName { get; set; }

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
