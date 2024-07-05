using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Admin
{
    public interface IContractorSQLRepository
    {
        Task<List<Contractor>> GetContractors();

        Task<Contractor> GetContractor(int ID);

        Task<Contractor> CreateContractor(Contractor claimStatus);

        Task<Contractor> UpdateContractor(Contractor claimStatus);
    }
}
