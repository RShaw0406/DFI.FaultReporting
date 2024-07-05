using DFI.FaultReporting.Models.Admin;
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
    public class RoleSQLRepository : IRoleSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public RoleSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Role>? Roles { get; set; }

        public async Task<List<Role>> GetRoles()
        {
            Roles = await _context.Role.ToListAsync();
            return Roles;
        }

        public async Task<Role> GetRole(int ID)
        {
            Role role = await _context.Role.FindAsync(ID);
            return role;
        }

        public async Task<Role> CreateRole(Role role)
        {
            _context.Role.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateRole(Role role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return role;
        }
    }
}
