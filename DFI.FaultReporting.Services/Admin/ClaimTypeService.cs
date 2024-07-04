using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Admin
{
    public class ClaimTypeService : IClaimTypeService
    {
        private readonly ClaimTypeHttp _claimTypeHttp;

        public List<ClaimType>? ClaimTypes { get; set; }

        public ClaimTypeService(ClaimTypeHttp claimTypeHttp)
        {
            _claimTypeHttp = claimTypeHttp;
        }

        public async Task<List<ClaimType>> GetClaimTypes(string token)
        {
            ClaimTypes = await _claimTypeHttp.GetClaimTypes(token);

            return ClaimTypes;
        }

        public async Task<ClaimType> GetClaimType(int ID, string token)
        {
            ClaimType claimType = await _claimTypeHttp.GetClaimType(ID, token);

            return claimType;
        }

        public async Task<ClaimType> CreateClaimType(ClaimType claimType, string token)
        {
            claimType = await _claimTypeHttp.CreateClaimType(claimType, token);

            return claimType;
        }

        public async Task<ClaimType> UpdateClaimType(ClaimType claimType, string token)
        {
            claimType = await _claimTypeHttp.UpdateClaimType(claimType, token);

            return claimType;
        }
    }
}
