using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.FaultReports
{
    public interface IRepairService
    {
        Task<List<Repair>> GetRepairs(string token);

        Task<Repair> GetRepair(int ID, string token);

        Task<Repair> CreateRepair(Repair repair, string token);

        Task<Repair> UpdateRepair(Repair repair, string token);
    }
}
