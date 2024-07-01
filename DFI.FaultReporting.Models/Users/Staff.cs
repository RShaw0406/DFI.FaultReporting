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

        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
        public string? Email { get; set; }

        //At this point password will be hashed.
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        //At this point salt will be in base64.
        public string? PasswordSalt { get; set; }

        [DisplayName("Title")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Prefix must not contain special characters or numbers")]
        [StringLength(8, ErrorMessage = "Prefix name must not be more than 8 characters")]
        public string? Prefix { get; set; }

        [DisplayName("First name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "First name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
        public string? FirstName { get; set; }

        [DisplayName("Last name")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Last name must not contain special characters or numbers")]
        [StringLength(125, ErrorMessage = "Last name must not be more than 125 characters")]
        public string? LastName { get; set; }

        [DisplayName("Account locked")]
        public bool? AccountLocked { get; set; }

        [DisplayName("Account locked end")]
        public DateTime? AccountLockedEnd { get; set; }

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
