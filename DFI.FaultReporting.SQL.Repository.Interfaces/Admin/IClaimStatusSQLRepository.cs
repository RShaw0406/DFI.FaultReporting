using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IClaimStatusSQLRepository
    {
        Task<List<ClaimStatus>> GetClaimStatuses();

        Task<ClaimStatus> GetClaimStatus(int ID);

        Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus);

        Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus);

        Task<int> DeleteClaimStatus(int ID);
    }
}
