using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Files
{
    public class ClaimFileSQLRepository : IClaimFileSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ClaimFileSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<ClaimFile>? ClaimFiles { get; set; }

        public async Task<List<ClaimFile>> GetClaimFiles()
        {
            ClaimFiles = await _context.ClaimFile.ToListAsync();
            return ClaimFiles;
        }

        public async Task<ClaimFile> GetClaimFile(int ID)
        {
            ClaimFile claimFile = await _context.ClaimFile.FindAsync(ID);
            return claimFile;
        }

        public async Task<ClaimFile> CreateClaimFile(ClaimFile claimFile)
        {
            _context.ClaimFile.Add(claimFile);
            await _context.SaveChangesAsync();
            return claimFile;
        }

        public async Task<ClaimFile> UpdateClaimFile(ClaimFile claimFile)
        {
            _context.Entry(claimFile).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return claimFile;
        }
    }
}
