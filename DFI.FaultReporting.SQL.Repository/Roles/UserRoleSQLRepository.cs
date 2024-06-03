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
    public class UserRoleSQLRepository : IUserRoleSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public UserRoleSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<UserRole>? UserRoles { get; set; }

        public async Task<List<UserRole>> GetUserRoles()
        {
            UserRoles = await _context.UserRole.ToListAsync();
            return UserRoles;
        }

        public async Task<UserRole> GetUserRole(int ID)
        {
            UserRole userRole = await _context.UserRole.FindAsync(ID);
            return userRole;
        }

        public async Task<UserRole> CreateUserRole(UserRole userRole)
        {
            _context.UserRole.Add(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task<UserRole> UpdateUserRole(UserRole userRole)
        {
            _context.Entry(userRole).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task<int> DeleteUserRole(int ID)
        {
            UserRole userRole = await _context.UserRole.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.UserRole.Remove(userRole);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
