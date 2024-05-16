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
        Task<List<ClaimType>> GetClaimTypes();

        Task<ClaimType> GetClaimType(int ID);

        Task<ClaimType> CreateClaimType(ClaimType claimType);

        Task<ClaimType> UpdateClaimType(ClaimType claimType);

        Task<ClaimType> DeleteClaimType(int ID);
    }
}
