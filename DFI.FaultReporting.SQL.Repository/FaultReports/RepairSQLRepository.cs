using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.FaultReports
{
    public class RepairSQLRepository : IRepairSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public RepairSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Repair>? Repairs { get; set; }

        public async Task<List<Repair>> GetRepairs()
        {
            Repairs = await _context.Repair.ToListAsync();
            return Repairs;
        }

        public async Task<Repair> GetRepair(int ID)
        {
            Repair repair = await _context.Repair.FindAsync(ID);
            return repair;
        }

        public async Task<Repair> CreateRepair(Repair repair)
        {
            _context.Repair.Add(repair);
            await _context.SaveChangesAsync();
            return repair;
        }

        public async Task<Repair> UpdateRepair(Repair repair)
        {
            _context.Entry(repair).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return repair;
        }
    }
}
