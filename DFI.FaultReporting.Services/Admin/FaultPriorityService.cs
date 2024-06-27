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
    public class FaultPriorityService : IFaultPriorityService
    {
        private readonly FaultPriorityHttp _faultPriorityHttp;

        public List<FaultPriority>? FaultPriorities { get; set; }

        public FaultPriorityService(FaultPriorityHttp faultPriorityHttp)
        {
            _faultPriorityHttp = faultPriorityHttp;
        }

        public async Task<List<FaultPriority>> GetFaultPriorities()
        {
            FaultPriorities = await _faultPriorityHttp.GetFaultPriorities();

            return FaultPriorities;
        }

        public async Task<FaultPriority> GetFaultPriority(int ID, string token)
        {
            FaultPriority faultPriority = await _faultPriorityHttp.GetFaultPriority(ID, token);

            return faultPriority;
        }

        public async Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority)
        {
            faultPriority = await _faultPriorityHttp.CreateFaultPriority(faultPriority);

            return faultPriority;
        }

        public async Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority)
        {
            faultPriority = await _faultPriorityHttp.UpdateFaultPriority(faultPriority);

            return faultPriority;
        }

        public async Task<int> DeleteFaultPriority(int ID)
        {
            await _faultPriorityHttp.DeleteFaultPriority(ID);

            return ID;
        }
    }
}
