using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Tokens
{
    public interface IVerificationTokenService
    {
        Task<int> GenerateToken();
    }
}
