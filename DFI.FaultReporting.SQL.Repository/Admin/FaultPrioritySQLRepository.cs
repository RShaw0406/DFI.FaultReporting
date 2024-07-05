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
    public class FaultPrioritySQLRepository : IFaultPrioritySQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public FaultPrioritySQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<FaultPriority>? FaultPriorities { get; set; }

        public async Task<List<FaultPriority>> GetFaultPriorities()
        {
            FaultPriorities = await _context.FaultPriority.ToListAsync();
            return FaultPriorities;
        }

        public async Task<FaultPriority> GetFaultPriority(int ID)
        {
            FaultPriority faultPriority = await _context.FaultPriority.FindAsync(ID);
            return faultPriority;
        }

        public async Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority)
        {
            _context.FaultPriority.Add(faultPriority);
            await _context.SaveChangesAsync();
            return faultPriority;
        }

        public async Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority)
        {
            _context.Entry(faultPriority).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return faultPriority;
        }
    }
}
