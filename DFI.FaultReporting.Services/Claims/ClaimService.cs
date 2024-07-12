using DFI.FaultReporting.Http.Claims;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Claims
{
    public class ClaimService : IClaimService
    {
        private readonly ClaimHttp _claimHttp;

        public List<Claim>? Claims { get; set; }

        public ClaimService(ClaimHttp claimHttp)
        {
            _claimHttp = claimHttp;
        }

        public async Task<List<Claim>> GetClaims(string token)
        {
            Claims = await _claimHttp.GetClaims(token);

            return Claims;
        }

        public async Task<Claim> GetClaim(int ID, string token)
        {
            Claim claim = await _claimHttp.GetClaim(ID, token);

            return claim;
        }

        public async Task<Claim> CreateClaim(Claim claim, string token)
        {
            claim = await _claimHttp.CreateClaim(claim, token);

            return claim;
        }

        public async Task<Claim> UpdateClaim(Claim claim, string token)
        {
            claim = await _claimHttp.UpdateClaim(claim, token);

            return claim;
        }
    }
}
