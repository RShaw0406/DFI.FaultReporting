using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Admin
{
    public interface IClaimStatusService
    {
        Task<List<ClaimStatus>> GetClaimStatuses();

        Task<ClaimStatus> GetClaimStatus(int ID);

        Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus);

        Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus);

        Task<ClaimStatus> DeleteClaimStatus(int ID);
    }
}
