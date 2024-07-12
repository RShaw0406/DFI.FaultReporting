using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Claims
{
    public class LegalRep
    {
        public int ID { get; set; }

        [DisplayName("Claim")]
        [Required(ErrorMessage = "You must enter a claim")]
        public int ClaimID { get; set; }

        [DisplayName("Title")]
        [Required(ErrorMessage = "You must enter a title")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Title must not contain special characters or numbers")]
        [StringLength(8, ErrorMessage = "Title must not be more than 8 characters")]
        public string? Title { get; set; }

        [DisplayName("First name")]
        [Required(ErrorMessage = "You must enter a first name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "First name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
        public string? FirstName { get; set; }

        [DisplayName("Last name")]
        [Required(ErrorMessage = "You must enter a last name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Last name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "Last name must not be more than 125 characters")]
        public string? LastName { get; set; }

        [DisplayName("Company name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Company name must not contain special characters or numbers")]
        [StringLength(250, ErrorMessage = "Company name must not be more than 250 characters")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "You must provide a postcode")]
        [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
        public string? Postcode { get; set; }

        [DisplayName("Address line 1")]
        [Required(ErrorMessage = "You must provide an address line 1")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 1 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 1 must not be more than 100 characters")]
        public string? AddressLine1 { get; set; }

        [DisplayName("Address line 2")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 2 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 2 must not be more than 100 characters")]
        public string? AddressLine2 { get; set; }

        [DisplayName("Address line 3")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 3 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 3 must not be more than 100 characters")]
        public string? AddressLine3 { get; set; }

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
