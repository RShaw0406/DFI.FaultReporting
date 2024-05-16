using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Users
{
    public class Contractor
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must enter a username")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,8}$", ErrorMessage = "Username must not contain special characters")]
        [StringLength(8, ErrorMessage = "Username must not be more than 8 characters")]
        public required string Username { get; set; }

        [DisplayName("Email Address")]
        [Required(ErrorMessage = "You must enter an email address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "You must enter a password")]
        // Password should contain the following:
        // At least 1 number
        // At least 1 special character
        // At least 1 uppercase letter
        // At least 1 lowercase letter
        // At least 8 characters in total
        // At most 127 characters
        [RegularExpression(@"^(?=.*\d)(?=(.*\W){1})(?=.*[a-z])(?=.*[A-Z])(?!.*\s).{8,127}$", ErrorMessage = "Password does not meet all requirements")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "You must enter a company name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,250}$", ErrorMessage = "Company name must not contain special characters or numbers")]
        [StringLength(250, ErrorMessage = "Company name must not be more than 250 characters")]
        public required string CompanyName { get; set; }

        [DisplayName("Date of Birth")]
        [Required(ErrorMessage = "You must enter a date of birth")]
        [DataType(DataType.Date)]
        public required DateTime DOB { get; set; }

        [DisplayName("Address Line 1")]
        [Required(ErrorMessage = "You must enter an address line 1")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 1 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 1 must not be more than 100 characters")]
        public required string AddressLine1 { get; set; }

        [DisplayName("Address Line 2")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 2 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 2 must not be more than 100 characters")]
        public required string AddressLine2 { get; set; }

        [DisplayName("Address Line 3")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 3 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 3 must not be more than 100 characters")]
        public required string AddressLine3 { get; set; }

        [Required(ErrorMessage = "You must enter a postcode")]
        [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
        public required string Postcode { get; set; }

        [DisplayName("Contact Number")]
        [Required(ErrorMessage = "You must enter a contact number")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid contact number")]
        public required string ContactNumber { get; set; }

        [DisplayName("Account Locked")]
        public bool AccountLocked { get; set; }

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
