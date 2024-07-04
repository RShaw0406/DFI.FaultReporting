using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Admin
{
    public class FaultTypeSQLRepository : IFaultTypeSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public FaultTypeSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<FaultType>? FaultTypes { get; set; }

        public async Task<List<FaultType>> GetFaultTypes()
        {
            FaultTypes = await _context.FaultType.ToListAsync();
            return FaultTypes;
        }

        public async Task<FaultType> GetFaultType(int ID)
        {
            FaultType faultType = await _context.FaultType.FindAsync(ID);
            return faultType;
        }

        public async Task<FaultType> CreateFaultType(FaultType faultType)
        {
            _context.FaultType.Add(faultType);
            await _context.SaveChangesAsync();
            return faultType;
        }

        public async Task<FaultType> UpdateFaultType(FaultType faultType)
        {
            _context.Entry(faultType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return faultType;
        }
    }
}
