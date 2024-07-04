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
        Task<List<ClaimStatus>> GetClaimStatuses(string token);

        Task<ClaimStatus> GetClaimStatus(int ID, string token);

        Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus, string token);

        Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus, string token);
    }
}
