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
    public class ClaimSQLRepository : IClaimSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ClaimSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Claim>? Claims { get; set; }

        public async Task<List<Claim>> GetClaims()
        {
            Claims = await _context.Claim.ToListAsync();
            return Claims;
        }

        public async Task<Claim> GetClaim(int ID)
        {
            Claim claim = await _context.Claim.FindAsync(ID);
            return claim;
        }

        public async Task<Claim> CreateClaim(Claim claim)
        {
            _context.Claim.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<Claim> UpdateClaim(Claim claim)
        {
            _context.Entry(claim).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return claim;
        }
    }
}
