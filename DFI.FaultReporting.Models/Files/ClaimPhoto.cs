using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Files
{
    public class ClaimPhoto
    {
        public int ID { get; set; }

        [DisplayName("Claim")]
        [Required(ErrorMessage = "You must enter a claim")]
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "You must enter a description")]
        [StringLength(100, ErrorMessage = "Description must not be more than 100 characters")]
        public string? Description { get; set; }

        [DisplayName("File Type")]
        [Required(ErrorMessage = "You must enter a a file type")]
        [StringLength(10, ErrorMessage = "File type must not be more than 10 characters")]
        public string? Type { get; set; }

        public string? Data { get; set; }

        [DisplayName("Input By")]
        [Required(ErrorMessage = "You must provide an input by")]
        public string? InputBy { get; set; }

        [DisplayName("Input On")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public bool Active { get; set; }
    }
}
