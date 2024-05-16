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

        Task<Fault> GetFault(int ID);

        Task<Fault> CreateFault(Fault fault);

        Task<Fault> UpdateFault(Fault fault);

        Task<Fault> DeleteFault(int ID);
    }
}
