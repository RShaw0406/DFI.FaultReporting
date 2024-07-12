using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Claims
{
    public interface ILegalRepService
    {
        Task<List<LegalRep>> GetLegalReps(string token);
        Task<LegalRep> GetLegalRep(int id, string token);
        Task<LegalRep> CreateLegalRep(LegalRep legalRep, string token);
        Task<LegalRep> UpdateLegalRep(LegalRep legalRep, string token);
    }
}
