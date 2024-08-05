using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.FaultReports
{
    public class FaultService : IFaultService
    {
        private readonly FaultHttp _faultHttp;

        public List<Fault>? Faults { get; set; }

        public FaultService(FaultHttp faultHttp)
        {
            _faultHttp = faultHttp;
        }

        public async Task<List<Fault>> GetFaults()
        {
            Faults = await _faultHttp.GetFaults();

            return Faults;
        }

        public async Task<Fault> GetFault(int ID, string token)
        {
            Fault fault = await _faultHttp.GetFault(ID, token);

            return fault;
        }

        public async Task<Fault> CreateFault(Fault fault, string token)
        {
            fault = await _faultHttp.CreateFault(fault, token);

            return fault;
        }

        public async Task<Fault> UpdateFault(Fault fault, string token)
        {
            fault = await _faultHttp.UpdateFault(fault, token);

            return fault;
        }
    }
}
