using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Users
{
    public class Staff
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

        [Required(ErrorMessage = "You must enter a prefix")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Prefix must not contain special characters or numbers")]
        [StringLength(8, ErrorMessage = "Prefix name must not be more than 8 characters")]
        public required string Prefix { get; set; }

        [Required(ErrorMessage = "You must enter a first name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,50}$", ErrorMessage = "First name must not contain special characters or numbers")]
        [StringLength(50, ErrorMessage = "First name must not be more than 50 characters")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "You must enter a last name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,50}$", ErrorMessage = "Last name must not contain special characters or numbers")]
        [StringLength(50, ErrorMessage = "Last name must not be more than 50 characters")]
        public required string LastName { get; set; }

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
