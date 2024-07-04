using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IFaultStatusSQLRepository
    {
        Task<List<FaultStatus>> GetFaultStatuses();

        Task<FaultStatus> GetFaultStatus(int ID);

        Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus);

        Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus);
    }
}
