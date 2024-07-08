using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports
{
    public interface IRepairSQLRepository
    {
        Task<List<Repair>> GetRepairs();

        Task<Repair> GetRepair(int ID);

        Task<Repair> CreateRepair(Repair repair);

        Task<Repair> UpdateRepair(Repair repair);
    }
}
