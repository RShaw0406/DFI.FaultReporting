using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IFaultPrioritySQLRepository
    {
        Task<List<FaultPriority>> GetFaultPriorities();

        Task<FaultPriority> GetFaultPriority(int ID);

        Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority);

        Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority);

        Task<int> DeleteFaultPriority(int ID);
    }
}
