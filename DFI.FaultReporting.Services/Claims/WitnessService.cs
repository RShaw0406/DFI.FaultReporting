using DFI.FaultReporting.Http.Claims;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Services.Interfaces.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Claims
{
    public class WitnessService : IWitnessService
    {
        private readonly WitnessHttp _witnessHttp;

        public List<Witness>? Witnesses { get; set; }

        public WitnessService(WitnessHttp witnessHttp)
        {
            _witnessHttp = witnessHttp;
        }

        public async Task<List<Witness>> GetWitnesses(string token)
        {
            Witnesses = await _witnessHttp.GetWitnesses(token);

            return Witnesses;
        }

        public async Task<Witness> GetWitness(int ID, string token)
        {
            Witness witness = await _witnessHttp.GetWitness(ID, token);

            return witness;
        }

        public async Task<Witness> CreateWitness(Witness witness, string token)
        {
            witness = await _witnessHttp.CreateWitness(witness, token);

            return witness;
        }

        public async Task<Witness> UpdateWitness(Witness witness, string token)
        {
            witness = await _witnessHttp.UpdateWitness(witness, token);

            return witness;
        }
    }
}
