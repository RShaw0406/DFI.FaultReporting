using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Claims
{
    public interface IClaimService
    {
        Task<List<Claim>> GetClaims(string token);

        Task<Claim> GetClaim(int ID, string token);

        Task<Claim> CreateClaim(Claim claim, string token);

        Task<Claim> UpdateClaim(Claim claim, string token);
    }
}
