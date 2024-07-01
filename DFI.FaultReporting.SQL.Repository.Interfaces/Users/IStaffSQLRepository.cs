using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Users
{
    public interface IStaffSQLRepository
    {
        Task<List<Staff>> GetStaff();

        Task<Staff> GetStaff(int ID);

        Task<Staff> CreateStaff(Staff staff);

        Task<Staff> UpdateStaff(Staff staff);

        Task<int> DeleteStaff(int ID);
    }
}
