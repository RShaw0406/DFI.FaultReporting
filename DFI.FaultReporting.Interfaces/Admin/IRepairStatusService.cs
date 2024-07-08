using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Admin
{
    public interface IRepairStatusService
    {
        Task<List<RepairStatus>> GetRepairStatuses(string token);

        Task<RepairStatus> GetRepairStatus(int ID, string token);

        Task<RepairStatus> CreateRepairStatus(RepairStatus repairStatus, string token);

        Task<RepairStatus> UpdateRepairStatus(RepairStatus repairStatus, string token);
    }
}
