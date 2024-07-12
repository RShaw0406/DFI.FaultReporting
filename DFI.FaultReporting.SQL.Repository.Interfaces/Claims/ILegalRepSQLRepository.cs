using DFI.FaultReporting.Models.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Claims
{
    public interface ILegalRepSQLRepository
    {
        Task<List<LegalRep>> GetLegalReps();
        Task<LegalRep> GetLegalRep(int id);
        Task<LegalRep> CreateLegalRep(LegalRep legalRep);
        Task<LegalRep> UpdateLegalRep(LegalRep legalRep);
    }
}
