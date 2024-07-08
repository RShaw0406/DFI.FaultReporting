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
    public class RepairStatusSQLRepository : IRepairStatusSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public RepairStatusSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<RepairStatus>? RepairStatuses { get; set; }

        public async Task<List<RepairStatus>> GetRepairStatuses()
        {
            RepairStatuses = await _context.RepairStatus.ToListAsync();
            return RepairStatuses;
        }

        public async Task<RepairStatus> GetRepairStatus(int ID)
        {
            RepairStatus repairStatus = await _context.RepairStatus.FindAsync(ID);
            return repairStatus;
        }

        public async Task<RepairStatus> CreateRepairStatus(RepairStatus repairStatus)
        {
            _context.RepairStatus.Add(repairStatus);
            await _context.SaveChangesAsync();
            return repairStatus;
        }

        public async Task<RepairStatus> UpdateRepairStatus(RepairStatus repairStatus)
        {
            _context.Entry(repairStatus).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return repairStatus;
        }
    }
}
