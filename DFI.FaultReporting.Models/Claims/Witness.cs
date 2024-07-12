using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Claims
{
    public class Witness
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

        [DisplayName("Date of birth")]
        [Required(ErrorMessage = "You must enter a date of birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public string? Email { get; set; }

        [DisplayName("Contact number")]
        [RegularExpression(@"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$", ErrorMessage = "You must enter a valid telephone number")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid contact number")]
        public string? ContactNumber { get; set; }

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
