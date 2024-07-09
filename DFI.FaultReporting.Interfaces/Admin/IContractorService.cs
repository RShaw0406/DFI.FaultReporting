using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Admin
{
    public interface IContractorService
    {
        Task<List<Contractor>> GetContractors(string token);

        Task<Contractor> GetContractor(int ID, string token);

        Task<bool> CheckForContractor(string email);

        Task<Contractor> CreateContractor(Contractor claimStatus, string token);

        Task<Contractor> UpdateContractor(Contractor claimStatus, string token);
    }
}
