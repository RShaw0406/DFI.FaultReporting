using DFI.FaultReporting.Models.Users;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Tokens
{
    public interface IJWTTokenService
    {
        Task<SecurityToken> GenerateToken(User? user, Staff? staff);
    }
}
