using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Files
{
    public interface IClaimFileService
    {
        Task<List<ClaimFile>> GetClaimFiles();

        Task<ClaimFile> GetClaimFile(int ID);

        Task<ClaimFile> CreateClaimFile(ClaimFile claimFile);

        Task<ClaimFile> UpdateClaimFile(ClaimFile claimFile);

        Task<ClaimFile> DeleteClaimFile(int ID);
    }
}
