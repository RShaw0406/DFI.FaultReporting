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
    public class FaultStatusService : IFaultStatusService
    {
        private readonly FaultStatusHttp _faultStatusHttp;

        public List<FaultStatus>? FaultStatuses { get; set; }

        public FaultStatusService(FaultStatusHttp faultStatusHttp)
        {
            _faultStatusHttp = faultStatusHttp;
        }

        public async Task<List<FaultStatus>> GetFaultStatuses()
        {
            FaultStatuses = await _faultStatusHttp.GetFaultStatuses();

            return FaultStatuses;
        }

        public async Task<FaultStatus> GetFaultStatus(int ID, string token)
        {
            FaultStatus faultStatus = await _faultStatusHttp.GetFaultStatus(ID, token);

            return faultStatus;
        }

        public async Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus, string token)
        {
            faultStatus = await _faultStatusHttp.CreateFaultStatus(faultStatus, token);

            return faultStatus;
        }

        public async Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus, string token)
        {
            faultStatus = await _faultStatusHttp.UpdateFaultStatus(faultStatus, token);

            return faultStatus;
        }
    }
}
