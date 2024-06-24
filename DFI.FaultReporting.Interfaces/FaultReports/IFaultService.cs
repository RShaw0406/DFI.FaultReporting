using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.FaultReports
{
    public interface IFaultService
    {
        Task<List<Fault>> GetFaults();

        Task<Fault> GetFault(int ID, string token);

        Task<Fault> CreateFault(Fault fault, string token);

        Task<Fault> UpdateFault(Fault fault, string token);

        Task<int> DeleteFault(int ID, string token);
    }
}
