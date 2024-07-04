using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IFaultTypeSQLRepository
    {
        Task<List<FaultType>> GetFaultTypes();

        Task<FaultType> GetFaultType(int ID);

        Task<FaultType> CreateFaultType(FaultType faultType);

        Task<FaultType> UpdateFaultType(FaultType faultType);
    }
}
