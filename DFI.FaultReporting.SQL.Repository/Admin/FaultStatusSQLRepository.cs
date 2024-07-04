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
    public class FaultStatusSQLRepository : IFaultStatusSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public FaultStatusSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<FaultStatus>? FaultStatuses { get; set; }

        public async Task<List<FaultStatus>> GetFaultStatuses()
        {
            FaultStatuses = await _context.FaultStatus.ToListAsync();
            return FaultStatuses;
        }

        public async Task<FaultStatus> GetFaultStatus(int ID)
        {
            FaultStatus faultStatus = await _context.FaultStatus.FindAsync(ID);
            return faultStatus;
        }

        public async Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus)
        {
            _context.FaultStatus.Add(faultStatus);
            await _context.SaveChangesAsync();
            return faultStatus;
        }

        public async Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus)
        {
            _context.Entry(faultStatus).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return faultStatus;
        }
    }
}
