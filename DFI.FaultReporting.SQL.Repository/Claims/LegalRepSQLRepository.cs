using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Claims;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Claims
{
    public class LegalRepSQLRepository : ILegalRepSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public LegalRepSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<LegalRep>? LegalReps { get; set; }

        public async Task<List<LegalRep>> GetLegalReps()
        {
            LegalReps = await _context.LegalRep.ToListAsync();
            return LegalReps;
        }

        public async Task<LegalRep> GetLegalRep(int ID)
        {
            LegalRep legalRep = await _context.LegalRep.FindAsync(ID);
            return legalRep;
        }

        public async Task<LegalRep> CreateLegalRep(LegalRep legalRep)
        {
            _context.LegalRep.Add(legalRep);
            await _context.SaveChangesAsync();
            return legalRep;
        }

        public async Task<LegalRep> UpdateLegalRep(LegalRep legalRep)
        {
            _context.Entry(legalRep).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return legalRep;
        }
    }
}
