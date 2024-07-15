using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Claims
{
    public class Claim
    {
        public int ID { get; set; }

        [DisplayName("Type")]
        [Required(ErrorMessage = "You must enter a claim type")]
        public int ClaimTypeID { get; set; }

        [DisplayName("Status")]
        [Required(ErrorMessage = "You must enter a claim status")]
        public int ClaimStatusID { get; set; }

        [DisplayName("User")]
        [Required(ErrorMessage = "You must enter a user")]
        public int UserID { get; set; }

        [DisplayName("Staff")]
        public int? StaffID { get; set; }

        [DisplayName("Fault")]
        public int? FaultID { get; set; }

        [DisplayName("Date of incident")]
        [Required(ErrorMessage = "You must provide a date of incident")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [DisplayName("Description of incident")]
        [Required(ErrorMessage = "You must enter a description of the incident")]
        public string? IncidentDescription { get; set; }

        [DisplayName("Description of incident location")]
        [Required(ErrorMessage = "You must enter a description of the incident location")]
        public string? IncidentLocationDescription { get; set; }

        [DisplayName("Lat")]
        public string? IncidentLocationLatitude { get; set; }

        [DisplayName("Long")]
        public string? IncidentLocationLongitude { get; set; }

        [DisplayName("Description of injury")]
        public string? InjuryDescription { get; set; }

        [DisplayName("Description of damage")]
        public string? DamageDescription { get; set; }

        [DisplayName("Description of claim")]
        public string? DamageClaimDescription { get; set; }

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
