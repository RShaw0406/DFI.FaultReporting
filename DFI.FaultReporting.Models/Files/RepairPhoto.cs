using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Files
{
    public class RepairPhoto
    {
        public int ID { get; set; }

        [DisplayName("Repair")]
        [Required(ErrorMessage = "You must enter a repair")]
        public int RepairID { get; set; }

        [Required(ErrorMessage = "You must enter a description")]
        [StringLength(200, ErrorMessage = "Description must not be more than 200 characters")]
        public string? Description { get; set; }

        [DisplayName("File type")]
        [Required(ErrorMessage = "You must enter a a file type")]
        [StringLength(10, ErrorMessage = "File type must not be more than 10 characters")]
        public string? Type { get; set; }

        public string? Data { get; set; }

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
