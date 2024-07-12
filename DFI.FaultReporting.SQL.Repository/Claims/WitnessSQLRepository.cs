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
    public class WitnessSQLRepository : IWitnessSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public WitnessSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Witness>? Witnesses { get; set; }

        public async Task<List<Witness>> GetWitnesses()
        {
            Witnesses = await _context.Witness.ToListAsync();
            return Witnesses;
        }

        public async Task<Witness> GetWitness(int ID)
        {
            Witness witness = await _context.Witness.FindAsync(ID);
            return witness;
        }

        public async Task<Witness> CreateWitness(Witness witness)
        {
            _context.Witness.Add(witness);
            await _context.SaveChangesAsync();
            return witness;
        }

        public async Task<Witness> UpdateWitness(Witness witness)
        {
            _context.Entry(witness).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return witness;
        }
    }
}
