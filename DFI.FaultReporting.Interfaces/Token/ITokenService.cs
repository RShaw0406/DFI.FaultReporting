using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Token
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}
