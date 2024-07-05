using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Admin
{
    public class FaultPriority
    {
        public int ID { get; set; }

        [DisplayName("Input By")]
        [Required(ErrorMessage = "You must provide an input by")]
        public string? InputBy { get; set; }

        [DisplayName("Input On")]
        [Required(ErrorMessage = "You must provide an input on")]
        [DataType(DataType.Date)]
        public DateTime InputOn { get; set; }

        [Required(ErrorMessage = "You must provide an active")]
        public bool Active { get; set; }

        [DisplayName("Fault priority")]
        [Required(ErrorMessage = "You must enter a fault priority description")]
        [StringLength(250, ErrorMessage = "Fault priority description must not be more than 250 characters")]
        public string? FaultPriorityDescription { get; set; }

        [DisplayName("Rating")]
        [Required(ErrorMessage = "You must enter a fault priority rating")]
        [StringLength(10, ErrorMessage = "Fault priority rating must not be more than 10 characters")]
        public string? FaultPriorityRating { get; set; }
    }
}
