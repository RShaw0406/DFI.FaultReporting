using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Admin
{
    public class RepairStatusService : IRepairStatusService
    {
        private readonly RepairStatusHttp _repairStatusHttp;

        public List<RepairStatus>? RepairStatuses { get; set; }

        public RepairStatusService(RepairStatusHttp repairStatusHttp)
        {
            _repairStatusHttp = repairStatusHttp;
        }

        public async Task<List<RepairStatus>> GetRepairStatuses(string token)
        {
            RepairStatuses = await _repairStatusHttp.GetRepairStatuses(token);

            return RepairStatuses;
        }

        public async Task<RepairStatus> GetRepairStatus(int ID, string token)
        {
            RepairStatus repairStatus = await _repairStatusHttp.GetRepairStatus(ID, token);

            return repairStatus;
        }

        public async Task<RepairStatus> CreateRepairStatus(RepairStatus repairStatus, string token)
        {
            repairStatus = await _repairStatusHttp.CreateRepairStatus(repairStatus, token);

            return repairStatus;
        }

        public async Task<RepairStatus> UpdateRepairStatus(RepairStatus repairStatus, string token)
        {
            repairStatus = await _repairStatusHttp.UpdateRepairStatus(repairStatus, token);

            return repairStatus;
        }
    }
}
