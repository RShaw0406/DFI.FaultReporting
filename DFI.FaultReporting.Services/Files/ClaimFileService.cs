using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class ClaimFileService : IClaimFileService
    {
        private readonly ClaimFileHttp _claimFileHttp;

        public List<ClaimFile>? ClaimFiles { get; set; }

        public ClaimFileService(ClaimFileHttp claimFileHttp)
        {
            _claimFileHttp = claimFileHttp;
        }

        public async Task<List<ClaimFile>> GetClaimFiles(string token)
        {
            ClaimFiles = await _claimFileHttp.GetClaimFiles(token);

            return ClaimFiles;
        }

        public async Task<ClaimFile> GetClaimFile(int ID, string token)
        {
            ClaimFile claimFile = await _claimFileHttp.GetClaimFile(ID, token);

            return claimFile;
        }

        public async Task<ClaimFile> CreateClaimFile(ClaimFile claimFile, string token)
        {
            claimFile = await _claimFileHttp.CreateClaimFile(claimFile, token);

            return claimFile;
        }

        public async Task<ClaimFile> UpdateClaimFile(ClaimFile claimFile, string token)
        {
            claimFile = await _claimFileHttp.UpdateClaimFile(claimFile, token);

            return claimFile;
        }
    }
}
