using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Roles
{
    public class StaffRoleSQLRepository : IStaffRoleSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public StaffRoleSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<StaffRole>? StaffRoles { get; set; }

        public async Task<List<StaffRole>> GetStaffRoles()
        {
            StaffRoles = await _context.StaffRole.ToListAsync();
            return StaffRoles;
        }

        public async Task<StaffRole> GetStaffRole(int ID)
        {
            StaffRole staffRole = await _context.StaffRole.FindAsync(ID);
            return staffRole;
        }

        public async Task<StaffRole> CreateStaffRole(StaffRole staffRole)
        {
            _context.StaffRole.Add(staffRole);
            await _context.SaveChangesAsync();
            return staffRole;
        }

        public async Task<StaffRole> UpdateStaffRole(StaffRole staffRole)
        {
            _context.Entry(staffRole).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return staffRole;
        }

        public async Task<int> DeleteStaffRole(int ID)
        {
            StaffRole staffRole = await _context.StaffRole.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.StaffRole.Remove(staffRole);
            await _context.SaveChangesAsync();
            return ID;
        }
     
    }
}
