using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Users
{
    public class User
    {
        public int ID { get; set; }

        //[Required(ErrorMessage = "You must enter a username")]
        //[RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,8}$", ErrorMessage = "Username must not contain special characters")]
        //[StringLength(8, ErrorMessage = "Username must not be more than 8 characters")]
        //public string? Username { get; set; }

        [DisplayName("Email Address")]
        [Required(ErrorMessage = "You must enter an email address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public string? Email { get; set; }

        [DisplayName("Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Required(ErrorMessage = "You must enter a password")]
        // Password should contain the following:
        // At least 1 number
        // At least 1 special character
        // At least 1 uppercase letter
        // At least 1 lowercase letter
        // At least 8 characters in total
        // At most 127 characters
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not meet all requirements")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? PasswordSalt { get; set; }

        [DisplayName("Title")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Prefix must not contain special characters or numbers")]
        [StringLength(8, ErrorMessage = "Prefix name must not be more than 8 characters")]
        public string? Prefix { get; set; }

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
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [DisplayName("Address Line 1")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 1 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 1 must not be more than 100 characters")]
        public string? AddressLine1 { get; set; }

        [DisplayName("Address Line 2")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 2 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 2 must not be more than 100 characters")]
        public string? AddressLine2 { get; set; }

        [DisplayName("Address Line 3")]
        [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 3 must not contain special characters")]
        [StringLength(100, ErrorMessage = "Address line 3 must not be more than 100 characters")]
        public string? AddressLine3 { get; set; }

        [Required(ErrorMessage = "You must enter a postcode")]
        [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
        public string? Postcode { get; set; }

        [DisplayName("Contact Number")]
        [RegularExpression(@"^[\d +]{11,15}$", ErrorMessage = "You must enter a valid telephone number")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid contact number")]
        public string? ContactNumber { get; set; }

        [DisplayName("Account Locked")]
        public bool? AccountLocked { get; set; }

        [DisplayName("Account Locked End")]
        [DataType(DataType.Date)]
        public DateTime? AccountLockedEnd { get; set; }

        [DisplayName("Incorrect Attempts")]
        public int? IncorrectAttempts { get; set; }

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
