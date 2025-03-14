﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Models.Roles
{
    public class UserRole
    {
        public int ID { get; set; }

        [DisplayName("Role")]
        [Required(ErrorMessage = "You must enter a role")]
        public int RoleID { get; set; }

        [DisplayName("User")]
        [Required(ErrorMessage = "You must enter a user")]
        public int UserID { get; set; }

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
