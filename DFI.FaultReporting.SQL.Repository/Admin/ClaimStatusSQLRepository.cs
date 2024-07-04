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
    public class ClaimStatusSQLRepository : IClaimStatusSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ClaimStatusSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        public async Task<List<ClaimStatus>> GetClaimStatuses()
        {
            ClaimStatuses = await _context.ClaimStatus.ToListAsync();
            return ClaimStatuses;
        }

        public async Task<ClaimStatus> GetClaimStatus(int ID)
        {
            ClaimStatus claimStatus = await _context.ClaimStatus.FindAsync(ID);
            return claimStatus;
        }

        public async Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus)
        {
            _context.ClaimStatus.Add(claimStatus);
            await _context.SaveChangesAsync();
            return claimStatus;
        }

        public async Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus)
        {
            _context.Entry(claimStatus).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return claimStatus;
        }
    }
}
