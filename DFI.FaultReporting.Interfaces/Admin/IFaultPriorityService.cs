using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Admin
{
    public interface IFaultPriorityService
    {
        Task<List<FaultPriority>> GetFaultPriorities();

        Task<FaultPriority> GetFaultPriority(int ID);

        Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority);

        Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority);

        Task<int> DeleteFaultPriority(int ID);
    }
}
