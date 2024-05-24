using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Admin
{
    public class ClaimTypeSQLRepository : IClaimTypeSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ClaimTypeSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<ClaimType>? ClaimTypes { get; set; }

        public async Task<List<ClaimType>> GetClaimTypes()
        {
            ClaimTypes = await _context.ClaimType.ToListAsync();
            return ClaimTypes;
        }

        public async Task<ClaimType> GetClaimType(int ID)
        {
            ClaimType claimType = await _context.ClaimType.FindAsync(ID);
            return claimType;
        }

        public async Task<ClaimType> CreateClaimType(ClaimType claimType)
        {
            _context.ClaimType.Add(claimType);
            await _context.SaveChangesAsync();
            return claimType;
        }

        public async Task<ClaimType> UpdateClaimType(ClaimType claimType)
        {
            _context.Entry(claimType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return claimType;
        }

        public async Task<int> DeleteClaimType(int ID)
        {
            ClaimType claimType = await _context.ClaimType.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.ClaimType.Remove(claimType);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
