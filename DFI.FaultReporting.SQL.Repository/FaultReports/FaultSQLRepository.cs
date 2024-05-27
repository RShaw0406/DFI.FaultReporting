using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.FaultReports
{
    public class FaultSQLRepository : IFaultSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public FaultSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Fault>? Faults { get; set; }

        public async Task<List<Fault>> GetFaults()
        {
            Faults = await _context.Fault.ToListAsync();
            return Faults;
        }

        public async Task<Fault> GetFault(int ID)
        {
            Fault fault = await _context.Fault.FindAsync(ID);
            return fault;
        }

        public async Task<Fault> CreateFault(Fault fault)
        {
            _context.Fault.Add(fault);
            await _context.SaveChangesAsync();
            return fault;
        }

        public async Task<Fault> UpdateFault(Fault fault)
        {
            _context.Entry(fault).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return fault;
        }

        public async Task<int> DeleteFault(int ID)
        {
            Fault fault = await _context.Fault.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.Fault.Remove(fault);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
