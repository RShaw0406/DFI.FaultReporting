using DFI.FaultReporting.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Passwords
{
    public interface IPasswordService
    {
        Task<bool> VerifyHashedPassword(string hashedPassword, string providedPassword);

        Task<string> HashPassword(string password);
    }
}
