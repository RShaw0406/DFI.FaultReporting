using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.JWT.Response
{
    public class AuthResponse
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }

        StatusCodeResult? StatusCodeResult { get; set; }
    }
}
