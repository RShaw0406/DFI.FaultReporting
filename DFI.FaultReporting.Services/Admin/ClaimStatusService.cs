using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Admin
{
    public class ClaimStatusService : IClaimStatusService
    {
        private readonly ClaimStatusHttp _claimStatusHttp;

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        public ClaimStatusService(ClaimStatusHttp claimStatusHttp)
        {
            _claimStatusHttp = claimStatusHttp;
        }

        public async Task<List<ClaimStatus>> GetClaimStatuses()
        {
            ClaimStatuses = await _claimStatusHttp.GetClaimStatuses();

            return ClaimStatuses;
        }

        public async Task<ClaimStatus> GetClaimStatus(int ID)
        {
            ClaimStatus claimStatus = await _claimStatusHttp.GetClaimStatus(ID);

            return claimStatus;
        }

        public async Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus)
        {
            claimStatus = await _claimStatusHttp.CreateClaimStatus(claimStatus);

            return claimStatus;
        }

        public async Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus)
        {
            claimStatus = await _claimStatusHttp.PutClaimStatus(claimStatus);

            return claimStatus;
        }

        public async Task<int> DeleteClaimStatus(int ID)
        {
            await _claimStatusHttp.DeleteClaimStatus(ID);

            return ID;
        }
    }
}
