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
    public class LegalRepService : ILegalRepService
    {
        private readonly LegalRepHttp _legalRepHttp;

        public List<LegalRep>? LegalReps { get; set; }

        public LegalRepService(LegalRepHttp legalRepHttp)
        {
            _legalRepHttp = legalRepHttp;
        }

        public async Task<List<LegalRep>> GetLegalReps(string token)
        {
            LegalReps = await _legalRepHttp.GetLegalReps(token);

            return LegalReps;
        }

        public async Task<LegalRep> GetLegalRep(int ID, string token)
        {
            LegalRep legalRep = await _legalRepHttp.GetLegalRep(ID, token);

            return legalRep;
        }

        public async Task<LegalRep> CreateLegalRep(LegalRep legalRep, string token)
        {
            legalRep = await _legalRepHttp.CreateLegalRep(legalRep, token);

            return legalRep;
        }

        public async Task<LegalRep> UpdateLegalRep(LegalRep legalRep, string token)
        {
            legalRep = await _legalRepHttp.UpdateLegalRep(legalRep, token);

            return legalRep;
        }
    }
}
