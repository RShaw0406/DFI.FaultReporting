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
        Task<List<ClaimFile>> GetClaimFiles(string token);

        Task<ClaimFile> GetClaimFile(int ID, string token);

        Task<ClaimFile> CreateClaimFile(ClaimFile claimFile, string token);

        Task<ClaimFile> UpdateClaimFile(ClaimFile claimFile, string token);
    }
}
