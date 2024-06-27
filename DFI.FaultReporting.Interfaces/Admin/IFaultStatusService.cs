using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Admin
{
    public interface IFaultStatusService
    {
        Task<List<FaultStatus>> GetFaultStatuses();

        Task<FaultStatus> GetFaultStatus(int ID, string token);

        Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus);

        Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus);

        Task<int> DeleteFaultStatus(int ID);
    }
}
