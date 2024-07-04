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
    public class StaffSQLRepository : IStaffSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public StaffSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Staff>? Staff { get; set; }

        public async Task<List<Staff>> GetStaff()
        {
            Staff = await _context.Staff.ToListAsync();
            return Staff;
        }

        public async Task<Staff> GetStaff(int ID)
        {
            Staff staff = await _context.Staff.FindAsync(ID);
            return staff;
        }

        public async Task<Staff> CreateStaff(Staff staff)
        {
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task<Staff> UpdateStaff(Staff staff)
        {
            _context.Entry(staff).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task<int> DeleteStaff(int ID)
        {
            Staff staff = await _context.Staff.Where(cs => cs.ID == ID).FirstOrDefaultAsync();

            staff.Email = null;
            staff.Password = null;
            staff.PasswordSalt = null;
            staff.Prefix = null;
            staff.FirstName = null;
            staff.LastName = null;
            staff.AccountLocked = null;
            staff.AccountLockedEnd = null;
            staff.Active = false;

            _context.Entry(staff).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
