using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.JWT.Requests
{
    public class RegistrationRequest
    {
        [Required]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public string? Email { get; set; }

        [Required]
        [DisplayName("Password")]
        // Password should contain the following:
        // At least 1 number
        // At least 1 special character
        // At least 1 uppercase letter
        // At least 1 lowercase letter
        // At least 8 characters in total
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not meet all requirements")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [DisplayName("Title")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Title must not contain special characters or numbers")]
        [StringLength(8, ErrorMessage = "Title must not be more than 8 characters")]
        public string? Prefix { get; set; }

        [Required]
        [DisplayName("First name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "First name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
        public string? FirstName { get; set; }

        [Required]
        [DisplayName("Last name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Last name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "Last name must not be more than 125 characters")]
        public string? LastName { get; set; }

        [Required]
        [DisplayName("Day")]
        [Range(1, 31, ErrorMessage = "New date of birth day must be between {1} and {2}")]
        public int? DayDOB { get; set; }

        [Required]
        [DisplayName("Month")]
        [Range(1, 12, ErrorMessage = "New date of birth month must be between {1} and {2}")]
        public int? MonthDOB { get; set; }

        [Required]
        [DisplayName("Year")]
        public int? YearDOB { get; set; }

        [Required]
        [DisplayName("Contact number")]
        [RegularExpression(@"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$", ErrorMessage = "You must enter a valid contact number")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid contact number")]
        public string? ContactNumber { get; set; }

        [Required]
        [DisplayName("Postcode")]
        [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
        public string? Postcode { get; set; }

        [Required]
        [DisplayName("Address line 1")]
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
    }
}
