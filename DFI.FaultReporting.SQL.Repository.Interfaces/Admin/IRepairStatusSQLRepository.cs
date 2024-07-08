using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IRepairStatusSQLRepository
    {
        Task<List<RepairStatus>> GetRepairStatuses();

        Task<RepairStatus> GetRepairStatus(int ID);

        Task<RepairStatus> CreateRepairStatus(RepairStatus repairStatus);

        Task<RepairStatus> UpdateRepairStatus(RepairStatus repairStatus);
    }
}
