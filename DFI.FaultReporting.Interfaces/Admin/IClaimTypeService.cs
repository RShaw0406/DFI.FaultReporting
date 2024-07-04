using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Admin
{
    public interface IClaimTypeService
    {
        Task<List<ClaimType>> GetClaimTypes(string token);

        Task<ClaimType> GetClaimType(int ID, string token);

        Task<ClaimType> CreateClaimType(ClaimType claimType, string token);

        Task<ClaimType> UpdateClaimType(ClaimType claimType, string token);
    }
}
