using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Claims
{
    public interface IWitnessService
    {
        Task<List<Witness>> GetWitnesses(string token);
        Task<Witness> GetWitness(int id, string token);
        Task<Witness> CreateWitness(Witness witness, string token);
        Task<Witness> UpdateWitness(Witness witness, string token);
    }
}
