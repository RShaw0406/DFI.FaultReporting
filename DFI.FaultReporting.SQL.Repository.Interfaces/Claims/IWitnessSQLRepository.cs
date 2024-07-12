using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Claims
{
    public interface IWitnessSQLRepository
    {
        Task<List<Witness>> GetWitnesses();
        Task<Witness> GetWitness(int id);
        Task<Witness> CreateWitness(Witness witness);
        Task<Witness> UpdateWitness(Witness witness);
    }
}
