using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Claims
{
    public interface IClaimSQLRepository
    {
        Task<List<Claim>> GetClaims();

        Task<Claim> GetClaim(int ID);

        Task<Claim> CreateClaim(Claim claim);

        Task<Claim> UpdateClaim(Claim claim);
    }
}
