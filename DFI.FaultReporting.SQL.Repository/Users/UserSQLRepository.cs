using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Users
{
    public class UserSQLRepository : IUserSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public UserSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<User>? Users { get; set; }

        public async Task<List<User>> GetUsers()
        {
            Users = await _context.User.ToListAsync();
            return Users;
        }

        public async Task<User> GetUser(int ID)
        {
            User user = await _context.User.FindAsync(ID);
            return user;
        }

        public async Task<User> CreateUser(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<int> DeleteUser(int ID)
        {
            User user = await _context.User.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
