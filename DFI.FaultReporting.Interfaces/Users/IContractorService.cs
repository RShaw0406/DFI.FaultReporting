using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Users
{
    public interface IContractorService
    {
        Task<List<Contractor>> GetContractors();

        Task<Contractor> GetContractor(int ID);

        Task<Contractor> CreateContractor(Contractor contractor);

        Task<Contractor> UpdateContractor(Contractor contractor);

        Task<Contractor> DeleteContractor(int ID);
    }
}